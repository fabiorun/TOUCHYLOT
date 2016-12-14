using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TOUCH_BOX
{
    public class Domanda
    {
        private string _id_domanda;
        private string _testo;
        private string _risposta;

        public string ID_Domanda
        {
            get { return _id_domanda; }
            set { _id_domanda = value; }
        }

        public string Testo
        {
            get { return _testo; }
            set { _testo = value; }
        }

        public string Risposta
        {
            get { return _risposta; }
            set { _risposta = value; }
        }

    }
}
