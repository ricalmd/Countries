using System.Windows;

namespace Biblioteca.Services
{
    public class Dialog
    {
        public void ShowMessage(string message, string header)
        {
            MessageBox.Show(message, header);
        }
    }
}