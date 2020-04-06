using Biblioteca;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            path = "/rest/v2/all?fields=name;capital;region;subregion;population;gini;flag";

        public MainWindow()
        {
            InitializeComponent();
            network = new Network();
            data = new Data();
            api = new Api();
            LoadCountries();
            ReportProgress();
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

        private async void ReportProgress()
        {
            while(!api.GetCountries(url, path).IsCompleted)
            {
                pgbProgresso.Value++;
                await Task.Delay(1);            
            }
        }

        private void LoadLocalCountries()
        {
            Countries = data.GetData();
        }

        private async Task LoadApiCountries()
        {
            var response = await api.GetCountries(url, path);

            Countries = (List<Country>)response.Result;

            data.DeleteData();

            data.SaveData(Countries);
        }

        private void btnVerPais_Click(object sender, RoutedEventArgs e)
        {
            foreach(Country country in Countries)
            {
                if (cbxListaPaises.SelectedItem != null)
                {
                    if (country.Name == cbxListaPaises.Text)
                    {
                        lblCapital.Content = $"Name: {country.Name}{Environment.NewLine}Capital: {country.Capital}" +
                            $"{Environment.NewLine}Region: {country.Region}{Environment.NewLine}Subregion: " +
                            $"{country.Subregion}{Environment.NewLine}Population: {country.Population}" +
                            $"{Environment.NewLine}Gini coefficient: {country.Gini}{Environment.NewLine}" +
                            $"Flag: {country.Flag}";
                    }
                }
                else
                {
                    MessageBox.Show("Selecione uma opção","Informação",MessageBoxButton.OK,MessageBoxImage.Information);
                    return;
                }
            }
        }
    }
}
