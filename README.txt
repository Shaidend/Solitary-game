Gioco del solitario

Programmato in C#
E' una riproduzione in console del classico gioco di carte Solitario (Klondike).


Prerequisiti per avviare il igoco senza problema
- .NET 6.0, espressamente consigliabile .NET 8.0
- Visual Studio 2019/2022 o Visual Studio Code con C# extension

Passaggi per l'Avvio

1. Estrarre il file .zip in una cartella locale

2. Apertura in Visual Studio
   - Aprire Visual Studio
   - File → Apri → Progetto/Soluzione
   - Selezionare il file Solitario.sln

3. Compilazione ed Esecuzione
   - Premere F5 per Debug
   - Oppure Ctrl+F5 per esecuzione senza debug
   - Oppure da terminale: dotnet run

-- Istruzioni per l'Utente --

1. Obiettivo del Gioco:

Spostare tutte le 52 carte nelle quattro pile finali, ordinate per seme dall'Asso al Re.

2. Layout del Gioco
- Pila di Riserva: Mazzo da cui pescare carte (in alto a sinistra)
- Carta Pescata: Carta visibile dalla pila di riserva
- Pile Finali: 4 pile dove completare le sequenze per seme (A→K)
- Colonne di Gioco: 7 colonne con carte disposte in sequenza crescente

3. Comandi Disponibili

