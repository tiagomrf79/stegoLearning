﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using stegoLearning.WinUI.comum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

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
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".bmp");
            fileOpenPicker.FileTypeFilter.Add(".ico");

            WinRT.Interop.InitializeWithWindow.Initialize(fileOpenPicker, App.appWindowHandle);

            StorageFile storageFile = await fileOpenPicker.PickSingleFileAsync();

            //só se escolher ficheiro é que cria uma nova imagem
            WriteableBitmap writeableBitmap = null;
            if (storageFile != null)
            {
                try
                {
                    writeableBitmap = await ImagemIO.ConverterFicheiroEmBitmap(storageFile);
                }
                catch (Exception exception)
                {
                    //informar que não possível abrir imagem
                    return;
                }
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
                throw new ArgumentException("É necessário esteganografar uma imagem antes de poder guardá-la.");
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

            StorageFile storageFile = await fileSavePicker.PickSaveFileAsync();

            //só se não cancelar é que cria um novo ficheiro
            if (storageFile != null)
            {
                try
                {
                    await ImagemIO.GravarBitmapEmFicheiro((WriteableBitmap)imgStego.Source, storageFile, formatoImagemId);
                    //informar que gravação foi bem sucedida
                }
                catch (Exception exception)
                {
                    //informar que gravação falhou
                    return;
                }
            }
        }

        private void btnSteg_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtMensagem.Text))
            {
                throw new ArgumentException("Não indicou a mensagem.");
            }
            else if (imgOriginal.Source == null)
            {
                throw new ArgumentException("Não seleccionou uma imagem.");
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
                catch (Exception)
                {
                    //informar que houve um problema a encriptar a mensagem com a password
                    return;
                }
            }

            WriteableBitmap writableBitmap = (WriteableBitmap)imgOriginal.Source;
            try
            {
                imgStego.Source = Esteganografia.EsteganografarImagem(writableBitmap, bytesMensagem, numeroBits);
                btnGuardar.IsEnabled = true;
            }
            catch (Exception)
            {
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
