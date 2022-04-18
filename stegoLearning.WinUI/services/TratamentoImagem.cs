using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace stegoLearning.WinUI
{
    public static class TratamentoImagem
    {
        /// <summary>
        /// Retorna os bytes de uma imagem.
        /// </summary>
        /// <param name="writeableBitmap"></param>
        /// <param name="primeiroPixel"></param>
        /// <param name="numPixeis"></param>
        /// <returns></returns>
        public static byte[] ConverterImagemEmBytes(WriteableBitmap writeableBitmap, uint primeiroPixel, int numPixeis)
        {
            //cada pixel tem 4 bytes, um por componente
            byte[] bytesImagem = writeableBitmap.PixelBuffer.ToArray(primeiroPixel * 4, numPixeis * 4);
            return bytesImagem;
        }

        /// <summary>
        /// Retorna os pixeis de uma imagem a partir dos seus bytes.
        /// </summary>
        /// <param name="bytesImagem"></param>
        /// <returns></returns>
        public static byte[][] ConverterBytesEmPixeis(byte[] bytesImagem)
        {
            //cada pixel tem 4 bytes, um por componente
            int numPixeis = bytesImagem.Length / 4;

            //o array externo tem cada pixel, o array interno tem cada componente desse pixel
            byte[][] pixeis = new byte[numPixeis][];

            //transformar bytes da imagem em pixeis
            for (int i = 0; i < bytesImagem.Length; i += 4)
            {
                //tenho de guardar o alpha porque no processo inverso (criar imagem a partir de pixels) preciso dele
                byte[] aux = new byte[4];
                for (int j = 0; j < 4; j++)
                {
                    aux[j] = bytesImagem[i + j];
                }
                pixeis[i / 4] = aux;
            }
            return pixeis;
        }

        /// <summary>
        /// Retorna os bytes de uma imagem através dos seus pixeis.
        /// </summary>
        /// <param name="pixeis"></param>
        /// <returns></returns>
        public static byte[] ConverterPixeisEmBytes(byte[][] pixeis)
        {
            //cada pixel tem 4 bytes, um por componente
            byte[] bytesImagem = new byte[pixeis.Length * 4];

            //transformar o array de pixeis em array com bytes da imagem
            for (int i = 0; i < pixeis.Length; i++)
            {
                for (int j = 0; j < pixeis[i].Length; j++)
                {
                    bytesImagem[i * 4 + j] = pixeis[i][j];
                }
            }

            return bytesImagem;
        }

        /// <summary>
        /// Retorna uma imagem a partir dos seus bytes
        /// </summary>
        /// <param name="dadosImagem"></param>
        /// <param name="largura"></param>
        /// <param name="altura"></param>
        /// <returns></returns>
        public static WriteableBitmap ConverterBytesEmImagem(byte[] dadosImagem, int largura, int altura)
        {
            //criar nova imagem
            WriteableBitmap writeableBitmap = new WriteableBitmap(largura, altura);

            //usar os bytes da imagem (dadosImagem) para desenhar a nova imagem
            using (Stream stream = writeableBitmap.PixelBuffer.AsStream())
            {
                stream.Write(dadosImagem, 0, dadosImagem.Length);
            }

            return writeableBitmap;
        }
    }
}
