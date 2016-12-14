using System;
using System.Configuration;
using System.Windows; 
using System.Windows.Media; 
using System.Data; 
using System.IO;

/*  
    App.Current.Properties["id_terminale"] 
    App.Current.Properties["gruppo_colore"] 
    App.Current.Properties["operatore_nome"] 
    App.Current.Properties["operatore_id"] 
    App.Current.Properties["lotto"]  
    App.Current.Properties["lotto_description"] 
    App.Current.Properties["lotto_qty"] 
    App.Current.Properties["lotto_unit"] 
 *  App.Current.Properties["lotto_status"] // indica se il lotto è stato chiuso su AS400, in questo caso non si può riaprire il file .dat associato
*/

namespace TOUCH_BOX
{
    /// <summary>
    /// Logica di interazione per Start_lotto.xaml
    /// </summary>
    public partial class Start_lotto : Window
    {
        // _lotto_precedente è Valorizzzato solo quando si sta cambiando il lotto: 
        // una volta scelto il lotto, bisogna ricaricare la stessa finestra precedente
        public MainWindow _lotto_precedente;

        public int is_login_pressed = 0; // serve ad evitare ce sia premuto di continuo il tasto invio

        public Start_lotto()
        {
            InitializeComponent();
            //this.Left = (SystemParameters.PrimaryScreenWidth / 2) - (this.Width / 2); // il TOP E' imposato nello xaml
            btn_start.IsDefault = true;
            //btn_start.Focusable = false;
        }

        public Start_lotto(MainWindow _lotto_esistente,int is_reopen = 0) // viene chiamato dalla finestra principale, quando si cambia lotto, quando si riapre un lotto(is_reopen=1)
        {
            //salviamo l'ultimo operatore prima di Ripulire le variabili di Sessione:
            String _ultimo_operatore = App.Current.Properties["operatore_id"].ToString();
            String _ultimo_lotto = App.Current.Properties["lotto"].ToString();
                        
            _lotto_precedente = _lotto_esistente;
            InitializeComponent();
            btn_start.IsDefault = true;
            
            txt_operatore.Text = _ultimo_operatore;
            if (is_reopen == 1)
            {
                txt_lotto.Text = _ultimo_lotto;
                enter();
            }
        }

        public void set_config()
        {
            try
            {
                App.Current.Properties["id_terminale"] = ConfigurationManager.AppSettings["id_terminale"].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:set_config():" + ex.Message);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tb_warning.Visibility = System.Windows.Visibility.Collapsed; lb_warning.Text = "";
            txt_operatore.Background = Brushes.White;
            txt_lotto.Background = Brushes.White;
            if (is_login_pressed == 0)
            {
                enter(); // dentro enter viene reimpostato ad is_login_pressed 0
                is_login_pressed = 0; 
            }
        }

        private void enter()
        {
            // ripuliamo le imposazioni globali dato che si potrebbe stare aprendo un altro lotto
            App.Current.Properties.Clear();
            set_config();
            is_login_pressed = 1;
            try
            {
                if (login())
                {
                    if (_lotto_precedente != null)
                    {
                        _lotto_precedente.Close();
                    }

                    MainWindow _main = new MainWindow();
                    save_log("[user Login]", "TRACK");
                    App.Current.MainWindow = _main;

                    _main.Show();
                    this.Close();
                }
                //T.H.O.D.
            }
            catch (Exception ex)
            {
                is_login_pressed = 0;
                set_warning_message(ex.Message);
            } 
        }
        private bool login()
        {
            App.Current.Properties["operatore_nome"] = "Paolo Rossi";
            App.Current.Properties["operatore_id"] = txt_operatore.Text;
            DBHandler _dbh = new DBHandler();
            DataTable _dt = new DataTable();
            try
            {
                _dt = _dbh.ExecuteShot("SELECT [ID_lot] ,[lot_description] ,[lot_unit] ,[lot_qty] , [lot_status], [lot_product] FROM [dbo].[lot] WHERE [ID_lot]  = '" + txt_lotto.Text.Replace("'", "''") + "'");
                if (_dt.Rows.Count == 1)
                {
                    App.Current.Properties["lotto"] = txt_lotto.Text;
                    App.Current.Properties["lotto_description"] = _dt.Rows[0]["lot_description"].ToString();
                    App.Current.Properties["lotto_qty"] = _dt.Rows[0]["lot_qty"].ToString();
                    App.Current.Properties.Add("lotto_status", _dt.Rows[0]["lot_status"].ToString());// Blank , C -> Closed, A -> Open 
                    App.Current.Properties.Add("lotto_unit", _dt.Rows[0]["lot_unit"].ToString());
                    App.Current.Properties.Add("id_articolo", _dt.Rows[0]["lot_product"].ToString());

                    return true;
                }
            }
            catch (Exception ep)
            {
                throw new Exception("login():" + ep.Message);
            }
            return false;
        }
       

        private void txt_lotto_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_lotto.Background = Brushes.Yellow;
            KeyboardStart.IsOpen = true;
        }

        private void txt_operatore_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_operatore.Background = Brushes.Yellow;
            KeyboardStart.IsOpen = true;
        }
        private void txt_lotto_LostFocus(object sender, RoutedEventArgs e)
        {
            txt_lotto.Background = Brushes.White;
        }

        private void txt_operatore_LostFocus(object sender, RoutedEventArgs e)
        {
            txt_operatore.Background = Brushes.White;
        }

        public void set_warning_message(String _msg)
        {
            tb_warning.Visibility = Visibility.Visible;
            lb_warning.Text += _msg;
            KeyboardStart.IsOpen = false;
            tb_warning.Focusable = true;
        }

        
        public void save_log(String _msg,String _type = "ERROR")
        {
            try
            {
                String _path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\log_touch.txt";

                if (!File.Exists(_path))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(_path))
                    {
                        sw.WriteLine("DATE LOG:" + DateTime.Today.ToString() + " | LOTTO: " + App.Current.Properties["lotto"].ToString() + " | " + _type  + ": " + _msg);
                    }
                }
                else
                { // open file already created.
                    using (TextWriter tw = new StreamWriter(_path, true))
                    {
                        tw.WriteLine("DATE LOG:" + DateTime.Today.ToString() + " | LOTTO: " + App.Current.Properties["lotto"].ToString() + " | " + _type + ": " + _msg);
                    }
                }
            }
            catch (Exception EP)
            {
                // NON FARE NULLA, IL LOG NON DEVE IMPATTARE IL FLUSSO...SE VA IN ECCEZIONE AMEN
            }
        }

    }
}
