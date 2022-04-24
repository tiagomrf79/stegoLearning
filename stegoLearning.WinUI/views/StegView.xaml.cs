using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
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

namespace stegoLearning.WinUI.views
{
    public sealed partial class StegView : Page
    {
        public StegView()
        {
            this.InitializeComponent();
        }

        private async void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            imgOriginal.Source = await AbrirConverterImagem();
            imgStego.Source = null;
            AjustarTamanhoImagem();
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {

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

            var window = (Application.Current as App)?.m_window as MenuView;
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(fileSavePicker, hwnd);

            StorageFile outputFile = await fileSavePicker.PickSaveFileAsync();

            if (outputFile != null)
            {
                WriteableBitmap writeableBitmap = (WriteableBitmap)imgStego.Source;

                //criar novo softwareBitmap com base no writableBitmap existente
                SoftwareBitmap softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
                    writeableBitmap.PixelBuffer,
                    BitmapPixelFormat.Bgra8,
                    writeableBitmap.PixelWidth,
                    writeableBitmap.PixelHeight);

                //gravar softwareBitmap no ficheiro escolhido
                using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    //converter imagem no formato escolhido
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(formatoImagemId, stream);

                    encoder.SetSoftwareBitmap(softwareBitmap);
                    await encoder.FlushAsync();
                }
            }
        }

        private async Task<WriteableBitmap> AbrirConverterImagem()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".ico");

            var window = (Application.Current as App)?.m_window as MenuView;
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile inputFile = await picker.PickSingleFileAsync();

            WriteableBitmap writableBitmap = null;
            if (inputFile != null)
            {
                using (IRandomAccessStream fileStream = await inputFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

                    //manter tamanho da imagem
                    writableBitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

                    //converter a imagem em formato suportado pelo controlo da UI
                    BitmapTransform transform = new BitmapTransform();
                    PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,    // WriteableBitmap uses BGRA format
                        BitmapAlphaMode.Straight,
                        transform,
                        ExifOrientationMode.IgnoreExifOrientation, // This sample ignores Exif orientation
                        ColorManagementMode.DoNotColorManage);

                    //obter dados da imagem e gravá-los no writableBitmap
                    byte[] sourcePixels = pixelData.DetachPixelData();
                    using (Stream stream = writableBitmap.PixelBuffer.AsStream())
                    {
                        await stream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    }
                }
            }
            return writableBitmap;
        }

        private void btnSteg_Click(object sender, RoutedEventArgs e)
        {
            string mensagem = txtMensagem.Text;
            byte[] bytesMensagem = Encoding.UTF8.GetBytes(mensagem);

            ComboBoxItem typeItem = (ComboBoxItem)cbBitsPorComponente.SelectedItem;
            short numeroBits;
            if (!short.TryParse(typeItem.Content.ToString(), out numeroBits))
            {
                numeroBits = 1;
            }

            string password = txtPassword.Text;
            if (password.Length > 0)
            {
                bytesMensagem = CifraSimetrica.EncriptarMensagem(mensagem, password);
            }

            WriteableBitmap writableBitmap = (WriteableBitmap)imgOriginal.Source;
            imgStego.Source = Esteganografia.EsteganografarImagem(writableBitmap, bytesMensagem, numeroBits);
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
