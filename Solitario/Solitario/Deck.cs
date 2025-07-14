using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solitario
{
    internal class Deck
    {
        public List<Card> cardList { get; set; }

        public Deck()
        {
            cardList = new List<Card>();
            creaDeck();
            mischiaDeck();
        }

        private void creaDeck()
        {
            string[] semi = { "♥", "♣", "♠", "♦" };
            for (int i = 1; i <= 13; i++)
            {
                foreach (var seme in semi)
                {
                    cardList.Add(new Card(i, seme, false));
                }
            }
        }

        private void mischiaDeck()
        {
            Random rand = new Random();
            cardList = cardList.OrderBy(x => rand.Next()).ToList();
        }

        public Card creaCarta()
        {
            if (cardList.Count > 0)
            {
                Card carta = cardList[0];
                cardList.RemoveAt(0);
                return carta;
            }
            return null;
        }
    }
}