1. Gestione Pila di Riserva
- p - Pesca una carta dalla pila di riserva
- r - Ricicla le carte pescate nella pila di riserva
2. Movimento tra Colonne
- m [origine] [destinazione` - Sposta carte tra colonne
  - Esempio: `m 1 3` sposta dalla colonna 1 alla colonna 3
  - Se multiple opzioni disponibili, il sistema mostrerà un menu di scelta
3. Movimento Carta Pescata
- cp [colonna] - Sposta la carta pescata su una colonna
  - Esempio: `cp 2` sposta sulla colonna 2
- cpf - Sposta la carta pescata alle pile finali

4. Movimento alle Pile Finali
- pf [colonna] - Sposta l'ultima carta di una colonna alle pile finali
  - Esempio: `pf 4` sposta dalla colonna 4
- fp [pila] [colonna] - Sposta da pila finale a colonna
  - Esempio: `fp 1 5` sposta dalla pila 1 alla colonna 5

5. Comandi di Sistema
- u - Annulla ultima mossa (undo)
- restart - Ricomincia la stessa partita da capo
- q - Esci dal gioco

6. Regole del Gioco
Distribuire le carte per ogni colonna nell'ordine decrescende (K - Q - J - ... - 2 - A), con colori alternati(rosso su nero, nero su rosso), 
soltanto il re può essere spostato su una colonna vuota o in caso spostiate tutta la colonna basta che ci sia il re in cima.
Nelle pile finali le carte devono essere inserite in ordine cresscente (A - 2 - 3 - ... - K), ogni pila può avere le carte di un solo seme(♥, ♣, ♠, ♦).
La pila di riserva è dove vi sono le carte rimanenti dal mazzo, si possono pescare queste carte una alla volta, inoltre c'è la possibilità di riciclare le carte facendo ripartire da capo con lo stesso ordine la pesca.

7. Architettura della Soluzione

- Panoramica Classi

Il progetto è separato nelle seguenti classi principali:
- Program.cs          'Entry point dell'applicazione'
- Solitario.cs        'Controller principale del gioco'
- Gameboard.cs        'Logica di gioco e stato del tabellone'
- Deck.cs             'Gestione del mazzo di carte'
- Card.cs             'Rappresentazione della singola carta'

- Descrizione delle Classi

// Card.cs

 Metodi:
- `Card(int valore, string seme, bool posizione)`: Costruttore per inizializzare una carta

// Deck.cs

Metodi principali:
- `Deck()`: Costruttore che crea e mescola il mazzo
- `creaDeck()`: Crea un mazzo standard di 52 carte
- `mischiaDeck()`: Mescola casualmente le carte
- `creaCarta()`: Estrae e restituisce la prima carta dal mazzo

// Gameboard.cs

Metodi Principali:
- `creazioneColonne()`: Inizializza le 7 colonne con la disposizione corretta
- `creazionePilaRiserva()`: Popola la pila di riserva con le carte rimanenti
- `PescaDallaPilaDiRiserva()`: Pesca una carta dalla pila di riserva
- `RiciclaPilaDiRiserva()`: Ricicla le carte pescate nella pila di riserva
- `MovimentoTraColonne()`: Gestisce il movimento delle carte tra colonne
- `MovimentoCartaPescataAColonna()`: Sposta carta pescata su una colonna
- `MovimentoCartaPescataAPileFinali()`: Sposta carta pescata alle pile finali
- `MovimentoColonnaAPileFinali()`: Sposta da colonna alle pile finali
- `MovimentoPileFinaliAColonna()`: Sposta da pile finali a colonna
- `IsVittoria()`: Verifica condizioni di vittoria
- `MostraTabellone()`: Visualizza lo stato corrente del gioco
- `AnnullaUltimaMossa()`: Sistema di undo
- `RicominciaPerizia()`: Restart della partita

Metodi di Supporto:
- `SonoColoriAlternati()`: Verifica alternanza dei colori
- `GetNomeCarta()`: Formattazione nome carta per display
- `SalvaStatoPerUndo()`: Salva stato per sistema di undo

Classi interne:
- MossaMemoria
// MossaMemoria (Classe Interna)
Responsabilità: Memorizzazione stato per sistema di undo.



// Solitario.cs

Metodi Principali:
- `AvviaGioco()`: Loop principale del gioco
- `MostraComandi()`: Visualizza i comandi disponibili
- `GestisciComando()`: Parser e dispatcher dei comandi utente
- `GestisciPesca()`: Gestisce comando di pesca
- `GestisciMovimentoColonne()`: Gestisce movimenti tra colonne con menu interattivo
- `GestisciUndo()`: Gestisce comando di undo
- `GestisciRestart()`: Gestisce restart con conferma

// Program.cs

Metodo principale:
- `Main(string[] args)`: Punto di ingresso, inizializza encoding UTF-8 e avvia il gioco


8. Funzionalità Avanzate Implementate

- Sistema di Undo Intelligente

- Movimento Intelligente tra Colonne

-  Pile Finali Flessibili

- Sistema di Restart

- Gestione Robusta degli Errori

- Interfaccia Console Ottimizzata

9. Principi di Progettazione Applicati

- Single Responsibility: Ogni classe ha una responsabilità ben definita
- Gerarchia di classi rispetta i contratti
- Interfacce coese e specifiche

10. Design Patterns
- MVC Pattern: Separazione tra Model (Gameboard), View (Console), Controller (Solitario)
- Gestione comandi utente
- Sistema di undo/redo
- Creazione delle carte nel Deck

11 Best Practices
- Eliminazione di codice duplicato
- Soluzioni semplici
- Nomi significativi, funzioni piccole, commenti appropriati
- Gestione completa degli errori e casi edge

12. Tecnologie Utilizzate

- Linguaggio C# (.NET Framework/Core)
- Programmazione Orientata agli Oggetti
- Librerie Standard (System.Collections.Generic, System.Linq)
- UTF-8 per supporto caratteri speciali
- IDE Supportati, Visual Studio e Visual Studio Code(con C# extention)

13 Note Tecniche

a) Gestione Memoria
- Utilizzo di List<T> per collezioni dinamiche
- Stack<T> per gestione LIFO delle carte pescate
- Clonazione profonda degli oggetti per sistema di undo

b) Algoritmi Implementati
- Fisher-Yates shuffle per mescolamento carte
- Algoritmo di validazione delle mosse basato su regole del Solitario
- Algoritmo di ricerca delle opzioni di movimento ottimizzato

c) Robustezza
- Validazione completa degli input utente
- Gestione delle eccezioni per operazioni critiche
- Fallback graceful per situazioni impreviste

---

Autore: David Capezzuto  
Data: 12/06/2025  
Versione: 1.0