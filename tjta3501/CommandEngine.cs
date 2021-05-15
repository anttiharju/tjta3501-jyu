using System;
using System.Collections.Generic;
using System.Text;

namespace hoyry
{
    /// <summary>
    /// Yksinkertainen komentorivisysteemi. Omassa tiedostossaan, jotta
    /// Program.cs sisältäisi lähinnä tietokantakyselyihin liittyvää koodia.
    /// </summary>
    public class CommandEngine
    {
        public readonly Dictionary<string, Action<string[]>> commands;
        private StringBuilder tuloste;

        public CommandEngine()
        {
            commands = new Dictionary<string, Action<string[]>>();
            AddCommand("apua", Help);
            AddCommand("help", Help);
            AddCommand("poistu", Exit);
            AddCommand("exit", Exit);
            Help();
        }


        public void AddCommand(string command, Action<string[]> method)
        {
            commands.Add(command, method);
        }


        public void Run()
        {
            while (true)
            {
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
                    Help(args);
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


        private void Help(string[] args = null)
        {
            Logo();
            tuloste = new StringBuilder("");
            tuloste.Append("KOMENTO          KUVAUS\n\n");
            tuloste.Append("apua             Tämä näkymä\n");
            tuloste.Append("poistu           Sulje sovellus\n");
            Console.WriteLine(tuloste.ToString());
        }


        private void Logo()
        {
            tuloste = new StringBuilder("");
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            tuloste.Append(" _   _                        \n");
            tuloste.Append("| | | | *_*  _   _ _ __ _   _ \n");
            tuloste.Append("| |_| |/ _ \\| | | | '__| | | |\n");
            tuloste.Append("|  _  | (_) | |_| | |  | |_| |\n");
            tuloste.Append("|_| |_|\\___/ \\__, |_|   \\__, |\n");
            tuloste.Append("             |___/      |___/ \n");
            Console.WriteLine(tuloste.ToString());
            Console.ForegroundColor = tmp;
        }
    }
}
