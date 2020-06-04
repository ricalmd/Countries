using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
            path = "/rest/v2/all?fields=name;alpha2Code;alpha3Code;capital;region;subregion;population;" +
            "demonym;area;gini;nativeName;flag;numericCode;currencies;languages;cioc;translations;topLevelDomain;" +
            "callingCodes;altSpellings;latlng;timezones;borders;regionalBlocs",
            textCurrency, textLanguage, loadMessage = string.Empty;

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
        /// displayed in lblPaises and lblCurrency (text) and imgFlag (flag figures).
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
                        CountLanguages(country.Languages);
                        VerifyEmpty(country);

                        lblPaises.Content = $"Name:{Environment.NewLine}    {country.Name}{Environment.NewLine}" +
                            $"Capital:{Environment.NewLine}    {country.Capital}" +
                            $"{Environment.NewLine}Region:{Environment.NewLine}    {country.Region}" +
                            $"{Environment.NewLine}Subregion:{Environment.NewLine}" +
                            $"    {country.Subregion}{Environment.NewLine}Population:{Environment.NewLine}" +
                            $"    {country.Population}" +
                            $"{Environment.NewLine}Gini coefficient:{Environment.NewLine}    {country.Gini}" +
                            $"{Environment.NewLine}Language(s):{Environment.NewLine}{textLanguage}";
                        lblCurrency.Content = $"Currency:{Environment.NewLine}{textCurrency}";
                        lblArea.Content = $"Area: {country.Area} km2";

                        ShowArea(country.Area);
                        ShowPopulation(country.Population);
                        ConvertSvgToImage(country.Name);
                        lblAvisoImg.Content = string.Empty;
                    }
                }
            }
            catch
            {
                lblAvisoImg.Content = "Bandeira ausente";
                imgFlag.Source = null;
            }
        }

        /// <summary>
        /// The SvgDocument.Open() method opens an SVG document to be converted to an image in PNG format.
        /// Since no image is saved in a folder, the bitmap is saved in memory using the MemoryStream class.
        /// Using the Draw() method, the SVG is passed to the img variable where it is saved as a PNG image.
        /// </summary>
        /// <param name="name"></param>
        private void ConvertSvgToImage(string name)
        {
            var svgDocument = SvgDocument.Open($"FlagImg/{name}.svg");
            
            MemoryStream memory = new MemoryStream();
            BitmapImage bitmapImage = new BitmapImage();
            var img = svgDocument.Draw(400, 230);
            img.Save(memory, ImageFormat.Png);

            memory.Position = 0;
            
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            memory.Close();
            
            imgFlag.Source = bitmapImage;
        }

        /// <summary>
        /// This method performs the counting of elements in a list of currencies. These lists have different
        /// numbers of elements and in order for the display of a list of currencies to be done properly, it is
        /// necessary to know how many elements the list contains. The list and count value are passed to 
        /// DisplayCurrency().
        /// </summary>
        /// <param name="currencies"></param>
        private void CountCurrency(List<Currency> currencies)
        {
            int count = 0;

            foreach (var currency in currencies)
            {
                count++;
            }
            DisplayCurrency(currencies, count);
        }

        /// <summary>
        /// This method performs the counting of elements in a list of languages. These lists have different
        /// numbers of elements and in order for the display of a list of languages to be done properly, it is
        /// necessary to know how many elements the list contains. The list and count value are passed to 
        /// DisplayLanguages().
        /// </summary>
        /// <param name="languages"></param>
        private void CountLanguages(List<Language> languages)
        {
            int count = 0;

            foreach (var language in languages)
            {
                count++;
            }
            DisplayLanguages(languages, count);
        }

        /// <summary>
        /// A list of currencies and the respective element count are passed to this method. An array of strings 
        /// with the size provided by the count variable is used to form the text with the data contained in the 
        /// list. Each element of the array is added to the textCurrency variable.
        /// </summary>
        /// <param name="currencies"></param>
        /// <param name="count"></param>
        private void DisplayCurrency(List<Currency> currencies, int count)
        {
            int num = 0;
            
            string[] text = new string[count];
            textCurrency = string.Empty;
            
            foreach (var currency in currencies)
            {
                VerifyEmptyCurrency(currency);

                text[num] = $"    {currency.Name}, {currency.Symbol}, {currency.Code}{Environment.NewLine}";
                textCurrency += text[num];

                num++;
            }
        }

        /// <summary>
        /// A list of languages and the respective element count are passed to this method. An array of strings 
        /// with the size provided by the count variable is used to form the text with the data contained in the 
        /// list. Each element of the array is added to the textLanguage variable.
        /// </summary>
        /// <param name="languages"></param>
        /// <param name="count"></param>
        private void DisplayLanguages(List<Language> languages, int count)
        {
            int num = 0;

            string[] text = new string[count];
            textLanguage = string.Empty;

            foreach (var language in languages)
            {
                text[num] = $"    {language.Name}, {language.NativeName}{Environment.NewLine}";
                textLanguage += text[num];

                num++;
            }
        }

        /// <summary>
        /// Method that receives the list of countries. The response variable receives the values of the GetCountries() 
        /// method, in the Api class. The Countries list receives the list contained in the Result object through the 
        /// response, being cast to that purpose. If Countries is different from 
        /// null, the process proceeds normally, switching to the LoadFlags() method, in which SVG files are 
        /// loaded into the folder. If the list is not loaded, an error message is displayed.
        /// </summary>
        /// <returns></returns>
        private async Task LoadApiCountries()
        {
            loadMessage = "A carregar dados...";

            var response = await Api.GetCountries(url, path);

            Countries = (List<Country>)response.Result;
        
            if (Countries != null)
            {
                LoadFlags();
            }
            else
            {
                Dialog.ShowMessage(response.Message, "ERRO");
            }

            Data.DeleteData();

            Data.SaveData(Countries);
        }

        /// <summary>
        /// The connection variable receives the Boolean value of the CheckConnection() method, in the Network class.
        /// If this Boolean value is false, it goes to the LoadLocalCountries() method, so that data is collected from 
        /// the database. In lblAviso it is indicated that the Internet connection was not successful. If the connection
        /// is successful, then proceed to the LoadApiCountries() method where the received values come directly from 
        /// the API. The values of the list of Countries are passed to the comboBox cbxListaPaises.
        /// </summary>
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

        /// <summary>
        /// The DownloadFile() method, of the WebClient class, receives the API path for each existing SVG file,
        /// and the path to the folder where those files will be kept in case there is no Internet connection.
        /// </summary>
        private void LoadFlags()
        {
            WebClient client = new WebClient();

            if (!Directory.Exists("FlagImg"))
            {
                Directory.CreateDirectory("FlagImg");
            }

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

        /// <summary>
        /// The list of countries receives the values from the database, using the GetData() method, in the Data class.
        /// </summary>
        private void LoadLocalCountries()
        {
            try
            {
                Countries = Data.GetData();
            }
            catch(Exception e)
            {
                Dialog.ShowMessage(e.Message, "ERRO");
            }
        }

        /// <summary>
        /// The progressBar progresses within a while cycle as long as the Confirm property of the Data class is false
        /// and allows the user to be notified when the data is loaded in the database. 
        /// The Delay() method causes the progressBar progression. The argument in the Delay() method is progressBar's
        /// own advancement. 
        /// </summary>
        private async void ReportProgress()
        {
            while (Data.Confirm == false)
            {
                int progress = (int)pgbProgresso.Value++;
                loadProgram.Content = loadMessage;
                await Task.Delay(progress);

                if(loadMessage == string.Empty && progress == 100)
                {
                    Data.Confirm = true;
                }
            }
            loadProgram.Content = string.Empty;
        }

        /// <summary>
        /// This method receives from the comboBox the value of the Area property, passed to the area variable.
        /// Then, the value of this variable is integrated into a formula that allows obtaining the dimensions of
        /// the circumference to be presented. The result of the formula is stored in the variable dim. Next, the
        /// Ellipse class is instantiated, as well as the SolidColorBrush class. The color variable receives the 
        /// color to be used in the Color property of the SolidColorBrush class. The Fill property, of the Ellipse 
        /// class, receives the value of solidBrush. The Width and Height properties are given the value of the dim 
        /// variable. Finally, a clear is made to delete the previous drawing and the display of the new drawing is made.
        /// </summary>
        /// <param name="area"></param>
        private void ShowArea(double area)
        {
            double dim = Math.Sqrt(area / Math.PI) * 2 / 20;
            
            Ellipse ellipse = new Ellipse();
            SolidColorBrush solidBrush = new SolidColorBrush();
            var color = Color.FromRgb(233, 150, 119);
            solidBrush.Color = color;
            ellipse.Fill = solidBrush;
            ellipse.Width = dim;
            ellipse.Height = dim;
            displayArea.Children.Clear();
            displayArea.Children.Add(ellipse);
        }

        /// <summary>
        /// This method receives from the comboBox the value of the Population property, passed to the population variable.
        /// Then, the value of this variable is divided, that allows obtaining the width of
        /// the bar to be presented. The result of the calculation is stored in the variable dim. Next, the
        /// Rectangle class is instantiated, as well as the SolidColorBrush class. The color variable receives the 
        /// color to be used in the Color property of the SolidColorBrush class. The Fill property, of the Rectangle 
        /// class, receives the value of solidBrush. The Width property are given the value of the dim 
        /// variable. Finally, a clear is made to delete the previous drawing and the display of the new drawing is made.
        /// </summary>
        /// <param name="population"></param>
        private void ShowPopulation(int population)
        {
            double dim = population / 2100000;

            Rectangle rectangle = new Rectangle();
            SolidColorBrush solidBrush = new SolidColorBrush();
            var color = Color.FromRgb(233, 150, 119);
            solidBrush.Color = color;
            rectangle.Fill = solidBrush;
            rectangle.Width = dim;
            rectangle.Height = 20;
            rectangle.RadiusX = 5;
            rectangle.RadiusY = 5;
            displayPopulation.Children.Clear();
            displayPopulation.Children.Add(rectangle);
        }

        /// <summary>
        /// If a property is not assigned a string, then a dash is displayed.
        /// </summary>
        /// <param name="country"></param>
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
        /// <summary>
        /// If a property is not assigned a string, then a dash is displayed.
        /// </summary>
        /// <param name="currency"></param>
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
