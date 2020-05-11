using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

        DataCountry Data;

        Api Api;

        Dialog Dialog;

        List<Country> Countries;

        string url = "https://restcountries.eu",
            path = "/rest/v2/all?fields=name;capital;region;subregion;population;gini;flag;numericCode;currencies",
            textCurrency;

        public MainWindow()
        {
            InitializeComponent();
            Network = new Network();
            Data = new DataCountry();
            Api = new Api();
            Dialog = new Dialog();
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
                    if (Countries.IndexOf(country).Equals(cbxListaPaises.SelectedIndex))
                    {
                        CountCurrency(country.Currencies);
                        VerifyEmpty(country);

                        lblPaises.Content = $"Name:{Environment.NewLine}    {country.Name}{Environment.NewLine}" +
                            $"Capital:{Environment.NewLine}    {country.Capital}" +
                            $"{Environment.NewLine}Region:{Environment.NewLine}    {country.Region}" +
                            $"{Environment.NewLine}Subregion:{Environment.NewLine}" +
                            $"    {country.Subregion}{Environment.NewLine}Population:{Environment.NewLine}" +
                            $"    {country.Population}" +
                            $"{Environment.NewLine}Gini coefficient:{Environment.NewLine}    {country.Gini}" +
                            $"{Environment.NewLine}Currency:{Environment.NewLine}{textCurrency}";
                        
                        ConvertSvgToImage(country.Name);
                        lblAvisoImg.Content = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Dialog.ShowMessage(ex.Message, "ERRO");
                lblAvisoImg.Content = "Bandeira ausente";
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

        private void CountCurrency(List<Currency> currencies)
        {
            int count = 0;

            foreach (var currency in currencies)
            {
                count++;
            }
            DisplayCurrency(currencies, count);
        }

        private void DisplayCurrency(List<Currency> currencies, int count)
        {
            int num = 0;
            
            string[] text1 = new string[count];
            string aux;
            textCurrency = string.Empty;
            
            foreach (var currency in currencies)
            {
                VerifyEmptyCurrency(currency);

                text1[num] = $"    {currency.Name}, {currency.Symbol}, {currency.Code}{Environment.NewLine}";
                aux = text1[num];
                textCurrency += aux;

                num++;
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
                Dialog.ShowMessage(ex.Message, "ERRO");
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

        private void VerifyEmptyCurrency(Currency currency)
        {
            if (currency.Name == null)
            {
                currency.Name = " - ";
            }
            if (currency.Symbol == null)
            {
                currency.Symbol = " - ";
            }
            if (currency.Code == null)
            {
                currency.Code = " - ";
            }
        }
    }
}
