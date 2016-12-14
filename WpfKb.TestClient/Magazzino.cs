using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TOUCH_BOX
{
    public class Magazzino
    {
        private string _Sett;
        private string _Piano;
        private string _Post;
        private string _Article;
        private string _Modello;
        private string _DS;
        private string _qty_ubicato;
        private string _qty_prelievo;

        public string Sett
        {
            get { return _Sett; }
            set { _Sett = value; }
        }

        public string Piano
        {
            get { return _Piano; }
            set { _Piano = value; }
        }

        public string Post
        {
            get { return _Post; }
            set { _Post = value; }
        }

        public string Article
        {
            get { return _Article; }
            set { _Article = value; }
        }

        public string Modello
        {
            get { return _Modello; }
            set { _Modello = value; }
        }

        public string DS
        {
            get { return _DS; }
            set { _DS = value; }
        }

        public string Qty_ubicato
        {
            get { return _qty_ubicato; }
            set { _qty_ubicato = value; }
        }

        public string Qty_prelievo
        {
            get { return _qty_prelievo; }
            set { _qty_prelievo = value; }
        }


    }

}
