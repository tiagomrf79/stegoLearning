using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace stegoLearning.WinUI.Componentes;

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
    /// Altera um bloco de bytes de uma determinada imagem
    /// </summary>
    /// <param name="imagem"></param>
    /// <param name="dadosImagem"></param>
    /// <param name="posicao"></param>
    public static void AlterarBytesEmImagem(WriteableBitmap imagem, byte[] dadosImagem, int posicao)
    {
        using (Stream stream = imagem.PixelBuffer.AsStream())
        {
            stream.Seek(posicao, SeekOrigin.Begin);
            stream.WriteAsync(dadosImagem, 0, dadosImagem.Length);
        }
    }

    /// <summary>
    /// Cria uma cópia de uma imagem de origem
    /// </summary>
    /// <param name="origem"></param>
    /// <returns></returns>
    public static WriteableBitmap DuplicarImagem(WriteableBitmap origem)
    {
        WriteableBitmap destino = new WriteableBitmap(origem.PixelWidth, origem.PixelHeight);

        using (Stream streamDestino = destino.PixelBuffer.AsStream())
        {
            using (Stream streamOrigem = origem.PixelBuffer.AsStream())
            {
                streamOrigem.CopyToAsync(streamDestino);
            }
        }

        return destino;
    }
}
