using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using VacStatus.Local;

namespace VacStatus.Functionality
{
    class MySql
    {
        private static MySqlConnection connection;
        private static MySqlCommand command;
        Logger log = new Logger();

        //Funkcija pridedanti zmogu i duombaze
        public bool AddSuspect(AccountSummary summary)
        {
            EstablishDatabaseConnection();

            if (!IsThisSuspectInTheDatabase(summary.SteamId))
            {
                try
                {
                    string currTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    MySqlCommand cmd = new MySqlCommand($"INSERT INTO players (Nickname, SteamId, VacBanned, DateAdded) values ('{summary.Nickname}', '{summary.SteamId}', {summary.VacBanned}, '{currTime}');", connection);
                    cmd.ExecuteNonQuery();
                    connection.Close();

                    log.Log($"[{summary.SteamId}][{summary.Nickname}] Paskyra prideta sekmingai.", Logger.LogType.Info);

                    return true;
                }
                catch (MySqlException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = ConsoleColor.White;

                    log.Log($"MySqlError: [{ex.Message}]", Logger.LogType.Error);
                }
            }
            connection.Close();

            return false;
        }

        //Funkcija patikrinanti ar steamId jau yra duombazeje
        public bool IsThisSuspectInTheDatabase(string steamId)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand("select count(*) from players where steamid = '" + steamId + "'", connection);
                object obj = cmd.ExecuteScalar();
                if (Convert.ToInt32(obj) == 0)
                {
                    return false;
                }
                else
                {
                    log.Log($"[{steamId}] Paskyra rasta duombazeje", Logger.LogType.Warn);
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;

                log.Log($"MySqlError: [{ex.Message}]", Logger.LogType.Error);

                return true;
            }
        }

        //Paprasom prisijungti prie duombazes, jei nepavyksta konsoleje ismetama raudona zinute
        public void EstablishDatabaseConnection()
        {
            try
            {
                connection = new MySqlConnection();

                connection.ConnectionString = Configuration.jsonConfig.MySqlConnection;
                connection.Open();
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;

                log.Log($"MySqlError: [{ex.Message}]", Logger.LogType.Error);
            }
        }

        //Recheck funkcija kuri istraukia visu neuzbanintu zmoniu vardus ir steamId
        public List<AccountSummary> Recheck(bool vacBannedAlso)
        {
            log.Log($"Gaunamas profiliu sarasas.", Logger.LogType.Info);

            List<AccountSummary> columnData = new List<AccountSummary>();

            EstablishDatabaseConnection();

            var query = string.Empty;
            if (vacBannedAlso)
            {
                query = $"SELECT steamid,nickname,vacbanned FROM players";
            }
            else
            {
                query = $"SELECT steamid,nickname,vacbanned FROM players where VacBanned = false";
            }

            using (command = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnData.Add(new AccountSummary() {SteamId = reader.GetString(0),Nickname = reader.GetString(1),VacBanned = reader.GetBoolean(2) });
                    }
                }
            }

            connection.Close();


            return columnData;
        }

        //Funkcija gaunanti visu neuzbanintu zmoniu skaiciu
        public int CurrentPlayerCountInDatabase(bool vacBannedAlso)
        {
            EstablishDatabaseConnection();
            object obj;
            try
            {
                if (vacBannedAlso)
                {
                    MySqlCommand cmd = new MySqlCommand("select count(*) from players", connection);
                    obj = cmd.ExecuteScalar();
                }
                else
                {
                    MySqlCommand cmd = new MySqlCommand("select count(*) from players where VacBanned = false", connection);
                    obj = cmd.ExecuteScalar();
                }
            }
            catch (MySqlException ex)
            {
                obj = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;

                log.Log($"MySqlError: [{ex.Message}]", Logger.LogType.Error);
            }
            connection.Close();
            return Convert.ToInt32(obj);

        }

        //Funkcija kuri pakeicia zmogaus varda is duombazeje esancio i nauja
        public void ChangeNickname(string nicknameInDatabase, string currentNickname)
        {
            EstablishDatabaseConnection();

            try
            {
                command = new MySqlCommand();
                command.CommandText = $"UPDATE players SET nickname ='{currentNickname}' WHERE nickname = '{nicknameInDatabase}';";
                command.Connection = connection;
                command.ExecuteNonQuery();

                log.Log($"[{nicknameInDatabase}] Keiciamas vardas i [{currentNickname}]", Logger.LogType.Info);
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;

                log.Log($"MySqlError: [{ex.Message}]", Logger.LogType.Error);
            }

            connection.Close();
            GC.Collect();
        }

        public void DeemSteamIdBanned(string steamId)
        {
            log.Log($"Keiciamas [{steamId}] statusas i BANNED", Logger.LogType.Info);

            EstablishDatabaseConnection();

            string currTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                command = new MySqlCommand();
                command.CommandText = $"UPDATE players SET vacbanned = true, datebanned = '{currTime}' WHERE steamid = '{steamId}';";
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;

                log.Log($"MySqlError: [{ex.Message}]", Logger.LogType.Error);
            }

            connection.Close();
            GC.Collect();
        }

        public void Remove(string steamId)
        {
            EstablishDatabaseConnection();

            try
            {
                command = new MySqlCommand();
                command.CommandText = $"delete from players where steamid = '{steamId}';";
                command.Connection = connection;
                command.ExecuteNonQuery();

                log.Log($"[{steamId}] Sekmingai istrintas.", Logger.LogType.Info);
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;

                log.Log($"MySqlError: [{ex.Message}]", Logger.LogType.Error);
            }

            connection.Close();
            GC.Collect();
        }

        public void ClearList()
        {
            EstablishDatabaseConnection();

            try
            {
                command = new MySqlCommand();
                command.CommandText = $"delete from players;";
                command.Connection = connection;
                command.ExecuteNonQuery();

                log.Log($"Visos duombazes paskyros sekmingai istrintos.", Logger.LogType.Info);
                log.Log($"------------------END------------------", Logger.LogType.Info);
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;

                log.Log($"MySqlError: [{ex.Message}]", Logger.LogType.Error);
            }

            connection.Close();
            GC.Collect();
        }
    }
}
