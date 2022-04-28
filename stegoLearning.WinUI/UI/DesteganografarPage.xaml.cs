using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using stegoLearning.WinUI.Componentes;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace stegoLearning.WinUI.UI
{
    public sealed partial class DesteganografarPage : Page
    {
        public DesteganografarPage()
        {
            this.InitializeComponent();
        }

        private async void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".bmp");

            WinRT.Interop.InitializeWithWindow.Initialize(fileOpenPicker, App.appWindowHandle);

            StorageFile storageFile = null;
            WriteableBitmap writeableBitmap = null;

            try
            {
                storageFile = await fileOpenPicker.PickSingleFileAsync();
                if (storageFile != null)
                {
                    writeableBitmap = await ImagemIO.ConverterFicheiroEmBitmap(storageFile);
                }
            }
            catch (FileNotFoundException) //não encontrou o ficheiro
            {
                txtErros.Text = "Não foi possível encontrar o ficheiro seleccionado.";
                return;
            }
            catch (UnauthorizedAccessException) //não tem permissões para abrir o ficheiro
            {
                txtErros.Text = "Não tem permissões para abrir o ficheiro seleccionado. Contacte o seu administrador.";
                return;
            }
            catch (EndOfStreamException) //algum problema com a stream de leitura de ficheiro ou de criação da imagem
            {
                txtErros.Text = "Não foi possível abrir o ficheiro. Certifique-se que seleccionou um ficheiro válido.";
                return;
            }
            catch (COMException) //não reconheceu o ficheiro como imagem (code 0x88982F50)
            {
                txtErros.Text = "Não foi possível abrir a imagem. Certifique-se que seleccionou uma imagem válida.";
                return;
            }

            //só se a conversão correr bem é que atualiza a imagem na UI
            if (writeableBitmap != null)
            {
                imgStego.Source = writeableBitmap;
                AjustarTamanhoImagem();
                btnUnsteg.IsEnabled = true;
            }
        }

        private void btnUnsteg_Click(object sender, RoutedEventArgs e)
        {
            if (imgStego.Source == null)
            {
                txtErros.Text = "Por favor escolha uma imagem para ser desteganografada.";
                return;
            }

            byte[] dados;
            ///////////////////////////////////////////////////////////////////////////////////////
            try
            {
                dados = Esteganografia.DesteganografarImagem((WriteableBitmap)imgStego.Source);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                txtErros.Text = exception.Message;
                txtMensagem.Text = "";
                return;
            }

            //só utilizar desencriptação se colocar texto na palavra-passe
            string password = txtPassword.Text;
            if (password.Length > 0)
            {
                ///////////////////////////////////////////////////////////////////////////////////////
                try
                {
                    dados = CifraSimetrica.DesencriptarDados(dados, password);
                }
                catch (Exception exception)
                {
                    //informar que a desencriptação da mensagem falhou
                    ErrosLog.EscreverErroEmLog(exception);
                    return;
                }
            }

            if (dados != null)
            {
                txtMensagem.Text = Encoding.UTF8.GetString(dados);
            }
        }

        public void AjustarTamanhoImagem(double novaLargura = 0)
        {
            //se não receber parâmetro da novaLargura, obter largura atual
            if (novaLargura == 0)
            {
                novaLargura = ((ScrollViewer)((Frame)this.Parent).Parent).ActualWidth;
            }

            //a imagem deve ter, pelo menos, a mesma largura dos botões da outra coluna
            double minLargura = btnAbrir.ActualWidth + 20;

            //a imagem deve ter, no máximo, cerca de metade do espaço do formulário
            double maxLargura = novaLargura * 0.45;

            double largura = Math.Max(minLargura, maxLargura);
            imgStego.Width = largura;
        }
    }
}
