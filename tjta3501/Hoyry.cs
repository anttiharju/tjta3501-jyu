﻿using System;
using System.Text;
using Npgsql;

namespace tjta3501
{
    public class Hoyry
    {
        private readonly CommandEngine engine;
        private readonly string connectionString = "Host=192.168.1.23;Username=hoyry;Password=hl3;Database=tjta3501";

        private int pelaajaid;
        private StringBuilder printout;
        private int genreLength;
        private int nimiLength;
        private int kehittajaLength;
        private int julkaisijaLength;


        public Hoyry(CommandEngine engine)
        {
            this.engine = engine;
            engine.AddCommand("kirjaudu", Kirjaudu);
            engine.AddCommand("poistu", Poistu);
            engine.AddCommand("kokoelma", Kokoelma);
            engine.AddCommand("hae", Hae);
            Calibrate();
            engine.Run();
        }


        public void Hae(string[] args)
        {
            if (args.Length != 1)
            {
                if (args.Length == 0)
                {
                    engine.Error("Tarvitaan hakusana!");
                }
                else
                {
                    engine.Error("Liikaa hakusanoja!");
                }
                engine.Continue();
                return;
            }

            printout = new StringBuilder();

            string sql = $"SELECT p.id, p.nimi, p.genre, k.nimi AS kehittäjä, j.nimi AS julkaisija, p.hinta, p.ikasuositus AS ikäsuositus, p.vuosi AS julkaisuvuosi FROM peli p, kehittaja k, julkaisija j WHERE k.id = p.id_kehittaja AND j.id = p.id_julkaisija AND p.nimi ILIKE '%{args[0]}%' ORDER BY levenshtein(p.nimi, '{args[0]}'), p.id";
            NpgsqlConnection connection = Connect();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader r = cmd.ExecuteReader();
            PrintPeli(r);
            connection.Close();

            engine.Continue();
        }


        public void Kokoelma(string[] args)
        {
            printout = new StringBuilder();
            if (pelaajaid == 0)
            {
                engine.Error("Kirjaudu ensin sisään!");
                engine.Continue();
                return;
            }

            string sql = $"SELECT p.id, p.nimi, p.genre, k.nimi AS kehittäjä, j.nimi AS julkaisija, p.hinta, p.ikasuositus AS ikäsuositus, p.vuosi AS julkaisuvuosi FROM peli p, omistaa o, pelaaja pe, kehittaja k, julkaisija j WHERE k.id = p.id_kehittaja AND j.id = p.id_julkaisija AND p.id = o.id_peli AND o.id_pelaaja = pe.id AND pe.id = {pelaajaid}";
            NpgsqlConnection connection = Connect();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader r = cmd.ExecuteReader();
            PrintPeli(r);
            connection.Close();

            engine.Continue();
        }


        public void Kirjaudu(string[] args)
        {
            if (args.Length > 0)
            {
                int tmp = int.Parse(args[0]);

                NpgsqlConnection connection = Connect();
                string sql = $"SELECT nimimerkki FROM pelaaja WHERE id = {tmp} AND ban_pvm IS NULL";
                string s = new NpgsqlCommand(sql, connection).ExecuteScalar()?.ToString();
                if (s == null)
                {
                    engine.Error($"Id {tmp} ei vastaa yhtään pelaajaa tai pelaajalla on porttikielto.");
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
                engine.Error("Komento vaatii argumentiksi pelaajaid:n.\nVoit myös kirjautua nopeasti asetuksella -f.\n\nkirjaudu [id] -f");
            }
            if (args.Length < 2 || args[1] != "-f")
            {
                engine.Continue();
            }
        }


        public void PrintPeli(NpgsqlDataReader r)
        {
            printout.Append($"id   {Pad("nimi", nimiLength)} {Pad("genre", genreLength)} {Pad("kehittäjä", kehittajaLength)} {Pad("julkaisija", julkaisijaLength)} hinta\tikasuos\tvuosi\n\n");
            while (r.Read())
            {
                int id = r.GetInt32(0);
                string nimi = r.GetString(1);
                string genre = r.GetString(2);
                string kehittaja = r.GetString(3);
                string julkaisija = r.GetString(4);
                string hinta = string.Format("{0:00.00}", r.GetDouble(5)).Replace('.', ',') + "e";
                int ikasuositus = r.GetInt32(6);
                int vuosi = r.GetInt32(7);
                printout.Append($"{id,-4} {Pad(nimi, nimiLength)} {Pad(genre, genreLength)} {Pad(kehittaja, kehittajaLength)} {Pad(julkaisija, julkaisijaLength)} {hinta}\t{ikasuositus}\t{vuosi}\n");
            }
            Console.Write(printout.ToString());
        }


        public void Poistu(string[] args)
        {
            pelaajaid = 0;
        }


        private NpgsqlConnection Connect()
        {
            NpgsqlConnection tmp = new NpgsqlConnection(connectionString);
            tmp.Open();
            return tmp;
        }


        private void Calibrate()
        {
            NpgsqlConnection connection = Connect();
            genreLength = ReadValue($"SELECT char_length(genre) AS pituus FROM peli ORDER BY pituus DESC LIMIT 1", connection);
            nimiLength = ReadValue($"SELECT char_length(nimi) AS pituus FROM peli ORDER BY pituus DESC LIMIT 1", connection);
            kehittajaLength = ReadValue($"SELECT char_length(nimi) AS pituus FROM kehittaja ORDER BY pituus DESC LIMIT 1", connection);
            julkaisijaLength = ReadValue($"SELECT char_length(nimi) AS pituus FROM julkaisija ORDER BY pituus DESC LIMIT 1", connection);
            connection.Close();
        }


        private string Pad(string toPad, int width)
        {
            return toPad + new string(new char[width - toPad.Length]).Replace('\0', ' ');
        }


        private int ReadValue(string sql, NpgsqlConnection connection)
        {
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

            using NpgsqlDataReader r = cmd.ExecuteReader();

            int tmp = 0;
            while (r.Read())
            {
                tmp = r.GetInt32(0);
            }
            return tmp;
        }
    }
}
