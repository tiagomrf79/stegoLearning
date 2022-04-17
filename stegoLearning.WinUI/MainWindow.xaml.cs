using Microsoft.UI.Xaml;
using System;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace stegoLearning.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private byte[] encrypted;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void encrypt_Click(object sender, RoutedEventArgs e)
        {
            string message = sendPlainText.Text;
            string password = senderKey.Text;
            encrypted = CifraSimetrica.EncriptarMensagem(message, password);
            sendCypherText.Text = Convert.ToBase64String(encrypted);
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
        }

        private void decrypt_Click(object sender, RoutedEventArgs e)
        {
            string password = receiverKey.Text;
            var decrypted = CifraSimetrica.DesencriptarDados(encrypted, password);
            var decryptedMessage = Encoding.UTF8.GetString(decrypted);
            receivedPlainText.Text = decryptedMessage;
        }
    }
}
