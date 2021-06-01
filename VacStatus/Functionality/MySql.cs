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
                    return true;
                }
                catch (MySqlException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            connection.Close();
            return false;
        }

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
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
                return true;
            }
        }

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
            }
        }
    }
}
