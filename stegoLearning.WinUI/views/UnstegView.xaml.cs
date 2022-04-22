using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
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

namespace stegoLearning.WinUI.views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UnstegView : Page
    {
        public UnstegView()
        {
            this.InitializeComponent();
        }

        private async void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            imgStego.Source = await AbrirConverterImagem();
            AjustarTamanhoImagem();
        }

        private void btnUnsteg_Click(object sender, RoutedEventArgs e)
        {
            byte[] dados = Esteganografia.DesteganografarImagem((WriteableBitmap)imgStego.Source);

            string password = txtPassword.Text;
            if (password.Length > 0)
            {
                dados = CifraSimetrica.DesencriptarDados(dados, password);
            }

            txtMensagem.Text = Encoding.UTF8.GetString(dados);
        }

        private async Task<WriteableBitmap> AbrirConverterImagem()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            var window = (Application.Current as App)?.m_window as teste;
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
        private void AjustarTamanhoImagem()
        {
            double fator = 0.40; //40% para cada imagem e o restante para o botão esteganografar, margens, etc
            imgStego.Width = ((ScrollViewer)((Frame)this.Parent).Parent).ActualWidth * fator;
        }

        private void btnAjustar_Click(object sender, RoutedEventArgs e)
        {
            AjustarTamanhoImagem();
        }
    }
}
