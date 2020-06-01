using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Biblioteca.Models;

namespace Biblioteca.Services
{
    public class DataCountry
    {
        private SQLiteConnection liteConnection;

        private SQLiteCommand command;

        private Dialog dialog;

        public bool Confirm { get; set; }

        /// <summary>
        /// If the Data folder does not exist, a new one is created and the path to it is also created. The SQLiteConnection 
        /// class is instantiated and the connection is made to the database, and an SQL command is sent to create the tables
        /// where the data will be saved. The SQLiteCommand class is instantiated, using the SQLiteConnection instance and 
        /// the variable containing the SQL command as arguments. Finally, the ExecuteNonQuery() method allows the command 
        /// to be executed.
        /// </summary>
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
                    "CREATE TABLE IF NOT EXISTS countries(Name VARCHAR(50), Alpha2Code CHAR(2)PRIMARY KEY, " +
                    "Alpha3Code CHAR(3), Capital VARCHAR(50), Region VARCHAR(50), Subregion VARCHAR(50), " +
                    "Population INT, Demonym VARCHAR(50),Area REAL, Gini REAL, " +
                    "Flag VARCHAR(100), NumericCode CHAR(3), NativeName VARCHAR(50), Cioc CHAR(3));" +
                    "CREATE TABLE IF NOT EXISTS currencies(Name VARCHAR(50), Code VARCHAR(10), Symbol CHAR(1), " +
                    "Country CHAR(2)REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS languages(Iso639_1 CHAR(2), Iso639_2 CHAR(3), Name VARCHAR(50), " +
                    "NativeName VARCHAR(50), Country CHAR(2)REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS translations(De VARCHAR(50), Es VARCHAR(50), Fr VARCHAR(50), " +
                    "Ja VARCHAR(50), It VARCHAR(50), Br VARCHAR(50), Pt VARCHAR(50), Nl VARCHAR(50), " +
                    "Hr VARCHAR(50), Fa VARCHAR(50), Country CHAR(2)REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS topLevelDomain(Domain VARCHAR(4),Country " +
                    "CHAR(2)REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS callingCodes(CallingCodes VARCHAR(4),Country CHAR(2)" +
                    "REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS altSpellings(AltSpellings VARCHAR(50),Country CHAR(2)" +
                    "REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS latlng(Lat VARCHAR(50),Lng VARCHAR(50),Country CHAR(2)" +
                    "REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS timezones(Timezones VARCHAR(10),Country CHAR(2)" +
                    "REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS borders(Borders CHAR(3),Country CHAR(2)" +
                    "REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS regionalBlocs(Acronym VARCHAR(10),Name VARCHAR(100)," +
                    "Country CHAR(2)REFERENCES countries(Alpha2Code));" +
                    "CREATE TABLE IF NOT EXISTS otherAcronyms(AcronymId VARCHAR(10)REFERENCES regionalBlocs" +
                    "(Acronym),OtherAcronyms VARCHAR(10));" +
                    "CREATE TABLE IF NOT EXISTS otherNames(AcronymId VARCHAR(10)REFERENCES regionalBlocs" +
                    "(Acronym),OtherNames VARCHAR(100))";

