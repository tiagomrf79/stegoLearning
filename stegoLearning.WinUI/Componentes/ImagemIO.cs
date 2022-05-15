using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace stegoLearning.WinUI.Componentes;

internal class ImagemIO
{
    /// <summary>
    /// Transforma um ficheiro local num bitmap que seja compatível com a UI do WinUI
    /// </summary>
    /// <param name="storageFile"></param>
    /// <returns></returns>
    public static async Task<WriteableBitmap> ConverterFicheiroEmBitmap(StorageFile storageFile)
    {
        WriteableBitmap writeableBitmap = null;

        using (IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.Read))
        {
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

            //manter tamanho da imagem
            writeableBitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

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
            using (Stream stream = writeableBitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
            }
        }

        return writeableBitmap;
    }

    /// <summary>
    /// Grava imagem localmente
    /// </summary>
    /// <param name="writeableBitmap"></param>
    /// <param name="storageFile"></param>
    /// <param name="encoderId"></param>
    /// <returns></returns>
    public static async Task GravarBitmapEmFicheiro(WriteableBitmap writeableBitmap, StorageFile storageFile, Guid encoderId)
    {
        //criar novo softwareBitmap com base no writableBitmap existente
        SoftwareBitmap softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
            writeableBitmap.PixelBuffer,
            BitmapPixelFormat.Bgra8,
            writeableBitmap.PixelWidth,
            writeableBitmap.PixelHeight,
            BitmapAlphaMode.Straight);


        //se for bitmap por defeito não guarda o canal alpha (assume que é opaco = 255)
        //EnableV5Header32bppBGRA força guardar o canal alpha
        var bitmapProperties = new BitmapPropertySet();
        if (encoderId == BitmapEncoder.BmpEncoderId)
        {
            var bitmapTypedValue = new BitmapTypedValue(true, Windows.Foundation.PropertyType.Boolean);
            bitmapProperties.Add("EnableV5Header32bppBGRA", bitmapTypedValue);
        }

        //gravar softwareBitmap no ficheiro escolhido
        using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
        {
            //converter imagem no formato escolhido
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, stream, bitmapProperties);

            encoder.SetSoftwareBitmap(softwareBitmap);
            await encoder.FlushAsync();
        }
    }

}
