using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace Biblioteca.Services
{
    public class Data
    {
        private SQLiteConnection liteConnection;

        private SQLiteCommand command;

        public Data()
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }
            var path = @"Data\Countries.sqlite";

            try
            {
                liteConnection = new SQLiteConnection("Data Source=" + path);
                liteConnection.Open();

                string sqlcommand =
                    "CREATE TABLE IF NOT EXISTS countries(Name VARCHAR(100),Capital VARCHAR(100), Region VARCHAR(100)," +
                    "Subregion VARCHAR(100), Population INT, Gini REAL, Flag VARCHAR(100))";

                command = new SQLiteCommand(sqlcommand, liteConnection);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERRO", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveData(List<Country> countries)
        {
            try
            {
                if (countries != null)
                {
                    foreach (var country in countries)
                    {
                        string sql =
                            string.Format("INSERT INTO countries(Name,Capital,Region,Subregion,Population,Gini,Flag)" +
                            "VALUES('{0}','{1}','{2}','{3}',{4},'{5}','{6}')",
                            country.Name.Replace("'", "´"), country.Capital.Replace("'", "´"), country.Region,
                            country.Subregion, country.Population, country.Gini, country.Flag);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                    liteConnection.Close();
                }
                else
                {
                    MessageBox.Show("Houve uma falha ao carregar dados. Favor reiniciar programa",
                        "ERRO", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERRO", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<Country> GetData()
        {
            List<Country> countries = new List<Country>();

            try
            {
                string sql = "SELECT Name,Capital,Region,Subregion,Population,Gini,Flag FROM countries";

                command = new SQLiteCommand(sql, liteConnection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    countries.Add(new Country
                    {
                        Name = (string)reader["Name"],
                        Capital = (string)reader["Capital"],
                        Region = (string)reader["Region"],
                        Subregion = (string)reader["Subregion"],
                        Population = (int)reader["Population"],
                        Gini = (double)reader["Gini"],
                        Flag = (string)reader["Flag"]
                    });
                }
                liteConnection.Close();

                return countries;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERRO", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public void DeleteData()
        {
            try
            {
                string sql = "DELETE FROM countries";

                command = new SQLiteCommand(sql, liteConnection);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERRO", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
