using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using stegoLearning.WinUI.Componentes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace stegoLearning.WinUI.UI
{
    public sealed partial class EsteganografarPage : Page
    {
        public EsteganografarPage()
        {
            this.InitializeComponent();
        }

        private async void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            txtErros.Text = "";

            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".bmp");
            fileOpenPicker.FileTypeFilter.Add(".ico");

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
            catch (COMException) //não reconheceu o ficheiro como imagem (code 0x88982F50)
            {
                txtErros.Text = "Não foi possível abrir a imagem. Certifique-se que seleccionou uma imagem válida.";
                return;
            }
            catch (IOException) //não tem acesso ou não encontrou o ficheiro, etc
            {
                txtErros.Text = "Não foi possível abrir o ficheiro seleccionado.";
                return;
            }
            catch (Exception ex)
            {
                txtErros.Text = "Ocorreu um erro e a operação terminou inesperadamente. Tente novamente e reinicie a aplicação caso o erro persista.";
                ErrosLog.EscreverErroEmLog(ex);
                return;
            }

            //só se a conversão correr bem é que atualiza a UI
            if (writeableBitmap != null)
            {
                imgOriginal.Source = writeableBitmap;
                imgStego.Source = null;
                AjustarTamanhoImagem();
                btnGuardar.IsEnabled = false;
                btnSteg.IsEnabled = true;
            }
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            txtErros.Text = "";

            if (imgStego.Source == null)
            {
                txtErros.Text = "Não foi encontrada uma imagem esteganografada para guardar. " +
                    "Clique em esteganografar para gerar essa imagem a partir da imagem da esquerda.";
                return;
            }


            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            Guid formatoImagemId;
            if (rbPng.IsChecked == true)
            {
                formatoImagemId = BitmapEncoder.PngEncoderId;
                fileSavePicker.FileTypeChoices.Add("PNG files", new List<string>() { ".png" });
            }
            else
            {
                formatoImagemId = BitmapEncoder.BmpEncoderId;
                fileSavePicker.FileTypeChoices.Add("Bitmap files", new List<string>() { ".bmp" });
            }
            fileSavePicker.SuggestedFileName = "stego";

            WinRT.Interop.InitializeWithWindow.Initialize(fileSavePicker, App.appWindowHandle);

            try
            {
                StorageFile storageFile = await fileSavePicker.PickSaveFileAsync();
                if (storageFile != null)
                {
                    await ImagemIO.GravarBitmapEmFicheiro((WriteableBitmap)imgStego.Source, storageFile, formatoImagemId);
                    txtErros.Text = "Imagem guardada com sucesso!";
                }
            }
            catch (IOException) //não tem acesso ou não encontrou o ficheiro, etc
            {
                txtErros.Text = "Não foi possível abrir o ficheiro seleccionado.";
                return;
            }
            catch (Exception ex)
            {
                txtErros.Text = "Ocorreu um erro e a operação terminou inesperadamente. Tente novamente e reinicie a aplicação caso o erro persista.";
                ErrosLog.EscreverErroEmLog(ex);
                return;
            }
        }

        private void btnSteg_Click(object sender, RoutedEventArgs e)
        {
            txtErros.Text = "";

            if (string.IsNullOrEmpty(txtMensagem.Text))
            {
                txtErros.Text = "Por favor preencha o campo com a mensagem que pretende esteganografar.";
                return;
            }
            else if (imgOriginal.Source == null)
            {
                txtErros.Text = "Por favor escolha uma imagem para ser esteganografada.";
                return;
            }

            //transformar mensagem digitada em bytes
            string mensagem = txtMensagem.Text;
            byte[] bytesMensagem = Encoding.UTF8.GetBytes(mensagem);

            //obter n.º de bits por componente
            ComboBoxItem typeItem = (ComboBoxItem)cbBitsPorComponente.SelectedItem;
            short numeroBits;
            if (!short.TryParse(typeItem.Content.ToString(), out numeroBits))
            {
                numeroBits = 1;
            }

            //só utilizar encriptação se colocar texto na palavra-passe
            string password = txtPassword.Text;
            if (password.Length > 0)
            {
                try
                {
                    bytesMensagem = CifraSimetrica.EncriptarMensagem(mensagem, password);
                }
                catch (Exception ex)
                {
                    txtErros.Text = "Ocorreu um erro e a operação terminou inesperadamente. Tente novamente e reinicie a aplicação caso o erro persista.";
                    ErrosLog.EscreverErroEmLog(ex);
                    return;
                }
            }

            WriteableBitmap writeableBitmap = (WriteableBitmap)imgOriginal.Source;

            //validar tamanho da mensagem e da imagem
            int numPixeisMensagem = Esteganografia.CalcularPixeisUtilizados(bytesMensagem.Length * 8, numeroBits);
            int numPixeisAux = (sizeof(int) + sizeof(short)) * 8;
            int numPixeisImagem = writeableBitmap.PixelWidth * writeableBitmap.PixelHeight;
            if (numPixeisMensagem + numPixeisAux > numPixeisImagem)
            {
                txtErros.Text = $"A resolução da imagem ({numPixeisImagem} pixeís) é insuficiente para a mensagem escolhida ({numPixeisMensagem + numPixeisAux} pixeís). Escolha uma imagem com maior resolução ou uma mensagem mais curta.";
                return;
            }

            try
            {
                imgStego.Source = Esteganografia.EsteganografarImagem(writeableBitmap, bytesMensagem, numeroBits);
            }
            catch (Exception ex)
            {
                txtErros.Text = "Ocorreu um erro e a operação terminou inesperadamente. Tente novamente e reinicie a aplicação caso o erro persista.";
                ErrosLog.EscreverErroEmLog(ex);
                return;
            }

            btnGuardar.IsEnabled = true;
        }

        /// <summary>
        /// Ajusta o tamanho do controlo da imagem consoante o tamanho da janela da aplicação
        /// </summary>
        /// <param name="novaLargura"></param>
        public void AjustarTamanhoImagem(double novaLargura = 0)
        {
            //se não receber parâmetro da novaLargura, obter largura atual
            if (novaLargura == 0)
            {
                novaLargura = ((ScrollViewer)((Frame)this.Parent).Parent).ActualWidth;
            }

            //a imagem deve ter, pelo menos, a mesma largura dos botões ficam abaixo da imagem stego
            double minLargura = rbBmp.ActualWidth + rbPng.ActualWidth + btnGuardar.ActualWidth + 20;

            //a imagem deve ter, no máximo, cerca de metade do espaço do formulário
            double maxLargura = novaLargura * 0.40;

            double largura = Math.Max(minLargura, maxLargura);
            imgOriginal.Width = largura;
            imgStego.Width = largura;
        }
    }
}
