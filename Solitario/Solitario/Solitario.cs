using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solitario
{
    internal class Solitario
    {
        private Gameboard gameboard;
        private bool giocoInCorso;

        public Solitario()
        {
            gameboard = new Gameboard();
            giocoInCorso = true;
            AvviaGioco();
        }

        private void AvviaGioco()
        {
            while (giocoInCorso)
            {
                gameboard.MostraTabellone();

                if (gameboard.IsVittoria())
                {
                    Console.WriteLine("\n🎉 CONGRATULAZIONI! HAI VINTO! 🎉");
                    Console.WriteLine("Premi un tasto per uscire...");
                    Console.ReadKey();
                    break;
                }

                MostraComandi();
                string input = Console.ReadLine()?.ToLower().Trim();
                GestisciComando(input);
            }
        }

        private void MostraComandi()
        {
            Console.WriteLine("\n=== COMANDI DISPONIBILI ===");
            Console.WriteLine("PILA DI RISERVA:");
            Console.WriteLine("  p           - Pesca una carta dalla pila di riserva");
            Console.WriteLine("  r           - Ricicla le carte pescate nella pila di riserva");
            Console.WriteLine();
            Console.WriteLine("MOVIMENTO TRA COLONNE:");
            Console.WriteLine("  m [da] [a]  - Sposta carte da una colonna all'altra");
            Console.WriteLine("                Esempio: 'm 1 3'");
            Console.WriteLine();
            Console.WriteLine("CARTA PESCATA:");
            Console.WriteLine("  cp [colonna] - Sposta la carta pescata su una colonna");
            Console.WriteLine("                 Esempio: 'cp 2' sposta la carta pescata sulla colonna 2");
            Console.WriteLine("  cpf          - Sposta la carta pescata alle pile finali");
            Console.WriteLine();
            Console.WriteLine("PILE FINALI:");
            Console.WriteLine("  pf [colonna] - Sposta l'ultima carta di una colonna alle pile finali");
            Console.WriteLine("                 Esempio: 'pf 4' sposta dalla colonna 4 alle pile finali");
            Console.WriteLine("  fp [pila] [colonna] - Sposta da pila finale a colonna");
            Console.WriteLine("                        Esempio: 'fp 1 5' sposta dalla pila 1 alla colonna 5");
            Console.WriteLine();
            Console.WriteLine("ALTRO:");
            Console.WriteLine("  u           - Annulla ultima mossa (undo)");
            Console.WriteLine("  restart     - Ricomincia la stessa partita da capo");
            Console.WriteLine("  q           - Esci dal gioco");
            Console.WriteLine("\nNOTE: Colonne numerate 1-7, Pile finali numerate 1-4");
            Console.Write("\nInserisci comando: ");
        }

        private void GestisciComando(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            string[] parti = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string comando = parti[0];

            switch (comando)
            {
                case "p":
                    GestisciPesca();
                    break;

                case "r":
                    GestisciRiciclo();
                    break;

                case "m":
                    GestisciMovimentoColonne(parti);
                    break;

                case "cp":
                    GestisciCartaPescataAColonna(parti);
                    break;

                case "pf":
                    GestisciColonnaAPileFinali(parti);
                    break;

                case "cpf":
                    GestisciCartaPescataAPileFinali();
                    break;

                case "fp":
                    GestisciPileFinaliAColonna(parti);
                    break;

                case "u":
                    GestisciUndo();
                    break;

                case "restart":
                    GestisciRestart();
                    break;

                case "q":
                    Console.WriteLine("Grazie per aver giocato!");
                    giocoInCorso = false;
                    return;

                default:
                    Console.WriteLine("Comando non riconosciuto! Usa 'h' per vedere tutti i comandi.");
                    break;
            }

            if (comando != "q" && comando != "h" && comando != "restart")
            {
                Console.WriteLine("\nPremi un tasto per continuare...");
                Console.ReadKey();
            }
        }

        private void GestisciPesca()
        {
            if (gameboard.PescaDallaPilaDiRiserva())
            {
                Console.WriteLine("✓ Carta pescata con successo!");
            }
            else
            {
                Console.WriteLine("✗ " + gameboard.GetDettaglioErrore("pila_riserva_vuota"));
                Console.WriteLine("  Suggerimento: Usa 'r' per riciclare le carte pescate.");
            }
        }

        private void GestisciRiciclo()
        {
            if (gameboard.RiciclaPilaDiRiserva())
            {
                Console.WriteLine("✓ Carte riciclate con successo nella pila di riserva!");
            }
            else
            {
                Console.WriteLine("✗ " + gameboard.GetDettaglioErrore("nessuna_carta_da_riciclare"));
            }
        }

        private void GestisciMovimentoColonne(string[] parti)
        {
            if (parti.Length != 3)
            {
                Console.WriteLine("✗ Formato errato! Usa: m [colonna origine] [colonna destinazione]");
                Console.WriteLine("  Esempio: 'm 1 3' per spostare dalla colonna 1 alla 3");
                return;
            }

            if (!int.TryParse(parti[1], out int origine) || !int.TryParse(parti[2], out int destinazione))
            {
                Console.WriteLine("✗ I numeri delle colonne devono essere validi!");
                return;
            }

            if (origine < 1 || origine > 7 || destinazione < 1 || destinazione > 7)
            {
                Console.WriteLine("✗ Le colonne devono essere numerate da 1 a 7!");
                return;
            }

            // Verifica che la colonna origine non sia vuota
            var colOrigine = gameboard.colonneIniziali[origine - 1];
            if (colOrigine.Count == 0)
            {
                Console.WriteLine($"✗ La colonna {origine} è vuota!");
                return;
            }

            // Trova l'indice della prima carta scoperta
            int indicePartenza = colOrigine.FindIndex(c => c.posizione);
            if (indicePartenza == -1)
            {
                Console.WriteLine($"✗ Non ci sono carte scoperte nella colonna {origine}!");
                return;
            }

            // Ottieni tutte le opzioni di spostamento possibili
            var opzioniSpostamento = CalcolaOpzioniSpostamento(origine - 1, destinazione - 1);

            if (opzioniSpostamento.Count == 0)
            {
                Console.WriteLine($"✗ Impossibile spostare carte dalla colonna {origine} alla colonna {destinazione}");
                Console.WriteLine("  Nessuna delle carte scoperte può essere spostata in quella posizione!");
                Console.WriteLine("  Controlla che il movimento rispetti le regole del Solitario!");
                return;
            }

            int numCarte;

            if (opzioniSpostamento.Count == 1)
            {
                // Se c'è solo un'opzione, usala direttamente
                numCarte = opzioniSpostamento[0].NumCarte;
                var carta = opzioniSpostamento[0].CartaInizio;
                string valoreCarta = carta.valore switch
                {
                    1 => "A",
                    11 => "J",
                    12 => "Q",
                    13 => "K",
                    _ => carta.valore.ToString()
                };
                Console.WriteLine($"Spostamento di {numCarte} carta/e dalla colonna {origine} alla colonna {destinazione}...");
                Console.WriteLine($"Partendo da: {valoreCarta}{carta.seme}");
            }
            else
            {
                // Se ci sono più opzioni, mostra il menu di scelta
                Console.WriteLine($"Dalla colonna {origine} alla colonna {destinazione} puoi spostare:");
                Console.WriteLine();

                for (int i = 0; i < opzioniSpostamento.Count; i++)
                {
                    var opzione = opzioniSpostamento[i];
                    string valoreCarta = opzione.CartaInizio.valore switch
                    {
                        1 => "A",
                        11 => "J",
                        12 => "Q",
                        13 => "K",
                        _ => opzione.CartaInizio.valore.ToString()
                    };

                    Console.WriteLine($"  {i + 1}. {opzione.NumCarte} carta/e partendo da {valoreCarta}{opzione.CartaInizio.seme}");

                    // Mostra le carte che verranno spostate
                    Console.Write("      Carte: ");
                    for (int j = 0; j < opzione.NumCarte; j++)
                    {
                        var carta = colOrigine[opzione.IndicePartenza + j];
                        string val = carta.valore switch
                        {
                            1 => "A",
                            11 => "J",
                            12 => "Q",
                            13 => "K",
                            _ => carta.valore.ToString()
                        };
                        Console.Write($"{val}{carta.seme}");
                        if (j < opzione.NumCarte - 1) Console.Write(" → ");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.Write($"Scegli un'opzione (1-{opzioniSpostamento.Count}): ");
                string input = Console.ReadLine();

                if (!int.TryParse(input, out int scelta) || scelta < 1 || scelta > opzioniSpostamento.Count)
                {
                    Console.WriteLine("✗ Scelta non valida!");
                    return;
                }

                var opzioneScelta = opzioniSpostamento[scelta - 1];
                numCarte = opzioneScelta.NumCarte;

                // Per il metodo MovimentoTraColonne, dobbiamo calcolare quante carte dalla prima carta scoperta
                numCarte = opzioneScelta.IndicePartenza - indicePartenza + opzioneScelta.NumCarte;
            }

            // Esegui il movimento
            if (gameboard.MovimentoTraColonne(origine - 1, destinazione - 1, numCarte))
            {
                Console.WriteLine($"✓ {numCarte} carta/e spostate con successo dalla colonna {origine} alla colonna {destinazione}!");
            }
            else
            {
                Console.WriteLine($"✗ Errore imprevisto durante lo spostamento!");
            }
        }

        private class OpzioneSpostamento
        {
            public Card CartaInizio { get; set; }
            public int IndicePartenza { get; set; }
            public int NumCarte { get; set; }
        }

        // Nuovo metodo per calcolare tutte le opzioni di spostamento possibili
        private List<OpzioneSpostamento> CalcolaOpzioniSpostamento(int origine, int destinazione)
        {
            var opzioni = new List<OpzioneSpostamento>();
            var colOrigine = gameboard.colonneIniziali[origine];
            var colDestinazione = gameboard.colonneIniziali[destinazione];

            if (colOrigine.Count == 0) return opzioni;

            // Trova l'indice della prima carta scoperta
            int indicePartenza = colOrigine.FindIndex(c => c.posizione);
            if (indicePartenza == -1) return opzioni;

            // Controlla ogni possibile punto di partenza
            for (int startIndex = indicePartenza; startIndex < colOrigine.Count; startIndex++)
            {
                Card cartaInizio = colOrigine[startIndex];

                // Verifica se questa carta può essere spostata sulla colonna di destinazione
                bool cartaValida = false;

                if (colDestinazione.Count == 0)
                {
                    // Colonna vuota: accetta solo Re
                    cartaValida = (cartaInizio.valore == 13);
                }
                else
                {
                    // Colonna non vuota: verifica colori alternati e sequenza decrescente
                    Card cartaDest = colDestinazione[colDestinazione.Count - 1];
                    cartaValida = SonoColoriAlternati(cartaInizio.seme, cartaDest.seme) &&
                                 (cartaInizio.valore == cartaDest.valore - 1);
                }

                if (cartaValida)
                {
                    // Questa carta può essere spostata, ora conta quante carte consecutive possono essere spostate
                    int carteSpostabili = 1;

                    // Controlla le carte successive per vedere se formano una sequenza valida fino alla fine
                    for (int i = startIndex + 1; i < colOrigine.Count; i++)
                    {
                        Card cartaCorrente = colOrigine[i - 1];
                        Card cartaSuccessiva = colOrigine[i];

                        // Verifica se la sequenza continua
                        if (cartaSuccessiva.posizione && // Deve essere scoperta
                            SonoColoriAlternati(cartaCorrente.seme, cartaSuccessiva.seme) && // Colori alternati
                            cartaCorrente.valore == cartaSuccessiva.valore + 1) // Sequenza decrescente
                        {
                            carteSpostabili++;
                        }
                        else
                        {
                            break; // La sequenza si interrompe
                        }
                    }

                    // Aggiungi questa opzione
                    opzioni.Add(new OpzioneSpostamento
                    {
                        CartaInizio = cartaInizio,
                        IndicePartenza = startIndex,
                        NumCarte = carteSpostabili
                    });
                }
            }

            return opzioni;
        }


        // Metodo helper per calcolare quante carte si possono effettivamente spostare
        private int CalcolaCarteSpostabili(int origine, int destinazione)
        {
            var colOrigine = gameboard.colonneIniziali[origine];
            var colDestinazione = gameboard.colonneIniziali[destinazione];

            if (colOrigine.Count == 0) return 0;

            // Trova l'indice della prima carta scoperta
            int indicePartenza = colOrigine.FindIndex(c => c.posizione);
            if (indicePartenza == -1) return 0;

            // Trova la prima carta scoperta che può essere spostata nella destinazione
            for (int startIndex = indicePartenza; startIndex < colOrigine.Count; startIndex++)
            {
                Card cartaInizio = colOrigine[startIndex];

                // Verifica se questa carta può essere spostata sulla colonna di destinazione
                bool cartaValida = false;

                if (colDestinazione.Count == 0)
                {
                    // Colonna vuota: accetta solo Re
                    cartaValida = (cartaInizio.valore == 13);
                }
                else
                {
                    // Colonna non vuota: verifica colori alternati e sequenza decrescente
                    Card cartaDest = colDestinazione[colDestinazione.Count - 1];
                    cartaValida = SonoColoriAlternati(cartaInizio.seme, cartaDest.seme) &&
                                 (cartaInizio.valore == cartaDest.valore - 1);
                }

                if (cartaValida)
                {
                    // Questa carta può essere spostata, ora conta quante carte consecutive possono essere spostate
                    int carteSpostabili = 1;

                    // Controlla le carte successive per vedere se formano una sequenza valida fino alla fine
                    for (int i = startIndex + 1; i < colOrigine.Count; i++)
                    {
                        Card cartaCorrente = colOrigine[i - 1];
                        Card cartaSuccessiva = colOrigine[i];

                        // Verifica se la sequenza continua
                        if (cartaSuccessiva.posizione && // Deve essere scoperta
                            SonoColoriAlternati(cartaCorrente.seme, cartaSuccessiva.seme) && // Colori alternati
                            cartaCorrente.valore == cartaSuccessiva.valore + 1) // Sequenza decrescente
                        {
                            carteSpostabili++;
                        }
                        else
                        {
                            break; // La sequenza si interrompe
                        }
                    }

                    return carteSpostabili;
                }
            }

            return 0; // Nessuna carta può essere spostata
        }

        private bool SonoColoriAlternati(string seme1, string seme2)
        {
            bool rosso1 = (seme1 == "♥" || seme1 == "♦");
            bool rosso2 = (seme2 == "♥" || seme2 == "♦");
            return rosso1 != rosso2;
        }

        private void GestisciUndo()
        {
            if (gameboard.AnnullaUltimaMossa())
            {
                Console.WriteLine("✓ Ultima mossa annullata con successo!");
            }
            else
            {
                Console.WriteLine("✗ Nessuna mossa da annullare!");
            }
        }

        private void GestisciRestart()
        {
            Console.WriteLine("Sei sicuro di voler ricominciare la partita da capo? (s/n)");
            string conferma = Console.ReadLine()?.ToLower().Trim();

            if (conferma == "s" || conferma == "si" || conferma == "y" || conferma == "yes")
            {
                gameboard.RicominciaPerizia();
                Console.WriteLine("✓ Partita riavviata! Stessa disposizione iniziale delle carte.");
            }
            else
            {
                Console.WriteLine("Restart annullato.");
            }
        }

        private void GestisciCartaPescataAColonna(string[] parti)
        {
            if (parti.Length != 2)
            {
                Console.WriteLine("✗ Formato errato! Usa: cp [colonna di destinazione]");
                Console.WriteLine("  Esempio: 'cp 3' per spostare la carta pescata sulla colonna 3");
                return;
            }

            if (!int.TryParse(parti[1], out int colonna))
            {
                Console.WriteLine("✗ Il numero della colonna deve essere valido!");
                return;
            }

            if (colonna < 1 || colonna > 7)
            {
                Console.WriteLine("✗ La colonna deve essere numerata da 1 a 7!");
                return;
            }

            if (gameboard.GetCartaVisibilePescata() == null)
            {
                Console.WriteLine("✗ " + gameboard.GetDettaglioErrore("nessuna_carta_pescata"));
                Console.WriteLine("  Suggerimento: Usa 'p' per pescare una carta prima!");
                return;
            }

            if (gameboard.MovimentoCartaPescataAColonna(colonna - 1))
            {
                Console.WriteLine($"✓ Carta pescata spostata con successo sulla colonna {colonna}!");
            }
            else
            {
                Console.WriteLine($"✗ Impossibile spostare la carta pescata sulla colonna {colonna}");
                Console.WriteLine("  Controlla che il movimento rispetti le regole del Solitario!");
            }
        }

        private void GestisciColonnaAPileFinali(string[] parti)
        {
            if (parti.Length != 2)
            {
                Console.WriteLine("✗ Formato errato! Usa: pf [colonna]");
                Console.WriteLine("  Esempio: 'pf 2' per spostare la carta dalla colonna 2 alle pile finali");
                return;
            }

            if (!int.TryParse(parti[1], out int colonna))
            {
                Console.WriteLine("✗ Il numero della colonna deve essere valido!");
                return;
            }

            if (colonna < 1 || colonna > 7)
            {
                Console.WriteLine("✗ La colonna deve essere numerata da 1 a 7!");
                return;
            }

            if (gameboard.MovimentoColonnaAPileFinali(colonna - 1))
            {
                Console.WriteLine($"✓ Carta spostata con successo dalla colonna {colonna} alle pile finali!");
            }
            else
            {
                Console.WriteLine($"✗ Impossibile spostare la carta dalla colonna {colonna} alle pile finali");
                Console.WriteLine("  La carta deve essere un Asso su pila vuota o la carta successiva nella sequenza!");
            }
        }

        private void GestisciCartaPescataAPileFinali()
        {
            if (gameboard.GetCartaVisibilePescata() == null)
            {
                Console.WriteLine("✗ " + gameboard.GetDettaglioErrore("nessuna_carta_pescata"));
                Console.WriteLine("  Suggerimento: Usa 'p' per pescare una carta prima!");
                return;
            }

            if (gameboard.MovimentoCartaPescataAPileFinali())
            {
                Console.WriteLine("✓ Carta pescata spostata con successo alle pile finali!");
            }
            else
            {
                Console.WriteLine("✗ Impossibile spostare la carta pescata alle pile finali");
                Console.WriteLine("  La carta deve essere un Asso su pila vuota o la carta successiva nella sequenza!");
            }
        }

        private void GestisciPileFinaliAColonna(string[] parti)
        {
            if (parti.Length != 3)
            {
                Console.WriteLine("✗ Formato errato! Usa: fp [pila] [colonna]");
                Console.WriteLine("  Esempio: 'fp 1 4' per spostare dalla pila finale 1 alla colonna 4");
                Console.WriteLine("  Pile finali: 1=♥(Cuori), 2=♣(Fiori), 3=♠(Picche), 4=♦(Quadri)");
                return;
            }

            if (!int.TryParse(parti[1], out int pila) || !int.TryParse(parti[2], out int colonna))
            {
                Console.WriteLine("✗ I numeri della pila e colonna devono essere validi!");
                return;
            }

            if (pila < 1 || pila > 4)
            {
                Console.WriteLine("✗ La pila deve essere numerata da 1 a 4!");
                Console.WriteLine("  1=♥(Cuori), 2=♣(Fiori), 3=♠(Picche), 4=♦(Quadri)");
                return;
            }

            if (colonna < 1 || colonna > 7)
            {
                Console.WriteLine("✗ La colonna deve essere numerata da 1 a 7!");
                return;
            }

            if (gameboard.MovimentoPileFinaliAColonna(pila - 1, colonna - 1))
            {
                Console.WriteLine($"✓ Carta spostata con successo dalla pila finale {pila} alla colonna {colonna}!");
            }
            else
            {
                Console.WriteLine($"✗ Impossibile spostare dalla pila finale {pila} alla colonna {colonna}");
                Console.WriteLine("  Controlla che il movimento rispetti le regole del Solitario!");
            }
        }
    }
}
