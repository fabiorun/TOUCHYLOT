using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;

namespace TOUCH_BOX
{
    public class Lotto
    {
        public String[] _gruppi = new String[] { "GR", "GV", "GG", "GB" };
        public String _separator = " ";
        public String ID_lotto = App.Current.Properties["lotto"].ToString();
        public String ID_terminale = App.Current.Properties["id_terminale"].ToString();

        public String Gruppo_colore_select;
        public List<Lotto_box> Items;
        public List<tipo_scarto> TipologieScartoList; // Tipo_scarto serve a rappresentare le tipologie di scarto caricate dal file configurazione.xml
        public String Lotto_stato;
        public string _current_color = "red";
        public String _screen_height;

        public Lotto(String _gruppo_select, String screen_height) // l'ID del LOTTO scelto nella login gli viene passato tramite il parametro di configurazione: ID_lotto
        {
            _screen_height = screen_height;

            try
            {
                Gruppo_colore_select = _gruppo_select;
                Items = new List<Lotto_box>();
                set_tipologie_di_scarto();
                
                DBHandler _dbh = new DBHandler();
                DataTable _dt = new DataTable();
                try
                {
                    _dt = _dbh.ExecuteShot("SELECT [ID_lot],[ID_terminal],[ID_color],[ID_waste_type],[date_reg],[waste_qty] FROM [dbo].[lot_waste] WHERE [ID_lot] = '"  + ID_lotto + "' AND [ID_terminal] = " + App.Current.Properties["id_terminale"].ToString());
                    if(_dt.Rows.Count > 0)
                    { 
                        Int32 _count_row = 0;
                        Int32 _index_gruppo = 0;
                        foreach (DataRow _dr in _dt.Rows)
                        {
                            _count_row += 1;
                            add_box(_dr["ID_waste_type"].ToString(), _dr["waste_qty"].ToString(), _gruppi[_index_gruppo]);
                            if (_count_row == TipologieScartoList.Count) { _count_row = 0; _index_gruppo += 1; }
                        }
                        return;
                    }
                }
                catch (Exception ep)
                {
                    
                }

                // se il flusso d'esecuzione arriva quì vuol dire che non ci sono sessioni aperte per questo lotto
                // per sessioni aperte intendiamo una situazione intemedia salvata su un file.
                // quindi andiamo a pescare i dati da AS400
                load_boxes_initial();
            }
            catch (Exception ex)
            {
                throw new Exception("Lotto(): " + ex.Message);
            }
        }

