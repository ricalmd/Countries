using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Biblioteca.Services
{
    public class DataCountry
    {
        private SQLiteConnection liteConnection;

        private SQLiteCommand command;

        private Dialog dialog;

        public DataCountry()
        {
            dialog = new Dialog();

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
                    "CREATE TABLE IF NOT EXISTS countries(Name VARCHAR(100), " +
                    "Capital VARCHAR(100), Region VARCHAR(100), Subregion VARCHAR(100), Population INT, Gini REAL, " +
                    "Flag VARCHAR(100), NumericCode CHAR(3));" +
                    "CREATE TABLE IF NOT EXISTS currencies(Name VARCHAR(100), Code VARCHAR(10), Symbol CHAR(1), " +
                    "Country VARCHAR(100)REFERENCES countries(Name))";

                command = new SQLiteCommand(sqlcommand, liteConnection);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void SaveCurrency(List<Currency> currencies, string name)
        {
            try
            {
                string verif = null;

                if (currencies != null)
                {
                    foreach (var currency in currencies)
                    {
                        if(currency.Name != null)
                        {
                            verif = currency.Name.Replace("'", "´");
                        }
                        string sql =
                                string.Format("INSERT INTO currencies(Name,Code,Symbol,Country)" +
                                "VALUES('{0}','{1}','{2}','{3}');",
                                verif, currency.Code, currency.Symbol, name.Replace("'", "´"));

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
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
                            string.Format("INSERT INTO countries(Name,Capital,Region,Subregion,Population,Gini,Flag,NumericCode)" +
                            "VALUES('{0}','{1}','{2}','{3}',{4},'{5}','{6}','{7}');",
                            country.Name.Replace("'", "´"), country.Capital.Replace("'", "´"), country.Region,
                            country.Subregion, country.Population, country.Gini, country.Flag, country.NumericCode);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                        SaveCurrency(country.Currencies, country.Name);
                    }
                    liteConnection.Close();
                }
                else
                {
                    dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public List<Country> GetData()
        {
            List<Country> countries = new List<Country>();

            try
            {
                string sql = "SELECT Name,Capital,Region,Subregion,Population,Gini,Flag,NumericCode FROM countries";

                command = new SQLiteCommand(sql, liteConnection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = (string)reader["Name"];
                    GetCurrency(name);

                    countries.Add(new Country
                    {
                        Name = (string)reader["Name"],
                        Capital = (string)reader["Capital"],
                        Region = (string)reader["Region"],
                        Subregion = (string)reader["Subregion"],
                        Population = (int)reader["Population"],
                        Gini = (double)reader["Gini"],
                        Flag = (string)reader["Flag"],
                        NumericCode = (string)reader["NumericCode"],
                        Currencies = GetCurrency(name)
                    });
                }
                liteConnection.Close();

                return countries;
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
                return null;
            }
        }

        public List<Currency> GetCurrency(string name)
        {
            List<Currency> currencies = new List<Currency>();

            try
            {
                string sql = $"SELECT Name,Code,Symbol FROM currencies WHERE Country='{name.Replace("'","´")}'";

                command = new SQLiteCommand(sql, liteConnection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    currencies.Add(new Currency
                    {
                        Name = (string)reader["Name"],
                        Code = (string)reader["Code"],
                        Symbol = (string)reader["Symbol"]
                    });
                }

                return currencies;
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
                return null;
            }
        }

        public void DeleteData()
        {
            try
            {
                string sql = "DELETE FROM countries;DELETE FROM currencies";

                command = new SQLiteCommand(sql, liteConnection);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }
    }
}
