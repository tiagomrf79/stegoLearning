using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace stegoLearning.WinUI
{
    public static class Esteganografia
    {
        //constantes que determinam o n.º de bytes ocupados na imagem com o "numeroBits" e "tamanhoMensagem", respectivamente
        private const int BytesShort = 2;   //2 bytes ou 16 bits
        private const int BytesInt = 4;     //4 bytes ou 32 bits

        /// <summary>
        /// Cria uma imagem com conteúdo esteganografado
        /// </summary>
        /// <param name="imagemOriginal"></param>
        /// <param name="dadosMensagem"></param>
        /// <param name="bitsPorComponente"></param>
        /// <returns></returns>
        public static WriteableBitmap EsteganografarImagem(WriteableBitmap imagemOriginal, byte[] dadosMensagem, short bitsPorComponente)
        {

            int tamanhoMensagem = dadosMensagem.Length;

            int largura = imagemOriginal.PixelWidth;
            int altura = imagemOriginal.PixelHeight;
            int numPixeisTotal = altura * largura;

            #region MENSAGEM

            //converter bloco da mensagem (encriptada ou não) em sequência binária
            BitArray bitsMensagem = SequenciaBinaria.BytesParaSequenciaBinaria(dadosMensagem);

            //obter pixéis da imagem que serão alterados com essa sequência binária
            int numPixeisMensagem = CalcularPixeisUtilizados(bitsMensagem.Length, bitsPorComponente);
            byte[] bytesImagemMensagem = TratamentoImagem.ConverterImagemEmBytes(imagemOriginal, 0, numPixeisMensagem);
            byte[][] pixeisMensagem = TratamentoImagem.ConverterBytesEmPixeis(bytesImagemMensagem);

            //alterar esses pixeis com a sequência binária respetiva
            pixeisMensagem = AlterarBitsComponentes(pixeisMensagem, bitsMensagem, bitsPorComponente);

            //obter bytes com as alterações feitas
            bytesImagemMensagem = TratamentoImagem.ConverterPixeisEmBytes(pixeisMensagem);

            #endregion

            #region BLOCO FINAL

            //construir um bloco com os bits alterados e o tamanho da mensagem, que irá ocupar 48 bits
            byte[] dadosFim = ComporBlocoFinal(bitsPorComponente, tamanhoMensagem);

            //converter esse bloco em sequência binária
            BitArray bitsFim = SequenciaBinaria.BytesParaSequenciaBinaria(dadosFim);

            //obter pixéis da imagem que serão alterados com essa sequência binária
            int numPixeisFim = CalcularPixeisUtilizados(bitsFim.Length, 1);
            byte[] bytesImagemFim = TratamentoImagem.ConverterImagemEmBytes(imagemOriginal, (uint)(numPixeisTotal - numPixeisFim), numPixeisFim);
            byte[][] pixeisFim = TratamentoImagem.ConverterBytesEmPixeis(bytesImagemFim);

            //alterar esses pixeis com a sequência binária respetiva
            pixeisFim = AlterarBitsComponentes(pixeisFim, bitsFim, 1);

            //obter bytes com as alterações feitas
            bytesImagemFim = TratamentoImagem.ConverterPixeisEmBytes(pixeisFim);

            #endregion

            #region CRIAR IMAGEM

            //criar uma cópia da imagem original, onde insere a mensagem e bloco final
            int numBytesTotal = numPixeisTotal * 4;
            int numBytesFim = bytesImagemFim.Length;
            WriteableBitmap imagemAlterada = TratamentoImagem.DuplicarImagem(imagemOriginal);
            TratamentoImagem.AlterarBytesEmImagem(imagemAlterada, bytesImagemMensagem, 0);
            TratamentoImagem.AlterarBytesEmImagem(imagemAlterada, bytesImagemFim, numBytesTotal - numBytesFim);

            #endregion

            return imagemAlterada;
        }

        /// <summary>
        /// Obtém conteúdo esteganografado de uma imagem.
        /// </summary>
        /// <param name="imagemStego"></param>
        /// <returns></returns>
        public static byte[] DesteganografarImagem(WriteableBitmap imagemStego)
        {
            int largura = imagemStego.PixelWidth;
            int altura = imagemStego.PixelHeight;
            int numPixeisTotal = altura * largura;

            #region BLOCO FINAL

            int numBytesFim = BytesShort + BytesInt;
            int numBitsFim = numBytesFim * 8;

            //obter os pixéis da imagem que necessitam de ser lidos para obter o n.º de bits por componente e o tamanho da mensagem
            int numPixeisFim = CalcularPixeisUtilizados(numBitsFim, 1);
            byte[] bytesImagemFim = TratamentoImagem.ConverterImagemEmBytes(imagemStego, (uint)(numPixeisTotal - numPixeisFim), numPixeisFim);
            byte[][] pixeisFim = TratamentoImagem.ConverterBytesEmPixeis(bytesImagemFim);

            //ler a sequência binária escrita nesses pixeis
            BitArray bitsFim = LerBitsComponentes(pixeisFim, 1, numBitsFim);

            //converter a sequência binária nas variáveis que necessitamos para processar pixeis com a mensagem
            byte[] dadosFim = SequenciaBinaria.SequenciaBinariaParaBytes(bitsFim, numBytesFim);
            short bitsAlteradosPorComponente;
            int numBytesMensagem;
            (bitsAlteradosPorComponente, numBytesMensagem) = DecomporBlocoFinal(dadosFim);

            #endregion

            #region MENSAGEM

            int numBitsMensagem = numBytesMensagem * 8;

            //obter os pixéis da imagem que necessitam de ser lidos para obter a mensagem
            int numPixeisMensagem = CalcularPixeisUtilizados(numBitsMensagem, bitsAlteradosPorComponente);
            byte[] bytesImagemMensagem = TratamentoImagem.ConverterImagemEmBytes(imagemStego, 0, numPixeisMensagem);
            byte[][] pixeisMensagem = TratamentoImagem.ConverterBytesEmPixeis(bytesImagemMensagem);

            //ler a sequência binária escrita nesses pixeis
            BitArray bitsMensagem = LerBitsComponentes(pixeisMensagem, bitsAlteradosPorComponente, numBitsMensagem);

            //converter a sequência binária em bytes
            byte[] dadosMensagem = SequenciaBinaria.SequenciaBinariaParaBytes(bitsMensagem, numBytesMensagem);

            #endregion

            return dadosMensagem;
        }

        /// <summary>
        /// Altera pixeis com bits de uma sequência binária
        /// </summary>
        /// <param name="pixeis"></param>
        /// <param name="bits"></param>
        /// <param name="bitsPorComponente"></param>
        /// <returns></returns>
        private static byte[][] AlterarBitsComponentes(byte[][] pixeis, BitArray bits, short bitsPorComponente)
        {
            int totalBits = bits.Length;
            int numPixeis = pixeis.Length;
            
            int bitsEscritos = 0;
            for (int i = 0; i < numPixeis; i++)
            {
                for (int j = 0; j < 3 && bitsEscritos < totalBits; j++) //ignorar componente alpha
                {
                    //converter componente atual (byte) numa sequencia de bits
                    BitArray bitsComponente = SequenciaBinaria.BytesParaSequenciaBinaria(pixeis[i][j]);

                    for (int k = 0; k < bitsPorComponente && bitsEscritos < totalBits; k++) //k indica a posição do bit a alterar (começa no menos significativo)
                    {
                        //obter valor que queremos escrever
                        bool valorNovo = bits.Get(bitsEscritos);

                        //gravar esse valor na sequencia binária com o componente atual
                        bitsComponente.Set(k, valorNovo);
                        bitsEscritos++;
                    }

                    //gravar alterações no componente atual
                    pixeis[i][j] = SequenciaBinaria.SequenciaBinariaParaBytes(bitsComponente);
                }
            }
            return pixeis;
        }

        /// <summary>
        /// Ler bits escritos num conjunto de pixeis
        /// </summary>
        /// <param name="pixeis"></param>
        /// <param name="bitsPorComponente"></param>
        /// <param name="totalBits"></param>
        /// <returns></returns>
        public static BitArray LerBitsComponentes(byte[][] pixeis, short bitsPorComponente, int totalBits)
        {
            //criar sequência binária para guardar os bits extraídos da imagem
            BitArray bitArray = new BitArray(totalBits);

            int numPixeis = pixeis.Length;
            int bitsLidos = 0;
            for (int i = 0; i < numPixeis; i++)
            {
                for (int j = 0; j < 3 && bitsLidos < totalBits; j++) //ignorar componente alpha
                {
                    //converter componente atual (byte) numa sequencia de bits
                    BitArray bitsComponente = SequenciaBinaria.BytesParaSequenciaBinaria(pixeis[i][j]);

                    for (int k = 0; k < bitsPorComponente && bitsLidos < totalBits; k++) //k indica a posição do bit a ler (começa no menos significativo)
                    {
                        //obter valor do bit no componente
                        bool valorBit = bitsComponente.Get(k);

                        //gravar esse valor na sequencia binária com os bits extraidos
                        bitArray.Set(bitsLidos, valorBit);
                        bitsLidos++;
                    }
                }
            }

            return bitArray;
        }

        /// <summary>
        /// Calcula quantos pixeis são utilizados para guardar um determinado n.º de bits.
        /// </summary>
        /// <param name="totalBits"></param>
        /// <param name="bitsPorComponente"></param>
        /// <returns></returns>
        private static int CalcularPixeisUtilizados(int totalBits, int bitsPorComponente)
        {
            double numComponentesAlterados = (double)totalBits / bitsPorComponente;
            int numPixeisAlterados = (int)Math.Ceiling(numComponentesAlterados / 3); //pixéis alterados parcialmente também contam
            return numPixeisAlterados;
        }


        /// <summary>
        /// Cria um bloco de bytes com o n.º de bits alterados por componente e o tamanho da mensagem.
        /// </summary>
        /// <param name="bitsAlterados"></param>
        /// <param name="tamanhoMensagem"></param>
        /// <returns></returns>
        private static byte[] ComporBlocoFinal(short bitsAlterados, int tamanhoMensagem)
        {
            //transformar valores dos argumentos em array de bytes
            byte[] bytesNumeroBits = BitConverter.GetBytes(bitsAlterados);
            byte[] bytesTamanhoMensagem = BitConverter.GetBytes(tamanhoMensagem);

            //BitConverter vai criar o array conforme a arquitectura do computador onde corre
            //para assegurar que é coerente entre máquinas:
            bytesNumeroBits = bytesNumeroBits.Take(BytesShort).ToArray();
            bytesTamanhoMensagem = bytesTamanhoMensagem.Take(BytesInt).ToArray();
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytesNumeroBits);
                Array.Reverse(bytesTamanhoMensagem);
            }

            //n.º de bits e tamanho vão ficar no mesmo bloco de 48 bits ou 6 bytes (em exatamente 16 pixels)
            byte[] bytesFim = new byte[BytesShort + BytesInt];
            Buffer.BlockCopy(bytesNumeroBits, 0, bytesFim, 0, BytesShort);
            Buffer.BlockCopy(bytesTamanhoMensagem, 0, bytesFim, BytesShort, BytesInt);

            return bytesFim;
        }

        /// <summary>
        /// Obtém o n.º de bits alterados por componente e o tamanho da mensagem a partir de um bloco de bytes
        /// </summary>
        /// <param name="bytesFim"></param>
        /// <returns></returns>
        private static (short, int) DecomporBlocoFinal(byte[] bytesFim)
        {
            //dividir bloco de bytes em dois conforme o espaço ocupado por cada variável
            byte[] bytesNumeroBits = bytesFim.Take(BytesShort).ToArray();
            byte[] bytesTamanhoMensagem = bytesFim.Skip(BytesShort).Take(BytesInt).ToArray();

            //BitConverter vai criar o array conforme a arquitectura do computador onde corre
            //para assegurar que é coerente entre máquinas:
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytesNumeroBits);
                Array.Reverse(bytesTamanhoMensagem);
            }

            //converter bytes em int
            short numeroBits = BitConverter.ToInt16(bytesNumeroBits, 0);
            int tamanhoMensagem = BitConverter.ToInt32(bytesTamanhoMensagem, 0);

            return (numeroBits, tamanhoMensagem);
        }
    }
}