                command = new SQLiteCommand(sqlcommand, liteConnection);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        /// <summary>
        /// A command is executed to delete the data from the tables so that updated data is saved.
        /// </summary>
        public void DeleteData()
        {
            try
            {
                string sql = "DELETE FROM countries;DELETE FROM currencies;DELETE FROM languages;DELETE FROM translations;" +
                    "DELETE FROM topLevelDomain;DELETE FROM callingCodes;DELETE FROM altSpellings;DELETE FROM latlng;" +
                    "DELETE FROM timezones;DELETE FROM borders;DELETE FROM regionalBlocs;DELETE FROM otherAcronyms;" +
                    "DELETE FROM otherNames";

                command = new SQLiteCommand(sql, liteConnection);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        /// <summary>
        /// This function receives the value of the Alpha2Code property from GetData(), so that only fields associated with
        /// a given country are returned. An SQL command is created to select certain fields in the currencies table
        /// for the respective data to be displayed in the program in case there is no Internet connection. The Read() method,
        /// of the SQLiteDataReader class, allows the data in the table to be read and passed to the respective properties. 
        /// At the end, the list of currencies is returned.
        /// </summary>
        /// <param name="code"></param>
        /// <returns>currencies</returns>
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

        /// <summary>
        /// An SQL command is created to select certain fields in the countries table for the respective data to be displayed
        /// in the program in case there is no Internet connection. The Read() method, of the SQLiteDataReader class, allows
        /// the data in the table to be read and passed to the respective properties. At the same time, parameters are passed 
        /// to other functions that perform the same task. At the end, the connection to the database is closed and the list
        /// of countries is returned.
        /// </summary>
        /// <returns>countries</returns>
        public List<Country> GetData()
        {
            List<Country> countries = new List<Country>();

            try
            {
                string sql = "SELECT Name,Alpha2Code,Capital,Region,Subregion,Population,Gini,Flag,Area FROM countries";

                command = new SQLiteCommand(sql, liteConnection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string code = (string)reader["Alpha2Code"];
                    GetCurrency(code);
                    GetLanguages(code);

                    countries.Add(new Country
                    {
                        Name = (string)reader["Name"],
                        Capital = (string)reader["Capital"],
                        Region = (string)reader["Region"],
                        Subregion = (string)reader["Subregion"],
                        Population = (int)reader["Population"],
                        Gini = (double)reader["Gini"],
                        Flag = (string)reader["Flag"],
                        Area = (double)reader["Area"],
                        Currencies = GetCurrency(code),
                        Languages = GetLanguages(code)
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

        /// <summary>
        /// This function receives the value of the Alpha2Code property from GetData(), so that only fields associated with
        /// a given country are returned. An SQL command is created to select certain fields in the languages table
        /// for the respective data to be displayed in the program in case there is no Internet connection. The Read() method,
        /// of the SQLiteDataReader class, allows the data in the table to be read and passed to the respective properties. 
        /// At the end, the list of languages is returned.
        /// </summary>
        /// <param name="code"></param>
        /// <returns>languages</returns>
        public List<Language> GetLanguages(string code)
        {
            List<Language> languages = new List<Language>();
             
            try
            {
                string sql = $"SELECT Name,NativeName FROM languages WHERE Country='{code}'";

                command = new SQLiteCommand(sql, liteConnection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    languages.Add(new Language
                    {
                        Name = (string)reader["Name"],
                        NativeName = (string)reader["NativeName"]
                    });
                }

                return languages;
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
                return null;
            }
        }

        public void SaveAltSpellings(List<string> altSpellings, string code)
        {
            try
            {
                byte count = 1;
                char a = Convert.ToChar(34);

                while (count < altSpellings.Count)
                {
                    if (code != null)
                    {
                        string sql =
                                string.Format("INSERT INTO altSpellings(AltSpellings,Country)" +
                                "VALUES("+a+"{0}"+a+",'{1}');",
                                altSpellings[count], code);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                    }
                    count++;
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void SaveBorders(List<string> borders, string code)
        {
            try
            {
                byte count = 0;
               
                while (count < borders.Count)
                {
                    if (code != null)
                    {
                        string sql =
                                string.Format("INSERT INTO borders(Borders,Country)" +
                                "VALUES('{0}','{1}');",
                                borders[count], code);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                    }
                    count++;
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void SaveCallingCodes(List<string> callingCodes, string code)
        {
            try
            {
                byte count = 0;

                while (count < callingCodes.Count)
                {
                    if (code != null)
                    {
                        string sql =
                                string.Format("INSERT INTO callingCodes(CallingCodes,Country)" +
                                "VALUES('{0}','{1}');",
                                callingCodes[count], code);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                    }
                    count++;
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void SaveCurrency(List<Currency> currencies, string code)
        {
            try
            {
                if (currencies != null)
                {
                    foreach (var currency in currencies)
                    {
                        char a = Convert.ToChar(34);
                        string sql =
                                string.Format("INSERT INTO currencies(Name,Code,Symbol,Country)" +
                                "VALUES("+a+"{0}"+a+","+a+"{1}"+a+","+a+"{2}"+a+",'{3}');",
                                currency.Name, currency.Code, currency.Symbol, code);

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

        public async void SaveData(List<Country> countries)
        {
            try
            {
                if (countries != null)
                {
                    await Task.Run(() => 
                    { 
                        foreach (var country in countries)
                        {
                            var transaction = liteConnection.BeginTransaction();

                            char a = Convert.ToChar(34);
                            string sql =
                                string.Format("INSERT INTO countries(Name,Alpha2Code,Alpha3Code," +
                                "Capital,Region,Subregion,Population,Demonym,Area,Gini,Flag,NumericCode,NativeName," +
                                "Cioc)" +
                                "VALUES(" + a + "{0}" + a + ",'{1}','{2}'," + a + "{3}" + a + "," + a + "{4}" + a + "," + a + "{5}" +
                                "" + a + ",{6}," + a + "{7}" + a + ",'{8}','{9}'," + a + "{10}" + a + ",'{11}'" +
                                "," + a + "{12}" + a + ",'{13}');",
                                country.Name, country.Alpha2Code, country.Alpha3Code, country.Capital, country.Region,
                                country.Subregion, country.Population, country.Demonym, country.Area,
                                country.Gini, country.Flag, country.NumericCode, country.NativeName, country.Cioc);

                            command = new SQLiteCommand(sql, liteConnection);
                            command.ExecuteNonQuery();

                            SaveCurrency(country.Currencies, country.Alpha2Code);
                            SaveLanguage(country.Languages, country.Alpha2Code);
                            SaveTranslations(country.Translations.De, country.Translations.Es, country.Translations.Fr,
                                country.Translations.Ja, country.Translations.It, country.Translations.Br,
                                country.Translations.Pt, country.Translations.Nl, country.Translations.Hr,
                                country.Translations.Fa, country.Alpha2Code);
                            SaveTopLevelDomain(country.TopLevelDomain, country.Alpha2Code);
                            SaveCallingCodes(country.CallingCodes, country.Alpha2Code);
                            SaveAltSpellings(country.AltSpellings, country.Alpha2Code);
                            SaveLatlng(country.Latlng, country.Alpha2Code);
                            SaveTimezones(country.Timezones, country.Alpha2Code);
                            SaveBorders(country.Borders, country.Alpha2Code);
                            SaveRegionalBlocs(country.RegionalBlocs, country.Alpha2Code);

                            transaction.Commit(); 
                        }
                    });
                    liteConnection.Close();

                    Confirm = true;
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

        public void SaveLanguage(List<Language> languages, string code)
        {
            try
            {
                if (languages != null)
                {
                    foreach (var language in languages)
                    {
                        char a = Convert.ToChar(34);
                        string sql =
                                string.Format("INSERT INTO languages(Iso639_1,Iso639_2,Name,NativeName,Country)" +
                                "VALUES("+a+"{0}"+a+","+a+"{1}"+a+","+a+"{2}"+a+","+a+"{3}"+a+",'{4}');",
                                language.Iso639_1, language.Iso639_2, language.Name, language.NativeName, code);

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

        public void SaveLatlng(List<double> latlng, string code)
        {
            try
            {
                byte count = 0;

                while (count < latlng.Count - 1)
                {
                    if (code != null)
                    {
                        string sql =
                                string.Format("INSERT INTO latlng(Lat,Lng,Country)" +
                                "VALUES('{0}','{1}','{2}');",
                                latlng[count],latlng[count + 1], code);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                    }
                    count++;
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void SaveOtherAcronyms(string acronym, List<object> otherAcronyms)
        {
            try
            {
                byte count = 0;

                while (count < otherAcronyms.Count)
                {
                    if (acronym != null)
                    {
                        string sql =
                                string.Format("INSERT INTO otherAcronyms(AcronymId,OtherAcronyms)" +
                                "VALUES('{0}','{1}');",
                                acronym, otherAcronyms[count]);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                    }
                    count++;
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void SaveOtherNames(string acronym, List<object> otherNames)
        {
            try
            {
                byte count = 0;

                while (count < otherNames.Count)
                {
                    if (acronym != null)
                    {
                        string sql =
                                string.Format("INSERT INTO otherNames(AcronymId,OtherNames)" +
                                "VALUES('{0}','{1}');",
                                acronym, otherNames[count]);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                    }
                    count++;
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void SaveRegionalBlocs(List<RegionalBloc> regionalBlocs, string code)
        {
            try
            {
                if (regionalBlocs != null)
                {
                    foreach (var regionalBloc in regionalBlocs)
                    {
                        char a = Convert.ToChar(34);
                        string sql =
                                string.Format("INSERT INTO regionalBlocs(Acronym,Name,Country)" +
                                "VALUES('{0}',"+a+"{1}"+a+",'{2}');",
                                regionalBloc.Acronym, regionalBloc.Name, code);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                        
                        SaveOtherAcronyms(regionalBloc.Acronym, regionalBloc.OtherAcronyms);
                        SaveOtherNames(regionalBloc.Acronym, regionalBloc.OtherNames);
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

        public void SaveTimezones(List<string> timezones, string code)
        {
            try
            {
                byte count = 0;

                while (count < timezones.Count)
                {
                    if (code != null)
                    {
                        string sql =
                                string.Format("INSERT INTO timezones(Timezones,Country)" +
                                "VALUES('{0}','{1}');",
                                timezones[count], code);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                    }
                    count++;
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void SaveTopLevelDomain(List<string> topLevelDomain, string code)
        {
            try
            {
                byte count = 0;

                while (count < topLevelDomain.Count)
                {
                    if (code != null)
                    {
                        string sql =
                                string.Format("INSERT INTO topLevelDomain(Domain,Country)" +
                                "VALUES('{0}','{1}');",
                                topLevelDomain[count], code);

                        command = new SQLiteCommand(sql, liteConnection);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        dialog.ShowMessage("Houve uma falha ao carregar dados. Favor reiniciar programa", "ERRO");
                    }
                    count++;
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        public void SaveTranslations(string de, string es, string fr, string ja, string it, string br, string pt, 
                            string nl, string hr, string fa, string code)
        {
            try
            {
                if (code != null)
                {
                    char a = Convert.ToChar(34);
                    string sql = 
                            string.Format("INSERT INTO translations(De,Es,Fr,Ja,It,Br,Pt,Nl,Hr,Fa,Country)" +
                            "VALUES("+a+"{0}"+a+","+a+"{1}"+a+","+a+"{2}"+a+","+a+"{3}"+a+","+a+"{4}"+a+","+a+"{5}" +
                            ""+a+","+a+"{6}"+a+","+a+"{7}"+a+","+a+"{8}"+a+","+a+"{9}"+a+",'{10}');",
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
