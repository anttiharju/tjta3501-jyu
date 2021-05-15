using Npgsql;
using System;

namespace tjta3501
{
    public class Program
    {
        public static void Main()
        {
            CommandEngine engine = new CommandEngine();
            new App(engine);
            engine.Run();
        }
    }


    public class App
    {
        private readonly string connectionString = "Host=192.168.1.23;Username=hoyry;Password=hl3;Database=tjta3501";
        private int pelaajaid;


        public App(CommandEngine engine)
        {
            engine.AddCommand("kirjaudu", Kirjaudu);
            engine.AddCommand("kokoelma", Kokoelma);
        }


        public void Kokoelma(string[] args)
        {
            Console.WriteLine(pelaajaid);
            Console.ReadKey();
        }


        public void Kirjaudu(string[] args)
        {
            if (args.Length > 0)
            {
                int tmp = int.Parse(args[0]);

                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();
                string sql = $"SELECT nimimerkki FROM pelaaja WHERE id = {tmp} AND ban_pvm IS NULL";
                string s = new NpgsqlCommand(sql, connection).ExecuteScalar()?.ToString();
                if (s == null)
                {
                    Console.WriteLine($"Id {tmp} ei vastaa yhtään pelaajaa tai pelaajalla on porttikielto.");
                }
                else
                {
                    Console.WriteLine($"Tervetuloa takaisin, {s}.");
                    pelaajaid = tmp;
                }
                connection.Close();
            }
            else
            {
                Console.WriteLine("\nAnna id kirjautuaksesi.");
            }
            Console.WriteLine("\nPaina mitä tahansa nappia jatkaaksesi.");
            Console.ReadKey();
        }
    }
}
