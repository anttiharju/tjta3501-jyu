using System;
using System.Collections.Generic;
using System.Text;

namespace tjta3501
{
    public class CommandEngine
    {
        private readonly Dictionary<string, Action<string[]>> commands;


        public CommandEngine()
        {
            commands = new Dictionary<string, Action<string[]>>();
            AddCommand("sulje", Exit);
            AddCommand("exit", Exit);
            AddCommand("q", Exit);
        }


        public void AddCommand(string command, Action<string[]> method)
        {
            commands.Add(command, method);
        }


        public void Run()
        {
            while (true)
            {
                Home();
                string[] input = Console.ReadLine().ToLower().Split(' ');
                Console.Clear();

                string command = input[0];
                string[] args = new string[input.Length - 1];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = input[i + 1];
                }

                if (commands.ContainsKey(command))
                {
                    commands[command].Invoke(args);
                }
                else
                {
                    if (command != "")
                    {
                        Error($"Komentoa \'{command}\' ei tunnistettu.");
                        continue;
                    }
                }
                Console.Clear();
            }
        }


        public void Continue()
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nPaina mitä tahansa näppäintä jatkaaksesi.");
            Console.ReadKey();
            Console.ForegroundColor = tmp;
        }


        public void Success(string s)
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(s);
            Console.ForegroundColor = tmp;
        }


        public void Error(string s)
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(s);
            Console.ForegroundColor = tmp;
        }


        private void Exit(string[] args)
        {
            Environment.Exit(0);
        }


        private void Home()
        {
            Logo();
            StringBuilder printout = new StringBuilder("");
            printout.Append("KOMENTO                KUVAUS\n\n");
            printout.Append("kirjaudu [pelaaja-id]  Kirjaudu sisään.\n");
            printout.Append("ulos                   Kirjaudu ulos.\n");
            printout.Append("kokoelma               Listaa omistamasi pelit (vaatii kirjautumisen).\n");
            printout.Append("hae [hakusana]         Hae pelejä.\n");
            printout.Append("osta [peli-id]         Lisää peli kokoelmaasi.\n");
            printout.Append("arvostele              Siirry arvostelutilaan.\n");
            printout.Append("arvostelut             Listaa kirjoittamasi arvostelut (raakile)\n");
            printout.Append("sulje                  Sulje sovellus.\n");
            Console.WriteLine(printout.ToString());
        }


        /// Isoa tekstiä saa generoitua hakemalla "[sana] big text" duckduckgo'sta. Ei tue äö joten * lisätty käsin
        private void Logo()
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            StringBuilder printout = new StringBuilder("");
            printout.Append(" _   _                        \n");
            printout.Append("| | | | *_*  _   _ _ __ _   _ \n");
            printout.Append("| |_| |/ _ \\| | | | '__| | | |\n");
            printout.Append("|  _  | (_) | |_| | |  | |_| |\n");
            printout.Append("|_| |_|\\___/ \\__, |_|   \\__, |\n");
            printout.Append("             |___/      |___/ \n");
            Console.WriteLine(printout.ToString());
            Console.ForegroundColor = tmp;
        }
    }
}
