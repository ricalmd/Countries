using System.Windows;

namespace Biblioteca.Services
{
    /// <summary>
    /// A method for creating error messages.
    /// </summary>
    public class Dialog
    {
        public void ShowMessage(string message, string header)
        {
            MessageBox.Show(message, header);
        }
    }
}