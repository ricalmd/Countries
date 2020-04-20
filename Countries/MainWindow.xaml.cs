using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Biblioteca;
using Biblioteca.Services;
using Svg;

namespace Countries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Network network;

        Data data;

        Api api;

        List<Country> Countries;

        string url = "https://restcountries.eu",
            path = "/rest/v2/all?fields=name;capital;region;subregion;population;gini;flag",
            equal = string.Empty;
        int num;
        bool verif;

        public MainWindow()
        {
            InitializeComponent();
            network = new Network();
            data = new Data();
            api = new Api();
            LoadCountries();
            ReportProgress();
        }

        private void btnVerPais_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(cbxListaPaises.SelectedItem != null) 
                {
                    foreach (Country country in Countries)
                    {
                        if (country.Name == cbxListaPaises.Text && country.Name != equal)
                        {
                            lblCapital.Content = $"Name: {country.Name}{Environment.NewLine}Capital: {country.Capital}" +
                                $"{Environment.NewLine}Region: {country.Region}{Environment.NewLine}Subregion: " +
                                $"{country.Subregion}{Environment.NewLine}Population: {country.Population}" +
                                $"{Environment.NewLine}Gini coefficient: {country.Gini}";

                            ConvertHtmlToImage(country.Name);
                            equal = country.Name;

                            lblAvisoImg.Content = string.Empty;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Selecione uma opção", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            catch(Exception ex)
            {
                imgFlag.Source = null;
                lblAvisoImg.Content = ex.Message;
            }
        }

        private void ConvertHtmlToImage(string name)
        {
            var svgDocument = SvgDocument.Open($"FlagImg/{name.Replace("´", "'")}.svg");  
            svgDocument.ShapeRendering = SvgShapeRendering.Auto;

            Bitmap bmp = svgDocument.Draw(400, 200);

            if (verif == true)
            {
                num = 0;
                verif = false;
            }
            else if(verif == false)
            {
                num = 1;
                verif = true;
            }

            bmp.Save($"img{num}.jpg");

            FlagImage();
        }

        private void FlagImage()
        {
            BitmapImage bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + $"img{num}.jpg", UriKind.Absolute);
            bitmapImage.EndInit();

            imgFlag.Source = bitmapImage;
        }

        private async Task LoadApiCountries()
        {
            var response = await api.GetCountries(url, path);

            Countries = (List<Country>)response.Result;

            if (Countries != null)
            {
                LoadFlags();
            }

            data.DeleteData();

            data.SaveData(Countries);
        }

        private async void LoadCountries()
        {
            var connection = network.CheckConnection();

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

        private async void LoadFlags()
        {
            WebClient client = new WebClient();

            try
            {
                foreach (Country country in Countries)
                {
                    client.DownloadFile(country.Flag, $"FlagImg/{country.Name}.svg");
                    await Task.Delay(50);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERRO", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadLocalCountries()
        {
            Countries = data.GetData();
        }

        private async void ReportProgress()
        {
            while (!api.GetCountries(url, path).IsCompleted)
            {
                pgbProgresso.Value++;
                await Task.Delay(1);
            }
        }
    }
}
