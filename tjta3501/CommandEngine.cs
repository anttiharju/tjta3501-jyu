using System;
using System.Collections.Generic;
using System.Text;

namespace tjta3501
{
    /// <summary>
    /// Yksinkertainen komentorivisysteemi. Omassa tiedostossaan, jotta
    /// Program.cs sisältäisi lähinnä tietokantakyselyihin liittyvää koodia.
    /// </summary>
    public class CommandEngine
    {
        public readonly Dictionary<string, Action<string[]>> commands;

        private StringBuilder printout;


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
                Console.Clear();
                Help();
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
                    ConsoleColor tmp = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Komentoa \'{command}\' ei tunnistettu.\n");
                    Console.ForegroundColor = tmp;
                }
            }
        }


        private void Exit(string[] args)
        {
            Environment.Exit(0);
        }


        private void Help()
        {
            Logo();
            printout = new StringBuilder("");
            printout.Append("KOMENTO          KUVAUS\n\n");
            printout.Append("kirjaudu [id]    Kirjaudu sisään.\n");
            printout.Append("poistu           Kirjaudu ulos.\n");
            printout.Append("kokoelma         Listaa omistamasi pelit (vaatii kirjautumisen).\n");
            printout.Append("hae [hakusana]   Hae pelejä.\n");
            printout.Append("sulje            Sulje sovellus.\n");
            Console.WriteLine(printout.ToString());
        }


        private void Logo()
        {
            printout = new StringBuilder("");
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
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
