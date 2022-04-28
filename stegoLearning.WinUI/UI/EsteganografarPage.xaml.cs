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
            catch (DirectoryNotFoundException) //não encontrou a pasta
            {
                txtErros.Text = "Não foi possível encontrar a pasta seleccionada.";
                return;
            }
            catch (UnauthorizedAccessException) //não tem permissões para guardar o ficheiro
            {
                txtErros.Text = "Não tem permissões para guardar o ficheiro na pasta seleccionada. Contacte o seu administrador.";
                return;
            }
            catch (EndOfStreamException) //algum problema com a stream de escrita do ficheiro
            {
                txtErros.Text = "Erro ao guardar o ficheiro.";
                return;
            }
        }

        private void btnSteg_Click(object sender, RoutedEventArgs e)
        {
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
                ///////////////////////////////////////////////////////////////////////////////////////
                try
                {
                    bytesMensagem = CifraSimetrica.EncriptarMensagem(mensagem, password);
                }
                catch (Exception)
                {
                    //informar que houve um problema a encriptar a mensagem com a password
                    return;
                }
            }

            WriteableBitmap writableBitmap = (WriteableBitmap)imgOriginal.Source;
            ///////////////////////////////////////////////////////////////////////////////////////
            try
            {
                imgStego.Source = Esteganografia.EsteganografarImagem(writableBitmap, bytesMensagem, numeroBits);
                btnGuardar.IsEnabled = true;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                txtErros.Text = exception.Message;
                return;
            }
        }

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
