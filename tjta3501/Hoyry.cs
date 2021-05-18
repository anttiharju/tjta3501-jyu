using System;
using System.Text;
using Npgsql;

namespace tjta3501
{
    public class Hoyry
    {
        private readonly CommandEngine engine;
        private readonly string connectionString = "Host=192.168.1.23;Username=hoyry;Password=hl3;Database=tjta3501";

        private StringBuilder printout;
        private int pelaajaid;
        private int nimiLength;
        private int genreLength;
        private int kehittajaLength;
        private int julkaisijaLength;


        public Hoyry(CommandEngine engine)
        {
            Console.Clear();
            this.engine = engine;
            engine.AddCommand("kirjaudu", Kirjaudu);     /// tietokantaoperaatio
            engine.AddCommand("kokoelma", Kokoelma);     /// tietokantaoperaatio
            engine.AddCommand("hae", Hae);               /// tietokantaoperaatio
            engine.AddCommand("osta", Osta);             /// kirjoitusoperaatio
            engine.AddCommand("arvostele", Arvostele);   /// kirjoitusoperaatio
            engine.AddCommand("ulos", KirjauduUlos);
            engine.AddCommand("arvostelut", Arvostelut); /// tietokantaoperaatio, raakile koska ylimääräinen
            Calibrate();
            engine.Run();
        }


