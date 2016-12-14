using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

namespace TOUCH_BOX
{   /// <summary>
    /// Logica di interazione per Articolo.xaml
    /// </summary>
    public partial class Articolo : Window
    {
        public List<Magazzino> items;

        public Articolo()
        {
            InitializeComponent();
            set_list_source();               
        }

        public void set_list_source()
        {
            items = new List<Magazzino>();
            try
            {
                // deve scaricare le tipologie di Scarto da AS400
                DBHandler _dbh400 = new DBHandler();

                String _query400 = "SELECT USUSET, USPIAN, USPOST, USCDAR, USDMOD, USLEDS, USQTUB FROM ZBAUS00f WHERE USQTUB <> 0 AND USCDAR = '" + App.Current.Properties["id_articolo"].ToString() + "'"; 
                // App.Current.Properties["id_terminale"] + "' AND PTLOTO = '" + App.Current.Properties["Articolo"] 
                
                DataTable _dt = _dbh400.ExecuteShot(_query400);
                
                foreach (DataRow _dr in _dt.Rows)
                {
                    String _DS = _dr[5].ToString(); if (_DS.Length == 1) { _DS += "X"; }
                    items.Add(new Magazzino() { Sett = _dr[0].ToString(), Piano = _dr[1].ToString(), Post = _dr[2].ToString(), Article = _dr[3].ToString(), Modello = _dr[4].ToString(), DS = _DS, Qty_ubicato = _dr[6].ToString() }); 
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            ListaMagazzini.ItemsSource = items;
            
        }

        private void btn_salva_Click(object sender, RoutedEventArgs e)
        {
            DBHandler _dbh400 = new DBHandler();
            try
            {
                foreach (Magazzino _mg in items)
                {
                    
                }
            }
            catch (Exception ex)
            {
 
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Keyboard.IsOpen = true;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
    }


}
