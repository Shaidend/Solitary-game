using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solitario
{
    internal class Card
    {
        public int valore { get; set; }
        public string seme { get; set; }
        public bool posizione { get; set; } // true = scoperta, false = coperta

        public Card(int valore, string seme, bool posizione = false)
        {
            this.valore = valore;
            this.seme = seme;
            this.posizione = posizione;
        }
    }
}
