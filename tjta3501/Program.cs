using System;

namespace tjta3501
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string errors = "";

            Home(errors);

            string command;
            bool exit = false;

            while (!exit)
            {
                command = Console.ReadLine();
                command = command.ToLower();
                string[] commands = command.Split(' ');

                switch (commands[0])
                {
                    case "home":
                        Home(errors);
                        break;

                    case "exit":
                        exit = true;
                        break;

                    case "version":
                        Console.Clear();
                        Console.WriteLine("v1.1");
                        break;

                    default:
                        Home(errors);
                        if (command != "")
                        {
                            Console.WriteLine("Unknown command: " + command);
                            Console.WriteLine("");
                        }
                        break;
                }
            }
        }


        public static void Home(string errors)
        {
            Console.Clear();
            Logo();
            Console.WriteLine("COMMAND          DESCRIPTION");
            Console.WriteLine();
            Console.WriteLine("home             Return to this view");
            Console.WriteLine("version          Check version");
            Console.WriteLine("exit             Close application");
            if (errors != "")
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errors);
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine();
        }


        static void Logo() // https://youtu.be/dQw4w9WgXcQ
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@" _   _                        ");
            Console.WriteLine(@"| | | | *_*  _   _ _ __ _   _ ");
            Console.WriteLine(@"| |_| |/ _ \| | | | '__| | | |");
            Console.WriteLine(@"|  _  | (_) | |_| | |  | |_| |");
            Console.WriteLine(@"|_| |_|\___/ \__, |_|   \__, |");
            Console.WriteLine(@"             |___/      |___/ ");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