        public void set_tipologie_di_scarto()
        {
            TipologieScartoList = new List<tipo_scarto>();

            DBHandler _dbh = new DBHandler();
            DataTable _dt = new DataTable();

            try
            {
                _dt = _dbh.ExecuteShot("SELECT [ID_waste_type],[waste_type] FROM [dbo].[waste_type] ");
                // let's read the waste types

                foreach (DataRow _dr in _dt.Rows)
                {
                    TipologieScartoList.Add(new tipo_scarto(_dr[0].ToString(), _dr[1].ToString()));
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("ERRORE:" + ex.Message);
            }
        }

        public String find_tiposcarto_name(String _ID_tipo_scarto)
        {

            foreach (tipo_scarto _tp in TipologieScartoList)
            {
                if (_tp.TipoID == _ID_tipo_scarto)
                {
                    return _tp.TipoNome;
                }
            }
            return null;
        }

        public void add_box(String _ID_tiposcarto, String _valore, String _gruppoColore)
        {

            Int32 _scarto_value;
            try { _scarto_value = Int32.Parse(_valore); }
            catch (Exception ep) { _scarto_value = 0; }
            _current_color = get_next_color(_current_color);

            Items.Add(new Lotto_box(_ID_tiposcarto, find_tiposcarto_name(_ID_tiposcarto), _scarto_value.ToString(), _gruppoColore, _current_color, Gruppo_colore_select, _screen_height));
        }
               

        public void load_boxes_initial()
        {
            try
            {
                foreach (String _gruppo in _gruppi)
                {
                    foreach (tipo_scarto _ts in TipologieScartoList)
                    {
                        add_box(_ts.TipoID, "0", _gruppo);
                    }
                }
                SalvaFile(); // generiamo il file per il lotto appena aperto.
            }
            catch (Exception ex)
            {
                throw new Exception("load_boxes_initial(): " + ex.Message);
            }

        }



        public void load_boxes_from_as400()
        {
            try
            {
                // deve scaricare le tipologie di Scarto da AS400
                DBHandler _dbh400 = new DBHandler();

                String _query400 = "SELECT PTCODS, PTQTAS , PTLOTO,PTGRUP, PTDESA FROM STOPTS0F WHERE PTTERM = '" + App.Current.Properties["id_terminale"] + "' AND PTLOTO = '" + App.Current.Properties["lotto"] + "'";// order by PTGRUP DESC,PTCODS";

                DataTable _dt = _dbh400.ExecuteShot(_query400);

                foreach (DataRow _dr in _dt.Rows)
                {
                    add_box(_dr[0].ToString(), _dr[1].ToString(), _dr[3].ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        public String get_next_color(String _current_color)
        {
            switch (_current_color)
            {
                case "red":
                    return "#00affe";
                case "#00affe":
                    return "#1de9b6";
                case "#1de9b6":
                    return "#ff8f03";
                case "#ff8f03":
                    return "#f30157";
                case "#f30157":
                    return "#c6ff00";
                case "#c6ff00":
                    return "olive";
                case "olive":
                    return "#795547";
                case "#795547":
                    return "#4cb050";
                case "#4cb050":
                    return "#607e89";
                case "#607e89":
                    return "red";
            }
            return "non ariverà mai quì per costruzione";
        }

        public void SalvaFile()
        { 
            try
            { 
                DBHandler _dbh = new DBHandler();
                _dbh.ExecuteNonQueryShot("DELETE FROM [dbo].[lot_waste] WHERE [ID_lot]='" + ID_lotto + "' AND [ID_terminal]=" + App.Current.Properties["id_terminale"].ToString());
                foreach (Lotto_box _lb in Items)
                {
                    String _sql = "INSERT INTO [dbo].[lot_waste] ([ID_lot] ,[ID_terminal] ,[ID_color] ,[ID_waste_type] ,[date_reg] ,[waste_qty]) ";
                    _sql += " VALUES('" + ID_lotto + "', " + App.Current.Properties["id_terminale"].ToString() + ", '" + _lb.GruppoColore + "', " + _lb.TipoID + ", '" + DateTime.Today.ToString("yyyyMMdd") + "', " + _lb.TipoValue + ")";
                    _dbh.ExecuteNonQueryShot(_sql);
                }

            }
            catch (Exception ex)
            { }
        }


        public void completa()
        {  
            try
            {
                DBHandler _dbh = new DBHandler();
                _dbh.ExecuteNonQueryShot("UPDATE lot SET lot_status='C' WHERE ID_lot='" + ID_lotto + "'");
            }
            catch (Exception ex)
            {
                throw new Exception("completa():" + ex.Message);
            }
        }
    }

    public class Lotto_box
    {
        private String _TipoID;
        private String _TipoNome; // da capire se bisogna recuperarlo su AS400
        private String _TipoValue;
        private String _BoxColore;
        private String _gruppoColore;
        private String _ScreenTypeBox;//x:Static SystemParameters.PrimaryScreenHeight
        private bool _showbox;
        public String TipoID
        {
            get { return _TipoID; }
            set { _TipoID = value; }
        }

        public String TipoNome
        {
            get { return _TipoNome; }
            set { _TipoNome = value; }
        }

        public String TipoValue
        {
            get { return _TipoValue; }
            set { _TipoValue = value; }
        }

        public String GruppoColore
        {
            get { return _gruppoColore; }
            set { _gruppoColore = value; }
        }

        public String BoxColore
        {
            get { return _BoxColore; }
            set { _BoxColore = value; }
        }

        public bool ShowBox
        {
            get { return _showbox; }
            set { _showbox = value; }
        }
        public String ScreenTypeBox
        {
            get { return _ScreenTypeBox; }
            set { _ScreenTypeBox = value; }
        }

        public Lotto_box(String _ID_tiposcarto, String _tiposcarto_nome, String _valore, String _gruppocolore, String _box_colore, String gruppo_colore_select, String _screen_type_box)
        {
            TipoID = _ID_tiposcarto;
            TipoNome = _tiposcarto_nome;
            TipoValue = _valore;
            ScreenTypeBox = _screen_type_box;
            GruppoColore = _gruppocolore; // questo è il gruppo colore del lotto che è riportato su AS400 (EX: GB;GG;GR;GV)
            BoxColore = _box_colore; // questo è il colore con cui viene renderizzato il BOX (l'interfaccia) - > solo una questione estetica
            if (TipoValue == "") { TipoValue = "0"; }
            if (GruppoColore == gruppo_colore_select)
            {
                ShowBox = true;
            }
            else
            {
                ShowBox = false;
            }
        }

        public String save_txt(int _index)
        {
            String _month = "";
            string _s_index = "";
            if (DateTime.Today.Month.ToString().Length == 1) { _month = "0" + DateTime.Today.Month.ToString(); } else { _month = DateTime.Today.Month.ToString(); }
            if (_index.ToString().Length == 1) { _s_index = "0" + _index.ToString(); } else { _s_index = _index.ToString(); }

            String _giorno = DateTime.Today.Day.ToString(); if (_giorno.Length == 1) { _giorno = "0" + _giorno; }
            String _today = _giorno + _month + DateTime.Today.Year.ToString().Substring(2, 2);
            String _value = myStaff("0000000", 7 - TipoValue.Length, TipoValue.Length, TipoValue);

            return App.Current.Properties["id_terminale"].ToString() + ' ' + _today + ' ' + App.Current.Properties["lotto"].ToString() + ' ' + _s_index + ' ' + _value + ' ' + TipoID;//  +' ' + TipoNome; 
        }

        public String myStaff(String str, int start, int length, String replacement)
        {
            return str.Substring(0, start) + replacement + str.Substring(start + length);
        }

    }

    public class tipo_scarto
    {
        // Tipo_scarto serve a rappresentare le tipologie di scarto caricate dal file configurazione.xml
        // non è da confondere con gli oggetti lotto_box
        private String _TipoID;
        private String _TipoNome;

        public String TipoID
        {
            get { return _TipoID; }
            set { _TipoID = value; }
        }

        public String TipoNome
        {
            get { return _TipoNome; }
            set { _TipoNome = value; }
        }


        public tipo_scarto(String _ID_tiposcarto, String _tiposcarto_nome)
        {
            TipoID = _ID_tiposcarto;
            TipoNome = _tiposcarto_nome;
        }
    }
}



