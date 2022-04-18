using Microsoft.UI.Xaml;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace stegoLearning.WinUI
{
    public sealed partial class ImageWindow : Window
    {
        public ImageWindow()
        {
            this.InitializeComponent();
        }

        private void btnSteg_Click(object sender, RoutedEventArgs e)
        {
            string mensagem = txtMensagemEmissor.Text;
            byte[] bytesMensagem = Encoding.UTF8.GetBytes(mensagem);

            short numeroBits;
            if (!short.TryParse(txtBitsEmissor.Text, out numeroBits))
            {
                numeroBits = 1;
            }

            string password = txtPasswordEmissor.Text;
            if (password.Length > 0)
            {
                bytesMensagem = CifraSimetrica.EncriptarMensagem(mensagem, password);
            }

            StegFromWritableBitmap(bytesMensagem, numeroBits);
        }

        private void StegFromWritableBitmap(byte[] bytesMensagem, short numeroBits)
        {
            WriteableBitmap writableBitmap = (WriteableBitmap)imgOriginal.Source;
            imgStego.Source = Esteganografia.EsteganografarImagem(writableBitmap, bytesMensagem, numeroBits);
        }

        private async void btnOpenImage_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".ico");

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile inputFile = await picker.PickSingleFileAsync();
            imgOriginal.Source = await OpenWritableBitmap(inputFile);
        }

        private async Task<WriteableBitmap> OpenWritableBitmap(StorageFile inputFile)
        {
            if (inputFile != null)
            {
                WriteableBitmap writableBitmap;
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
                return writableBitmap;
            }
            return null;
        }

        private async void btnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            string formatoImagem = "bmp";
            Guid formatoImagemId;

            if (formatoImagem == "png")
            {
                formatoImagemId = BitmapEncoder.PngEncoderId;
            }
            else
            {
                formatoImagemId = BitmapEncoder.BmpEncoderId;
            }

            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            if (formatoImagem == "png")
            {
                fileSavePicker.FileTypeChoices.Add("PNG files", new List<string>() { ".png" });
            }
            else
            {
                fileSavePicker.FileTypeChoices.Add("Bitmap files", new List<string>() { ".bmp" });
            }
            fileSavePicker.SuggestedFileName = "stego";

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
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

        private async void btnOpenStego_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile inputFile = await picker.PickSingleFileAsync();
            imgFinal.Source = await OpenWritableBitmap(inputFile);
        }

        private void btnUnsteg_Click(object sender, RoutedEventArgs e)
        {
            byte[] dados = UnstegFromWritableBitmap();

            string password = txtPasswordRecetor.Text;
            if (password.Length > 0)
            {
                dados = CifraSimetrica.DesencriptarDados(dados, password);
            }

            txtMensagemRecetor.Text = Encoding.UTF8.GetString(dados);
        }

        private byte[] UnstegFromWritableBitmap()
        {
            WriteableBitmap writableBitmap = (WriteableBitmap)imgFinal.Source;
            return Esteganografia.DesteganografarImagem(writableBitmap);
        }
    }
}
