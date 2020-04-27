using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Biblioteca;
using Biblioteca.Models;
using Biblioteca.Services;
using Svg;

namespace Countries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Network Network;

        Data Data;

        Api Api;

        Currency Currency;

        List<Country> Countries;

        string url = "https://restcountries.eu",
            path = "/rest/v2/all?fields=name;capital;region;subregion;population;gini;flag;currencies";

        public MainWindow()
        {
            InitializeComponent();
            Network = new Network();
            Data = new Data();
            Api = new Api();
            Currency = new Currency();
            LoadCountries();
            ReportProgress();
        }
        /// <summary>
        /// List of country names. By selecting an item from this list, the chosen data are sent to be 
        /// displayed in lblPaises (text) and imgFlag (flag figures).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxListaPaises_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                foreach (Country country in Countries)
                {
                    GetCurrency(country.Currencies);
                    
                    if (Countries.IndexOf(country).Equals(cbxListaPaises.SelectedIndex))
                    {
                        VerifyEmpty(country);

                        lblPaises.Content = $"Name: {country.Name}{Environment.NewLine}Capital: {country.Capital}" +
                            $"{Environment.NewLine}Region: {country.Region}{Environment.NewLine}Subregion: " +
                            $"{country.Subregion}{Environment.NewLine}Population: {country.Population}" +
                            $"{Environment.NewLine}Gini coefficient: {country.Gini}" +
                            $"{Environment.NewLine}Currency: {Currency.Name}; {Currency.Symbol}; {Currency.Code}";

                        ConvertSvgToImage(country.Name);

                        lblAvisoImg.Content = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                lblAvisoImg.Content = ex.Message;
                imgFlag.Source = null;
            }
        }

        private void ConvertSvgToImage(string name)
        {
            var svgDocument = SvgDocument.Open($"FlagImg/{name.Replace("´", "'")}.svg");
            
            MemoryStream memory = new MemoryStream();
            BitmapImage bitmapImage = new BitmapImage();
            
            using (var bmp = new Bitmap(svgDocument.Draw(400, 230)))
            {
                bmp.Save(memory, ImageFormat.Jpeg);
                memory.Position = 0;
                
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                memory.Close();
            }
            imgFlag.Source = bitmapImage;
        }

        private void GetCurrency(List<Currency> currencies)
        {
            foreach (Currency currency in currencies)
            {
                Currency.Symbol = currency.Symbol;
                Currency.Code = currency.Code;
                Currency.Name = currency.Name;
            }
        }

        private async Task LoadApiCountries()
        {
            var response = await Api.GetCountries(url, path);

            Countries = (List<Country>)response.Result;

            if (Countries != null)
            {
                LoadFlags();
            }

            Data.DeleteData();

            Data.SaveData(Countries);
        }

        private async void LoadCountries()
        {
            var connection = Network.CheckConnection();

            if (!connection.Connect)
            {
                LoadLocalCountries();
                lblAviso.Content = connection.Message;
            }
            else
            {
                await LoadApiCountries();
                lblAviso.Content = string.Empty;
            }

            cbxListaPaises.ItemsSource = Countries;
            cbxListaPaises.DisplayMemberPath = "Name";
        }

        private void LoadFlags()
        {
            WebClient client = new WebClient();

            try
            {
                foreach (Country country in Countries)
                {
                    client.DownloadFile(country.Flag, $"FlagImg/{country.Name}.svg");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERRO", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadLocalCountries()
        {
            Countries = Data.GetData();
        }

        private async void ReportProgress()
        {
            Response response = new Response();
            
            while (!response.Connect)
            {
                pgbProgresso.Value++;
                await Task.Delay(1);
            }
        }

        private void VerifyEmpty(Country country)
        {
            if (country.Capital == string.Empty)
            {
                country.Capital = " - ";
            }
            if (country.Region == string.Empty)
            {
                country.Region = " - ";
            }
            if (country.Subregion == string.Empty)
            {
                country.Subregion = " - ";
            }
        }
    }
}