        public void Kirjaudu(string[] args)
        {
            if (args.Length > 0)
            {
                if (!int.TryParse(args[0], out int tmp))
                {
                    engine.Error("Anna pelaajaid numerona!");
                    engine.Continue();
                    return;
                }

                string sql = $"SELECT nimimerkki FROM pelaaja WHERE id = {tmp} AND banned = 'false'";
                NpgsqlConnection connection = Connect();
                string s = new NpgsqlCommand(sql, connection).ExecuteScalar()?.ToString();
                if (s == null)
                {
                    engine.Error($"Id {tmp} ei vastaa yhtään pelaajaa tai pelaajalla on porttikielto.");
                }
                else
                {
                    engine.Success($"Tervetuloa takaisin, {s}.");
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


        public void Kokoelma(string[] args)
        {
            printout = new StringBuilder();
            if (RequireLogin()) return;

            string sql = $"SELECT p.id, p.nimi, p.genre, k.nimi AS kehittäjä, j.nimi AS julkaisija, p.hinta, p.ikasuositus AS ikäsuositus, p.vuosi AS julkaisuvuosi FROM peli p, omistaa o, pelaaja pe, kehittaja k, julkaisija j WHERE k.id = p.id_kehittaja AND j.id = p.id_julkaisija AND p.id = o.id_peli AND o.id_pelaaja = pe.id AND pe.id = {pelaajaid}";
            NpgsqlConnection connection = Connect();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader r = cmd.ExecuteReader();
            PrintPeli(r);
            connection.Close();
            if (args != null)
                engine.Continue();
        }


        public void Hae(string[] args)
        {
            if (RequireArgs(1, args)) return;
            printout = new StringBuilder();

            string sql = $"SELECT p.id, p.nimi, p.genre, k.nimi AS kehittäjä, j.nimi AS julkaisija, p.hinta, p.ikasuositus AS ikäsuositus, p.vuosi AS julkaisuvuosi FROM peli p, kehittaja k, julkaisija j WHERE k.id = p.id_kehittaja AND j.id = p.id_julkaisija AND p.nimi ILIKE '%{args[0]}%' ORDER BY levenshtein(p.nimi, '{args[0]}'), p.id";
            NpgsqlConnection connection = Connect();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader r = cmd.ExecuteReader();
            PrintPeli(r);
            connection.Close();

            engine.Continue();
        }


        public void Osta(string[] args)
        {
            if (RequireLogin()) return;
            if (RequireArgs(1, args)) return;

            if (int.TryParse(args[0], out int peliid))
            {
                string sql = $"insert into omistaa (id_pelaaja, id_peli, minuutit) values ({pelaajaid}, {peliid}, 0)";
                NpgsqlConnection connection = Connect();
                try
                {
                    new NpgsqlCommand(sql, connection).ExecuteScalar()?.ToString();
                    engine.Success("Osto onnistui!");
                }
                catch (PostgresException pe)
                {
                    engine.Error(pe.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                engine.Error("Anna peli-id numerona!");
            }
            engine.Continue();
        }


        public void Arvostele(string[] args)
        {
            if (RequireLogin()) return;
            peliid:
            Kokoelma(null);
            int peliid = KysyNumero();
            if (peliid == 0) goto peliid;
            suosittelu:
            string suosittelu = KysySuosittelua();
            if (suosittelu == "") goto suosittelu;
            string arvostelu = KysyArvostelu();

            string sql = $"insert into arvostelu (id_pelaaja, id_peli, suosittelee, viesti) values ({pelaajaid}, {peliid}, {suosittelu}, '{arvostelu}');";
            NpgsqlConnection connection = Connect();
            try
            {
                new NpgsqlCommand(sql, connection).ExecuteScalar()?.ToString();
                engine.Success("Arvostelu lähetetty!");
            }
            catch (PostgresException pe)
            {
                engine.Error(pe.Message);
            }
            finally
            {
                connection.Close();
            }
            engine.Continue();
        }


        private string KysyArvostelu()
        {
            Console.Write("\nKirjoita arvostelusi: ");
            return Console.ReadLine();
        }


        private string KysySuosittelua()
        {
            Console.Write("\nSuositteletko peliä? (y/n): ");
            string[] input = Console.ReadLine().ToLower().Split(' ');
            if (input != null)
            {
                if (input[0] == "y")
                {
                    return "true";
                }
                if (input[0] == "n")
                {
                    return "false";
                }
            }
            return "";
        }


        private int KysyNumero()
        {
            Console.Write("\nAnna arvosteltavan pelin id: ");
            string[] input = Console.ReadLine().ToLower().Split(' ');

            if (input.Length > 0 && int.TryParse(input[0], out int peliid))
            {
                string sql = $"SELECT p.id FROM peli p, omistaa o WHERE p.id = o.id_peli AND o.id_pelaaja = {pelaajaid} AND p.id IN ({peliid})";
                NpgsqlConnection connection = Connect();
                try
                {
                    string s = new NpgsqlCommand(sql, connection).ExecuteScalar()?.ToString();
                    if (s == null)
                    {
                        Console.Clear();
                        engine.Error("Anna jonkun omistamasi pelin numero!");
                        return 0;
                    }
                }
                catch (PostgresException pe)
                {
                    engine.Error(pe.Message);
                    engine.Continue();
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                Console.Clear();
                engine.Error("Anna jonkun omistamasi pelin numero!");
                return 0;
            }
            return peliid;
        }


        public void Arvostelut(string[] args)
        {
            printout = new StringBuilder();
            if (RequireLogin()) return;

            string sql = $"SELECT suosittelee, viesti FROM arvostelu WHERE id_pelaaja = {pelaajaid}";
            NpgsqlConnection connection = Connect();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
            {
                string suosittelee = r.GetString(0);
                string viesti = r.GetString(1);
                printout.Append($"{suosittelee}\t{viesti}\n");
            }
            connection.Close();
            Console.WriteLine(printout.ToString());
            engine.Continue();
        }


        private bool RequireLogin()
        {
            if (pelaajaid == 0)
            {
                engine.Error("Kirjaudu ensin sisään!");
                engine.Continue();
            }
            return pelaajaid == 0;
        }


        private bool RequireArgs(int lkm, string[] args)
        {
            if (args.Length != 1)
            {
                if (args.Length == 0)
                {
                    engine.Error($"Vaaditaan {lkm} argumentti(a)!");
                }
                else
                {
                    engine.Error("Argumentteja on liikaa!");
                }
                engine.Continue();
            }
            return args.Length != 1;
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


        public void KirjauduUlos(string[] args)
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
