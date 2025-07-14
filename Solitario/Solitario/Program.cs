namespace Solitario
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("🎴 Benvenuto al Solitario! 🎴");
            Console.WriteLine("Premi un tasto per iniziare...");
            Console.ReadKey();

            Solitario partita = new Solitario();
        }
    }
}
