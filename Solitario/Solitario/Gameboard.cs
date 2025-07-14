using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solitario
{
    internal class MossaMemoria
    {
        public string TipoMossa { get; set; }
        public List<List<Card>> ColonneStato { get; set; }
        public List<List<Card>> PileFinaliStato { get; set; }
        public List<Card> PilaRiservaStato { get; set; }
        public Stack<Card> CartePescateStato { get; set; }
        public Card CartaSpostata { get; set; }
        public int OrigineIndex { get; set; }
        public int DestinazioneIndex { get; set; }
    }

    internal class Gameboard
    {
        private Deck deck;
        private List<List<Card>> pileFinali;
        private List<Card> pilaDiRiserva;
        private List<Card> pilaDiRiservaOriginale; // Mantiene l'ordine originale
        private Stack<Card> cartePescate;
        public List<List<Card>> colonneIniziali;

        // Per il sistema di undo
        private Stack<MossaMemoria> cronologiaMosse;

        // Per il restart
        private List<List<Card>> colonneIniziali_Backup;
        private List<Card> pilaDiRiserva_Backup;

        public Gameboard()
        {
            deck = new Deck();
            colonneIniziali = new List<List<Card>>();
            pileFinali = new List<List<Card>>();
            pilaDiRiserva = new List<Card>();
            pilaDiRiservaOriginale = new List<Card>();
            cartePescate = new Stack<Card>();
            cronologiaMosse = new Stack<MossaMemoria>();

            creazioneColonne();
            creazionePilaRiserva();
            creazionePileFinali();
            CreaBackup();
        }

        private void creazioneColonne()
        {
            for (int i = 0; i < 7; i++)
            {
                colonneIniziali.Add(new List<Card>());
            }

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    Card carta = deck.creaCarta();
                    if (carta != null)
                    {
                        if (j == i)
                        {
                            carta.posizione = true;
                        }
                        else
                        {
                            carta.posizione = false;
                        }
                        colonneIniziali[i].Add(carta);
                    }
                }
            }
        }

        private void creazionePilaRiserva()
        {
            // Aggiungo tutte le carte rimanenti alla pila di riserva
            Card carta;
            while ((carta = deck.creaCarta()) != null)
            {
                pilaDiRiserva.Add(carta);
                // Creo una copia per mantenere l'ordine originale
                pilaDiRiservaOriginale.Add(new Card(carta.valore, carta.seme, carta.posizione));
            }
        }

        private void creazionePileFinali()
        {
            for (int i = 0; i < 4; i++)
            {
                pileFinali.Add(new List<Card>());
            }
        }

        public bool PescaDallaPilaDiRiserva()
        {
            if (pilaDiRiserva.Count > 0)
            {
                SalvaStatoPerUndo("pesca");

                Card carta = pilaDiRiserva[0];
                pilaDiRiserva.RemoveAt(0);
                carta.posizione = true;
                cartePescate.Push(carta);
                return true;
            }
            return false;
        }

        // METODO AGGIORNATO - Riciclo semplificato
        public bool RiciclaPilaDiRiserva()
        {
            if (cartePescate.Count == 0)
            {
                return false; // Nessuna carta da riciclare
            }

            SalvaStatoPerUndo("riciclo");

            // Semplicemente rimetto tutte le carte pescate nella pila di riserva
            // nell'ordine inverso (così mantengono l'ordine originale quando pescate di nuovo)
            while (cartePescate.Count > 0)
            {
                Card carta = cartePescate.Pop();
                carta.posizione = false; // Le carte tornano coperte
                pilaDiRiserva.Insert(0, carta); // Inserisco all'inizio per mantenere l'ordine
            }

            return true;
        }

        public Card GetCartaVisibilePescata()
        {
            return cartePescate.Count > 0 ? cartePescate.Peek() : null;
        }

        public Card UsaCartaVisibilePescata()
        {
            return cartePescate.Count > 0 ? cartePescate.Pop() : null;
        }

        public bool MovimentoTraColonne(int origine, int destinazione, int numCarte = -1)
        {
            if (origine < 0 || origine >= 7 || destinazione < 0 || destinazione >= 7)
            {
                return false;
            }

            var colOrigine = colonneIniziali[origine];
            var colDestinazione = colonneIniziali[destinazione];

            if (colOrigine.Count == 0) return false;

            // Trova l'indice della prima carta scoperta
            int indicePartenza = colOrigine.FindIndex(c => c.posizione);
            if (indicePartenza == -1) return false;

            // Se numCarte non è specificato, sposta tutta la sequenza scoperta
            if (numCarte == -1)
            {
                numCarte = colOrigine.Count - indicePartenza;
            }

            // Verifica che ci siano abbastanza carte da spostare
            if (indicePartenza + numCarte > colOrigine.Count)
            {
                return false;
            }

            // Verifica che tutte le carte da spostare siano scoperte
            for (int i = indicePartenza; i < indicePartenza + numCarte; i++)
            {
                if (!colOrigine[i].posizione) return false;
            }

            var sequenzaDaSpostare = colOrigine.GetRange(indicePartenza, numCarte);
            Card cartaSpostata = sequenzaDaSpostare[0];

            // Verifica validità del movimento
            if (colDestinazione.Count == 0)
            {
                if (cartaSpostata.valore != 13) return false;
            }
            else
            {
                Card cartaDest = colDestinazione[colDestinazione.Count - 1];
                if (!SonoColoriAlternati(cartaSpostata.seme, cartaDest.seme)) return false;
                if (cartaSpostata.valore != cartaDest.valore - 1) return false;
            }

            // Salva stato per undo prima di eseguire la mossa
            SalvaStatoPerUndo("movimento_colonne");

            colDestinazione.AddRange(sequenzaDaSpostare);
            colOrigine.RemoveRange(indicePartenza, numCarte);

            if (colOrigine.Count > 0 && !colOrigine.Last().posizione)
                colOrigine.Last().posizione = true;

            return true;
        }

        public bool MovimentoCartaPescataAColonna(int destinazione)
        {
            if (destinazione < 0 || destinazione >= 7 || cartePescate.Count == 0)
                return false;

            Card carta = cartePescate.Peek();
            var colDestinazione = colonneIniziali[destinazione];

            if (colDestinazione.Count == 0)
            {
                if (carta.valore != 13) return false;
            }
            else
            {
                Card cartaDest = colDestinazione.Last();
                if (!SonoColoriAlternati(carta.seme, cartaDest.seme)) return false;
                if (carta.valore != cartaDest.valore - 1) return false;
            }

            // Salva stato per undo prima di eseguire la mossa
            SalvaStatoPerUndo("carta_pescata_colonna");

            colDestinazione.Add(UsaCartaVisibilePescata());
            return true;
        }

        // METODO AGGIORNATO - Pile finali flessibili
        public bool MovimentoCartaPescataAPileFinali()
        {
            if (cartePescate.Count == 0) return false;

            Card carta = cartePescate.Peek();

            // Cerca la prima pila finale adatta per questa carta
            for (int i = 0; i < pileFinali.Count; i++)
            {
                var pilaFinale = pileFinali[i];

                if (pilaFinale.Count == 0)
                {
                    // Pila vuota: accetta solo Assi
                    if (carta.valore == 1)
                    {
                        SalvaStatoPerUndo("carta_pescata_pile_finali");
                        pilaFinale.Add(UsaCartaVisibilePescata());
                        return true;
                    }
                }
                else
                {
                    // Pila non vuota: deve essere stesso seme e valore successivo
                    Card ultimaCarta = pilaFinale.Last();
                    if (ultimaCarta.seme == carta.seme && ultimaCarta.valore == carta.valore - 1)
                    {
                        SalvaStatoPerUndo("carta_pescata_pile_finali");
                        pilaFinale.Add(UsaCartaVisibilePescata());
                        return true;
                    }
                }
            }

            return false; // Nessuna pila adatta trovata
        }

        // METODO AGGIORNATO - Pile finali flessibili
        public bool MovimentoColonnaAPileFinali(int colonnaIndex)
        {
            if (colonnaIndex < 0 || colonnaIndex >= colonneIniziali.Count)
                return false;

            var colonna = colonneIniziali[colonnaIndex];
            if (colonna.Count == 0) return false;

            Card carta = colonna.Last();
            if (!carta.posizione) return false;

            // Cerca la prima pila finale adatta per questa carta
            for (int i = 0; i < pileFinali.Count; i++)
            {
                var pilaFinale = pileFinali[i];

                if (pilaFinale.Count == 0)
                {
                    // Pila vuota: accetta solo Assi
                    if (carta.valore == 1)
                    {
                        SalvaStatoPerUndo("colonna_pile_finali");
                        pilaFinale.Add(carta);
                        colonna.RemoveAt(colonna.Count - 1);

                        if (colonna.Count > 0 && !colonna.Last().posizione)
                            colonna.Last().posizione = true;

                        return true;
                    }
                }
                else
                {
                    // Pila non vuota: deve essere stesso seme e valore successivo
                    Card ultimaCarta = pilaFinale.Last();
                    if (ultimaCarta.seme == carta.seme && ultimaCarta.valore == carta.valore - 1)
                    {
                        SalvaStatoPerUndo("colonna_pile_finali");
                        pilaFinale.Add(carta);
                        colonna.RemoveAt(colonna.Count - 1);

                        if (colonna.Count > 0 && !colonna.Last().posizione)
                            colonna.Last().posizione = true;

                        return true;
                    }
                }
            }

            return false; // Nessuna pila adatta trovata
        }

        public bool MovimentoPileFinaliAColonna(int pilaIndex, int colonnaIndex)
        {
            if (pilaIndex < 0 || pilaIndex >= pileFinali.Count ||
                colonnaIndex < 0 || colonnaIndex >= colonneIniziali.Count)
                return false;

            var pilaFinale = pileFinali[pilaIndex];
            if (pilaFinale.Count == 0) return false;

            Card carta = pilaFinale.Last();
            var colonna = colonneIniziali[colonnaIndex];

            if (colonna.Count == 0)
            {
                if (carta.valore != 13) return false;
            }
            else
            {
                Card ultimaCarta = colonna.Last();
                if (!SonoColoriAlternati(carta.seme, ultimaCarta.seme)) return false;
                if (carta.valore != ultimaCarta.valore - 1) return false;
            }

            // Salva stato per undo prima di eseguire la mossa
            SalvaStatoPerUndo("pile_finali_colonna");

            colonna.Add(carta);
            pilaFinale.RemoveAt(pilaFinale.Count - 1);
            return true;
        }

        private bool SonoColoriAlternati(string seme1, string seme2)
        {
            bool rosso1 = (seme1 == "♥" || seme1 == "♦");
            bool rosso2 = (seme2 == "♥" || seme2 == "♦");
            return rosso1 != rosso2;
        }

        private string GetNomeCarta(Card carta)
        {
            if (carta == null) return "   ";

            string valore = carta.valore switch
            {
                1 => "A",
                11 => "J",
                12 => "Q",
                13 => "K",
                _ => carta.valore.ToString()
            };

            string nomeCompleto = valore + carta.seme;
            return nomeCompleto.PadRight(3);
        }

        // METODO AGGIORNATO per supportare i nomi completi dei semi
        private string GetNomeSemeCompleto(string seme)
        {
            return seme switch
            {
                "♥" => "Cuori",
                "♣" => "Fiori",
                "♠" => "Picche",
                "♦" => "Quadri",
                _ => "Sconosciuto"
            };
        }

        public bool IsVittoria()
        {
            // Vittoria tradizionale: tutte le carte nelle pile finali
            int totaleCarte = pileFinali.Sum(pila => pila.Count);
            if (totaleCarte == 52) return true;

            // Vittoria anticipata: verifica se tutte le colonne sono ordinate perfettamente
            return SonoTutteLeColonneOrdinate();
        }

        private bool SonoTutteLeColonneOrdinate()
        {
            foreach (var colonna in colonneIniziali)
            {
                if (colonna.Count == 0) continue;

                // Verifica che tutte le carte della colonna siano scoperte
                if (colonna.Any(c => !c.posizione)) return false;

                // Verifica sequenza decrescente e colori alternati
                for (int i = 0; i < colonna.Count - 1; i++)
                {
                    Card cartaCorrente = colonna[i];
                    Card cartaSuccessiva = colonna[i + 1];

                    // Verifica sequenza decrescente
                    if (cartaCorrente.valore != cartaSuccessiva.valore + 1) return false;

                    // Verifica colori alternati
                    if (!SonoColoriAlternati(cartaCorrente.seme, cartaSuccessiva.seme)) return false;
                }

                // La prima carta deve essere un Re per una sequenza completa
                if (colonna[0].valore != 13) return false;
            }

            return true;
        }

        // METODO AGGIORNATO - Display migliorato per pile finali flessibili
        public void MostraTabellone()
        {
            Console.Clear();
            Console.WriteLine("=== SOLITARIO ===\n");

            // Pila di riserva e carta pescata
            Console.Write("PILA RISERVA: ");
            if (pilaDiRiserva.Count > 0)
                Console.Write($"[###] ({pilaDiRiserva.Count})");
            else
                Console.Write("[   ] (0)");

            Console.Write("  CARTA PESCATA: ");
            Card cartaVisibile = GetCartaVisibilePescata();
            if (cartaVisibile != null)
                Console.Write($"[{GetNomeCarta(cartaVisibile)}]");
            else
                Console.Write("[   ]");

            

            // Pile finali - ora mostrano il contenuto effettivo invece di essere fisse per seme
            Console.WriteLine("PILE FINALI:");
            for (int i = 0; i < 4; i++)
            {
                Console.Write($"Pila {i + 1}: ");
                if (pileFinali[i].Count > 0)
                {
                    Card ultimaCarta = pileFinali[i].Last();
                    Console.Write($"[{GetNomeCarta(ultimaCarta)}] ");
                }
                else
                {
                    Console.Write("[   ]");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            // Colonne di gioco
            Console.WriteLine("COLONNE DI GIOCO:");
            Console.WriteLine("Col1  Col2  Col3  Col4  Col5  Col6  Col7");
            Console.WriteLine("----  ----  ----  ----  ----  ----  ----");

            int maxAltezza = colonneIniziali.Max(col => col.Count);

            for (int riga = 0; riga < maxAltezza; riga++)
            {
                for (int col = 0; col < 7; col++)
                {
                    if (riga < colonneIniziali[col].Count)
                    {
                        Card carta = colonneIniziali[col][riga];
                        if (carta.posizione)
                            Console.Write($"[{GetNomeCarta(carta)}] ");
                        else
                            Console.Write("[###] ");
                    }
                    else
                    {
                        Console.Write("      ");
                    }
                }
                Console.WriteLine();
            }
        }

        // Metodi per il sistema di Undo e Restart
        private void SalvaStatoPerUndo(string tipoMossa)
        {
            var memoria = new MossaMemoria
            {
                TipoMossa = tipoMossa,
                ColonneStato = CopiaColonne(colonneIniziali),
                PileFinaliStato = CopiaPileFinali(pileFinali),
                PilaRiservaStato = new List<Card>(pilaDiRiserva.Select(c => new Card(c.valore, c.seme, c.posizione))),
                CartePescateStato = new Stack<Card>(cartePescate.Reverse().Select(c => new Card(c.valore, c.seme, c.posizione)))
            };

            cronologiaMosse.Push(memoria);

            // Mantieni solo le ultime 10 mosse per evitare consumo eccessivo di memoria
            if (cronologiaMosse.Count > 10)
            {
                var mosseTemp = new Stack<MossaMemoria>();
                for (int i = 0; i < 10; i++)
                {
                    mosseTemp.Push(cronologiaMosse.Pop());
                }
                cronologiaMosse.Clear();
                while (mosseTemp.Count > 0)
                {
                    cronologiaMosse.Push(mosseTemp.Pop());
                }
            }
        }

        public bool AnnullaUltimaMossa()
        {
            if (cronologiaMosse.Count == 0)
                return false;

            var ultimaMossa = cronologiaMosse.Pop();

            // Ripristina lo stato precedente
            colonneIniziali = ultimaMossa.ColonneStato;
            pileFinali = ultimaMossa.PileFinaliStato;
            pilaDiRiserva = ultimaMossa.PilaRiservaStato;
            cartePescate = ultimaMossa.CartePescateStato;

            return true;
        }

        public void RicominciaPerizia()
        {
            // Ripristina lo stato iniziale della partita
            colonneIniziali = CopiaColonne(colonneIniziali_Backup);
            pileFinali = new List<List<Card>>();
            for (int i = 0; i < 4; i++)
            {
                pileFinali.Add(new List<Card>());
            }
            pilaDiRiserva = new List<Card>(pilaDiRiserva_Backup.Select(c => new Card(c.valore, c.seme, c.posizione)));
            cartePescate = new Stack<Card>();
            cronologiaMosse = new Stack<MossaMemoria>();
        }

        private void CreaBackup()
        {
            // Crea backup dello stato iniziale per il restart
            colonneIniziali_Backup = CopiaColonne(colonneIniziali);
            pilaDiRiserva_Backup = new List<Card>(pilaDiRiserva.Select(c => new Card(c.valore, c.seme, c.posizione)));
        }

        private List<List<Card>> CopiaColonne(List<List<Card>> originale)
        {
            var copia = new List<List<Card>>();
            foreach (var colonna in originale)
            {
                var nuovaColonna = new List<Card>();
                foreach (var carta in colonna)
                {
                    nuovaColonna.Add(new Card(carta.valore, carta.seme, carta.posizione));
                }
                copia.Add(nuovaColonna);
            }
            return copia;
        }

        private List<List<Card>> CopiaPileFinali(List<List<Card>> originale)
        {
            var copia = new List<List<Card>>();
            foreach (var pila in originale)
            {
                var nuovaPila = new List<Card>();
                foreach (var carta in pila)
                {
                    nuovaPila.Add(new Card(carta.valore, carta.seme, carta.posizione));
                }
                copia.Add(nuovaPila);
            }
            return copia;
        }

        public bool HaMosseDisponibili()
        {
            return cronologiaMosse.Count > 0;
        }

        // Metodi di supporto per messaggi di errore dettagliati
        public string GetDettaglioErrore(string tipoMovimento, params object[] parametri)
        {
            switch (tipoMovimento)
            {
                case "colonna_vuota":
                    return $"La colonna {(int)parametri[0]} è vuota!";
                case "carta_coperta":
                    return "Non puoi spostare carte coperte!";
                case "solo_re_su_vuota":
                    return "Su una colonna vuota puoi mettere solo un Re!";
                case "colori_non_alternati":
                    return "I colori devono essere alternati (rosso su nero, nero su rosso)!";
                case "sequenza_sbagliata":
                    return "La carta deve essere di valore immediatamente inferiore!";
                case "solo_asso_su_pila_vuota":
                    return "Su una pila finale vuota puoi mettere solo un Asso!";
                case "sequenza_pila_sbagliata":
                    return "Nelle pile finali le carte devono essere in sequenza crescente dello stesso seme!";
                case "nessuna_carta_pescata":
                    return "Non hai carte pescate da spostare!";
                case "pila_riserva_vuota":
                    return "La pila di riserva è vuota!";
                case "nessuna_carta_da_riciclare":
                    return "Non ci sono carte pescate da riciclare!";
                default:
                    return "Movimento non valido!";
            }
        }
    }
}
