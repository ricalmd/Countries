using Biblioteca.Models;
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
                    "CREATE TABLE IF NOT EXISTS countries(Name VARCHAR(100), Alpha2Code CHAR(2)PRIMARY KEY, " +
                    "Alpha3Code CHAR(3), Capital VARCHAR(100), Region VARCHAR(100), Subregion VARCHAR(100), " +
                    "Population INT, Demonym VARCHAR(50),Area REAL, Gini REAL, " +
                    "Flag VARCHAR(100), NumericCode CHAR(3), NativeName VARCHAR(100), Cioc CHAR(3));" +
                    "CREATE TABLE IF NOT EXISTS currencies(Name VARCHAR(100), Code VARCHAR(10), Symbol CHAR(1), " +
                    "Country CHAR(2)REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS languages(Iso639_1 CHAR(2), Iso639_2 CHAR(3), Name VARCHAR(50), " +
                    "NativeName VARCHAR(50), Country CHAR(2)REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS translations(De VARCHAR(50), Es VARCHAR(50), Fr VARCHAR(50), " +
                    "Ja VARCHAR(50), It VARCHAR(50), Br VARCHAR(50), Pt VARCHAR(50), Nl VARCHAR(50), " +
                    "Hr VARCHAR(50), Fa VARCHAR(50), Country CHAR(2)REFERENCES countries(Alpha2Code))";

                command = new SQLiteCommand(sqlcommand, liteConnection);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void DeleteData()
        {
            try
            {
                string sql = "DELETE FROM countries;DELETE FROM currencies;DELETE FROM languages;DELETE FROM translations";

                command = new SQLiteCommand(sql, liteConnection);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public List<Currency> GetCurrency(string code)
        {
            List<Currency> currencies = new List<Currency>();

            try
            {
                string sql = $"SELECT Name,Code,Symbol FROM currencies WHERE Country='{code}'";

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

        public List<Country> GetData()
        {
            List<Country> countries = new List<Country>();

            try
            {
                string sql = "SELECT Name,Alpha2Code,Capital,Region,Subregion,Population,Gini,Flag FROM countries";

                command = new SQLiteCommand(sql, liteConnection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string code = (string)reader["Alpha2Code"];
                    GetCurrency(code);

                    countries.Add(new Country
                    {
                        Name = (string)reader["Name"],
                        Capital = (string)reader["Capital"],
                        Region = (string)reader["Region"],
                        Subregion = (string)reader["Subregion"],
                        Population = (int)reader["Population"],
                        Gini = (double)reader["Gini"],
                        Flag = (string)reader["Flag"],
                        Currencies = GetCurrency(code)
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

        public void SaveCurrency(List<Currency> currencies, string code)
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
                                verif, currency.Code, currency.Symbol, code);

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
                            string.Format("INSERT INTO countries(Name,Alpha2Code,Alpha3Code," +
                            "Capital,Region,Subregion,Population,Demonym,Area,Gini,Flag,NumericCode,NativeName," +
                            "Cioc)" +
                            "VALUES('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}'," +
                            "'{13}');",
                            country.Name.Replace("'", "´"), country.Alpha2Code, country.Alpha3Code,
                            country.Capital.Replace("'", "´"), country.Region,
                            country.Subregion, country.Population, country.Demonym, country.Area,
                            country.Gini, country.Flag, country.NumericCode, 
                            country.NativeName.Replace("'", "´"), country.Cioc);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                        SaveCurrency(country.Currencies, country.Alpha2Code);
                        SaveLanguage(country.Languages, country.Alpha2Code);
                        SaveTranslations(country.Translations.De, country.Translations.Es, country.Translations.Fr, 
                            country.Translations.Ja, country.Translations.It, country.Translations.Br, 
                            country.Translations.Pt, country.Translations.Nl, country.Translations.Hr,
                            country.Translations.Fa, country.Alpha2Code);
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

        private void SaveLanguage(List<Language> languages, string code)
        {
            try
            {
                if (languages != null)
                {
                    foreach (var language in languages)
                    {
                        string sql =
                                string.Format("INSERT INTO languages(Iso639_1,Iso639_2,Name,NativeName,Country)" +
                                "VALUES('{0}','{1}','{2}','{3}','{4}');",
                                language.Iso639_1, language.Iso639_2, language.Name, 
                                language.NativeName.Replace("'", "´"), code);

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

        private void SaveTranslations(string de, string es, string fr, string ja, string it, string br, string pt, 
                            string nl, string hr, string fa, string code)
        {
            try
            {
                if (code != null)
                { 
                    string sql =
                            string.Format("INSERT INTO translations(De,Es,Fr,Ja,It,Br,Pt,Nl,Hr,Fa,Country)" +
                            "VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}');",
                            de, es, fr, ja, it, br, pt, nl, hr, fa, code);

                    command = new SQLiteCommand(sql, liteConnection);

                    command.ExecuteNonQuery();
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
    }
}
