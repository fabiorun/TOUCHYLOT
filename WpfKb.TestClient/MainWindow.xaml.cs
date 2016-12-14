using System;
using System.Data;
using System.Collections.Generic; 
using System.Windows;
using System.Windows.Controls; 
using System.Windows.Input;
using System.Windows.Media; 
using BAR_controls;
using System.IO;
using MaterialMenu;

namespace TOUCH_BOX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Domanda> domande;
        public Lotto LottoCurrent;  
        public bool _IS_loaded = false; //lo usiamo esattamente come il post back di ASP, in questo modo riusciamo ad evitare il salvataggio già in fase di binding iniziale dei box di scarto
        public bool _color_changed = false; 
        public Int32[] _gruppi_total = new Int32[] { 0, 0, 0, 0, 0 };
        public Boolean is_questionario_completed = true; // serve a capire se il questionario è già stato completato in una precedente sessione di lavoro

        public MainWindow()
        { 
            InitializeComponent();
            //popup_riapri.HorizontalOffset = SystemParameters.PrimaryScreenWidth  - 250 / 2;
            //popup_riapri.VerticalOffset = SystemParameters.PrimaryScreenHeight - 200 / 2;
        
            load_tipologie_scarto("GR");

            txt_lotto.Text = App.Current.Properties["lotto"].ToString() + " - " + App.Current.Properties["lotto_description"].ToString();
            txt_utente.Text = App.Current.Properties["operatore_nome"].ToString();
            txt_qty.Text = App.Current.Properties["lotto_qty"].ToString() + " - " + App.Current.Properties["lotto_unit"].ToString();
            Double _qty;
            if (App.Current.Properties["lotto_qty"] != null) { _qty = Double.Parse(App.Current.Properties["lotto_qty"].ToString()); } else { _qty = 0; }
            txt_qty.Text += " | " +  (_qty * 2).ToString() + " - Pezzi";
            txt_terminale.Text = App.Current.Properties["id_terminale"].ToString();
            txt_coda.Text = App.Current.Properties["id_articolo"].ToString();
            save_log("[lotto Aperto][totale:" + Convert.ToString(get_totals()) + "]", "TRACK");
            try
            {
                String _lotto_stato = App.Current.Properties["lotto_status"].ToString();
                if (_lotto_stato == "A")
                {
                    popup_colori.IsOpen = true;
                }
            }
            catch (Exception ep)
            { }
            apri_questionario();
        }

        public void load_tipologie_scarto(string _colore)
        {
            try
            {
                LottoCurrent = new Lotto(_colore, SystemParameters.PrimaryScreenHeight.ToString()); // Quì vengono caricati i dati dal file o da AS400, a secona dello stato del lotto 

                TipoScartoList.ItemsSource = null; // dobbiamo dissociare la sorgente dati prima di poterla ricaricare
                TipoScartoList.Items.Clear();
                TipoScartoList.ItemsSource = LottoCurrent.Items;
               
                switch (_colore)
                {
                    case "GR":
                        button_colore.Background = Brushes.Red;
                        button_colore.Foreground = Brushes.White;
                        break;
                    case "GV":
                        button_colore.Background = Brushes.Green;
                        button_colore.Foreground = Brushes.Black;
                        break;
                    case "GG":
                        button_colore.Background = Brushes.Yellow;
                        button_colore.Foreground = Brushes.Black;
                        break;
                    case "GB":
                        button_colore.Background = Brushes.Blue;
                        button_colore.Foreground = Brushes.White;
                        break;
                }
                txt_file_closed.Visibility = Visibility.Collapsed;
                btn_riapri.Visibility = Visibility.Collapsed;
                if (App.Current.Properties["lotto_status"].ToString().ToLower() == "c")
                { 
                    txt_file_closed.Visibility = Visibility.Visible;
                    btn_riapri.Visibility = Visibility.Visible;
                }
                else
                { 
                    txt_file_closed.Visibility = Visibility.Collapsed;
                    btn_riapri.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("load_tipologie_scarto(): " + ex.Message);
            }

        } 
     
        public void set_tipologie_scarto(string _colore)
        {
            _IS_loaded = false;
            try
            {
                foreach (Lotto_box lb in TipoScartoList.Items)
                {
                    if (lb.GruppoColore == _colore)
                    {
                        lb.ShowBox = true;
                    }
                    else
                    {
                        lb.ShowBox = false;
                    }
                }
                TipoScartoList.ItemsSource = null; // dobbiamo dissociare la sorgente dati prima di poterla ricaricare
                //TipoScartoList.Items.Clear();
                TipoScartoList.ItemsSource = LottoCurrent.Items;
                 
                switch (_colore)
                {
                    case "GR":
                        button_colore.Background = Brushes.Red;
                        button_colore.Foreground = Brushes.White;
                        break;
                    case "GV":
                        button_colore.Background = Brushes.Green;
                        button_colore.Foreground = Brushes.Black;
                        break;
                    case "GG":
                        button_colore.Background = Brushes.Yellow;
                        button_colore.Foreground = Brushes.Black;
                        break;
                    case "GB":
                        button_colore.Background = Brushes.Blue;
                        button_colore.Foreground = Brushes.White;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _IS_loaded = true;
            set_totals();
        } 
         
        private void keyboard_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.IsOpen = true;
        }

        private void ScartoBox_ScartoBoxGotFocus(object sender, EventArgs e)
        {
            BAR_controls.ScartoBox _box = (BAR_controls.ScartoBox)sender;
            if (_box.TipoScartoValue == "0") { _box.TipoScartoValue = ""; }
            Keyboard.IsOpen = true;
        }

        private void ScartoBox_TextChanged(object sender, RoutedEventArgs e)
        {
            ScartoBox _box = (ScartoBox)sender;
            //TextBox _box_text = e.Source
            _box.TipoScartoValue = ((TextBox)e.Source).Text;

            save_change(); // salviamo i cambiamenti.
        }

        public void set_totals()
        {
            _gruppi_total = new Int32[] { 0, 0, 0, 0, 0 };
            Int32 IndexGV = 0; Int32 Index = 0;
            foreach(Lotto_box _ts in TipoScartoList.Items)
            {
                Index += 1;
                try
                {
                    _gruppi_total[IndexGV] += Convert.ToInt32(_ts.TipoValue);
                }
                catch (Exception ep) 
                {
                    //non fare niente, siamo nella situazione in cui il tasto del o il tasto canc è stato premuto e non ci sono caratteri 
                }
                if (Index == LottoCurrent.TipologieScartoList.Count) { IndexGV += 1; Index = 0; }

            }

            _gruppi_total[4] = _gruppi_total[0] + _gruppi_total[1] + _gruppi_total[2] + _gruppi_total[3];

            txt_tot_red.Text = _gruppi_total[0].ToString();
            txt_tot_green.Text = _gruppi_total[1].ToString();
            txt_tot_yellow.Text = _gruppi_total[2].ToString();
            txt_tot_blue.Text = _gruppi_total[3].ToString();
            txt_tot_all.Text = _gruppi_total[4].ToString();
            txt_totale.Text = _gruppi_total[4].ToString();

        }

        public Int32 get_totals()
        {
            _gruppi_total = new Int32[] { 0, 0, 0, 0, 0 };
            Int32 IndexGV = 0; Int32 Index = 0;
            try
            {
                foreach (Lotto_box _ts in TipoScartoList.Items)
                {
                    Index += 1;
                    try
                    {
                        _gruppi_total[IndexGV] += Convert.ToInt32(_ts.TipoValue);
                    }
                    catch (Exception ep) 
                    { 
                        //non fare nulla, è il momento in cui l'utente ha premuto il tasto del o cancel
                        //per questo non ci sono carattteri e la conversione andrebbe in errore
                    }
                    if (Index == LottoCurrent.TipologieScartoList.Count) { IndexGV += 1; Index = 0; }
                }
                _gruppi_total[4] = _gruppi_total[0] + _gruppi_total[1] + _gruppi_total[2] + _gruppi_total[3];
            }
            catch (Exception ep) { throw new Exception("get_totals()" + ep.Message); }
            return _gruppi_total[4];
        }
        public void log_totals()
        {
            _gruppi_total = new Int32[] { 0, 0, 0, 0, 0 };
            Int32 IndexGV = 0; Int32 Index = 0;
            try
            {
                foreach (Lotto_box _ts in TipoScartoList.Items)
                {
                    Index += 1;
                    try
                    {
                        _gruppi_total[IndexGV] += Convert.ToInt32(_ts.TipoValue);
                    }
                    catch (Exception ep)
                    {
                        //non fare nulla, è il momento in cui l'utente ha premuto il tasto del o cancel
                        //per questo non ci sono carattteri e la conversione andrebbe in errore
                    }
                    if (Index == LottoCurrent.TipologieScartoList.Count) { IndexGV += 1; Index = 0; }
                }

                save_log("[lotto Completato][totale gruppo Rosso:" + Convert.ToString(_gruppi_total[0]) + "]", "TRACK");
                save_log("[lotto Completato][totale gruppo Verde:" + Convert.ToString(_gruppi_total[1]) + "]", "TRACK");
                save_log("[lotto Completato][totale gruppo Giallo:" + Convert.ToString(_gruppi_total[2]) + "]", "TRACK");
                save_log("[lotto Completato][totale gruppo Blue:" + Convert.ToString(_gruppi_total[3]) + "]", "TRACK");
            }
            catch (Exception ep) { throw new Exception("log_totals()" + ep.Message); }
            

        }
        private void ScartoBox_incrementa_Click(object sender, EventArgs e)
        {
            ScartoBox _box = (ScartoBox)sender;
            try
            {
                if (_box.TipoScartoValue == "") { _box.TipoScartoValue = "0"; }
                _box.TipoScartoValue = (Convert.ToInt32(_box.TipoScartoValue) + 1).ToString();
                save_change(); //salviamo i cambiamenti : se e solo se il lotto status = 0 -> ovvero è aperto
            }
            catch (Exception ep) { save_log("ERROR:ScartoBox_incrementa_Click():" + ep.Message); }
        }

        private void ScartoBox_decrementa_Click(object sender, EventArgs e)
        {
            ScartoBox _box = (ScartoBox)sender;
            try
            {

                if (_box.TipoScartoValue == "") { _box.TipoScartoValue = "0"; }
                if ((Convert.ToInt32(_box.TipoScartoValue) - 1) >= 0)
                {
                    _box.TipoScartoValue = (Convert.ToInt32(_box.TipoScartoValue) - 1).ToString();
                }
                else
                {
                    _box.TipoScartoValue = "0";
                }
                save_change(); //salviamo i cambiamenti
            }
            catch (Exception ep) { save_log("ERROR:ScartoBox_decrementa_Click():" + ep.Message); }
        }

        private void btn_colore_Click(object sender, RoutedEventArgs e)
        {
            set_tipologie_scarto(((Button)sender).CommandParameter.ToString());
            popup_colori.IsOpen = false;
        }
        
        private void btn_completa_Click(object sender, RoutedEventArgs e)
        {
            //andiamo a completare il Lotto corrente; (il file tmp verrà salvato in .dat)
            LottoCurrent.completa();
            log_totals(); // salva i log con il totale per colore

            Start_lotto _cambia_lotto = new Start_lotto(this);
            App.Current.MainWindow = _cambia_lotto;
            this.Close();
            _cambia_lotto.Show();
            
        }
         
        private void save_change()
        {// is loaded ci permette di prevenire che l'app vada a salvare in fase di caricamento dei box
            if (_IS_loaded) 
            {// possiamo salvare solo Lotti aperti, altrimenti dobbiamo far riaprire il lotto-> quindi proporremo il popup di riapetura nel ramo Else
                if (App.Current.Properties["lotto_status"].ToString() == "A")
                {
                    if (IS_LT_QTY())
                    {
                        // salva la sessione del lotto, in uno stato che non è ultimato
                        // Il lotto potrà essere riaperto in un secondo momento
                        LottoCurrent.SalvaFile();

                        // dopo aver salvato, andiamo a ricalcolare i totali:
                        set_totals();  // ricalcoliamo i totali.
                    }
                    else
                    {
                        MessageBox.Show("ATTENZIONE: hai superato la quantità del LOTTO!");
                    }
                }
                else
                {
                    popup_riapri.IsOpen = true;
                    if (App.Current.Properties["lotto_status"].ToString() != "A")
                    {
                        btn_riapri_pop.Visibility = System.Windows.Visibility.Collapsed;
                        lb_can_be_open.Text = "Questo lotto non può essere riaperto.";
                    }
                    else
                    {
                        btn_riapri_pop.Visibility = System.Windows.Visibility.Visible;
                        lb_can_be_open.Text = "Le modifiche possono essere salvate riaprendolo";
                    }
                }
            }
        }

        public bool IS_LT_QTY() // IS LESS THAN Quantity
        {
            //torna true se il totale degli scarti non ha superato la Quantità.
            Int32 _qty_pezzi;
            try
            {
                string lotto_qty = App.Current.Properties["lotto_qty"].ToString();
                _qty_pezzi = (Int32)(double.Parse(lotto_qty) * 2);
                if (_qty_pezzi > get_totals())
                { return true; }
                else 
                { return false; }
            }
            catch (Exception ep)
            {
                throw new Exception("IS_LT_QTY():" + ep.Message);
            }
        }


        private void TestWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _IS_loaded = true;
            set_totals();
        }
         
 
        private void btn_riapri_Click(object sender, RoutedEventArgs e)
        {
            // permette di RIAPRIRE un lotto completato se e solo se non è chiuso su AS400
            // viene chiamato dal tasto riapri sulla barra, e dal tasto iapri sul popup di warning del lotto chiuso popup_riapri
            
            String ID_lotto = App.Current.Properties["lotto"].ToString();
            try
            {
                DBHandler _dbh = new DBHandler();
                _dbh.ExecuteNonQueryShot("UPDATE [lot] SET [lot_status]='A' WHERE  [ID_lot] = '" + ID_lotto + "'");
            }
            catch (Exception ep)
            {  
                save_log("btn_riapri_Click():" + ep.Message); 
            }
            // ricarichiamo il lotto corrente
            Start_lotto _cambia_lotto = new Start_lotto(this, 1);
            App.Current.MainWindow = _cambia_lotto;
            this.Close();
            //_cambia_lotto.Show();
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
                        sw.WriteLine("DATE LOG:" + DateTime.Today.ToString() + " | LOTTO: " + App.Current.Properties["lotto"].ToString() + " | " + _type + ": " + _msg);
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
            catch (Exception ep)
            {
                // NON FARE NULLA, IL LOG NON DEVE IMPATTARE IL FLUSSO...SE VA IN ECCEZIONE AMEN
            }
        }

        private void btn_close_pop_Click(object sender, RoutedEventArgs e)
        {
            popup_riapri.IsOpen = false;
        }
         

        private void TestWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //main_grid.Height = e.NewSize.Height - 20;
            //pup_colori.Height = e.NewSize.Height - 20;
            //popup_colori.Height = this.Height; // Height="{Binding Path=ActualHeight, ElementName=main_grid}" 
            //popup_colori.Height = SystemParameters.PrimaryScreenHeight;
            //popup_colori.Width = SystemParameters.PrimaryScreenWidth - 30;
        }

        private void btn_procedi_Click(object sender, RoutedEventArgs e)
        {
            questionario_warning.Visibility = System.Windows.Visibility.Collapsed;
            // controlliamo che tutte le domande abbiano una risposta (si,no)
            foreach (Domanda _d in domande)
            {
                if (_d.Risposta == "")
                {
                    // non ci siamo 
                    questionario_warning.Visibility = System.Windows.Visibility.Visible;
                    return; //il popup resterà visibile
                }
            }

            try 
            {
                DBHandler _dbh = new DBHandler();
                foreach (Domanda _d in domande)
                {
                    string _sql = "INSERT INTO TABQSR0F(TRLOTO,TRIDQS,TRIDRE,TRDATA,TRORA,TROPER,TRIDTE) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')";
                    
                    _sql = string.Format(_sql, LottoCurrent.ID_lotto, _d.ID_Domanda, _d.Risposta, DateTime.Today.ToString("dMyyyy"), DateTime.Now.ToString("HHmmss"), App.Current.Properties["operatore_id"].ToString(), App.Current.Properties["id_terminale"].ToString());
                    _dbh.ExecuteNonQueryShot("DELETE FROM TABQSR0F WHERE TRIDQS='" + _d.ID_Domanda  + "'");
                    _dbh.ExecuteNonQueryShot(_sql);
                }
            }
            catch (Exception ep) 
            { 

            }
            popup_domande_lotto.IsOpen = false;
            is_questionario_completed = true;
            btn_question_close.Visibility = System.Windows.Visibility.Visible;

            CustomMenu.Toggle();
        }

        private void apri_questionario()
        {
            set_questionario();
            if (!is_questionario_completed)
            {
                popup_domande_lotto.IsOpen = true;
                btn_question_close.Visibility = System.Windows.Visibility.Collapsed;
            }
            else {
                btn_question_close.Visibility = System.Windows.Visibility.Visible; // se si apre volontariamente il popup Questionario lo si può anche richiudere
            }
        }

        public void set_questionario()
        {
            domande = new List<Domanda>();
            try
            {
                //String _sql = "SELECT [ID_domanda],[ID_lotto],[TestoDomanda],[Risposta] FROM [Domande] WHERE [ID_lotto]= '" + App.Current.Properties["lotto"].ToString() + "'";
                String _sql = "SELECT [ID_question] ,[_question] ,[_answer] FROM [dbo].[Audit] where _lot = '" + App.Current.Properties["lotto"].ToString() + "'";
                DBHandler  _dbh = new DBHandler();
                DataTable _dt = _dbh.ExecuteShot(_sql);
                
                foreach (DataRow _dr in _dt.Rows)
                {
                    domande.Add(new Domanda() { ID_Domanda = _dr[0].ToString(), Testo = _dr[1].ToString(), Risposta = _dr[2].ToString() });
                    if (_dr[2].ToString() == "") { 
                        is_questionario_completed = false; 
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            ListaDomande.ItemsSource = domande;
        }
         

        private void CloseQuesstionarioCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            popup_domande_lotto.IsOpen = false;
            CustomMenu.Toggle();
        }
        private void Menu_OnMouseDown_completa(object sender, MouseButtonEventArgs e)
        {

            //andiamo a completare il Lotto corrente; (il file tmp verrà salvato in .dat)
            LottoCurrent.completa();
            log_totals(); // salva i log con il totale per colore

            Start_lotto _cambia_lotto = new Start_lotto(this);
            App.Current.MainWindow = _cambia_lotto;
            this.Close();
            _cambia_lotto.Show();
        }

        private void Menu_OnMouseDown_changelot(object sender, MouseButtonEventArgs e)
        {
            Start_lotto _cambia_lotto = new Start_lotto(this);
            App.Current.MainWindow = _cambia_lotto;
            this.Close();
            _cambia_lotto.Show();
            popup_domande_lotto.IsOpen = true;
        }

        private void Menu_OnMouseDown_audit(object sender, MouseButtonEventArgs e)
        { 
            popup_domande_lotto.IsOpen = true;
        }

        private void Menu_OnMouseDown_quit(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CustomMenu.Toggle();
        }

        private void Menu_OnMouseDown_storage(object sender, MouseButtonEventArgs e)
        {
            ////Apriamo la finestra article
            //Articolo _articolo = new Articolo();
            //_articolo.ShowDialog();
            MessageBox.Show("NOT YET IMPLEMENTED!");
        }

        private void CustomMenu_Loaded(object sender, RoutedEventArgs e)
        {

        }
         
    }

}
