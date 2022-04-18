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
            int largura = imagemOriginal.PixelWidth;
            int altura = imagemOriginal.PixelHeight;
            int tamanhoMensagem = dadosMensagem.Length;

            #region BLOCO INICIAL

            //construir um bloco com os bits alterados e o tamanho da mensagem, que irá ocupar 48 bits
            byte[] dadosInicio = ComporBlocoInicial(bitsPorComponente, tamanhoMensagem);

            //converter esse bloco em sequência binária
            BitArray bitsInicio = SequenciaBinaria.BytesParaSequenciaBinaria(dadosInicio);

            //obter pixéis da imagem que serão alterados com essa sequência binária
            int numPixeisInicio = CalcularPixeisUtilizados(bitsInicio.Length, 1);
            byte[] bytesImagemInicio = TratamentoImagem.ConverterImagemEmBytes(imagemOriginal, 0, numPixeisInicio);
            byte[][] pixeisInicio = TratamentoImagem.ConverterBytesEmPixeis(bytesImagemInicio);

            //alterar esses pixeis com a sequência binária respetiva
            pixeisInicio = AlterarBitsComponentes(pixeisInicio, bitsInicio, 1);

            //obter bytes com as alterações feitas
            bytesImagemInicio = TratamentoImagem.ConverterPixeisEmBytes(pixeisInicio);

            #endregion

            #region MENSAGEM

            //converter bloco da mensagem (encriptada ou não) em sequência binária
            BitArray bitsMensagem = SequenciaBinaria.BytesParaSequenciaBinaria(dadosMensagem);

            //obter pixéis da imagem que serão alterados com essa sequência binária
            int numPixeisMensagem = CalcularPixeisUtilizados(bitsMensagem.Length, bitsPorComponente);
            byte[] bytesImagemMensagem = TratamentoImagem.ConverterImagemEmBytes(imagemOriginal, (uint)numPixeisInicio, numPixeisMensagem);
            byte[][] pixeisMensagem = TratamentoImagem.ConverterBytesEmPixeis(bytesImagemMensagem);

            //alterar esses pixeis com a sequência binária respetiva
            pixeisMensagem = AlterarBitsComponentes(pixeisMensagem, bitsMensagem, bitsPorComponente);

            //obter bytes com as alterações feitas
            bytesImagemMensagem = TratamentoImagem.ConverterPixeisEmBytes(pixeisMensagem);

            #endregion

            #region CRIAR IMAGEM

            //obter bytes não alterados
            int numPixeisTotal = altura * largura;
            int numPixeisRestante = numPixeisTotal - numPixeisInicio - numPixeisMensagem;
            byte[] bytesImagemRestante = TratamentoImagem.ConverterImagemEmBytes(imagemOriginal, (uint)(numPixeisInicio + numPixeisMensagem), numPixeisRestante);

            //juntar tudo no mesmo array (início + mensagem + restante)
            int numBytesInicio = bytesImagemInicio.Length;
            int numBytesMensagem = bytesImagemMensagem.Length;
            int numBytesTotal = numPixeisTotal * 4;
            int numBytesRestante = numBytesTotal - numBytesInicio - numBytesMensagem;
            byte[] bytesImagem = new byte[numBytesTotal];
            Buffer.BlockCopy(bytesImagemInicio, 0, bytesImagem, 0, numBytesInicio);
            Buffer.BlockCopy(bytesImagemMensagem, 0, bytesImagem, numBytesInicio, numBytesMensagem);
            Buffer.BlockCopy(bytesImagemRestante, 0, bytesImagem, numBytesInicio + numBytesMensagem, numBytesRestante);

            //criar imagem a partir do array obtido
            WriteableBitmap imagemAlterada = TratamentoImagem.ConverterBytesEmImagem(bytesImagem, largura, altura);

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
            #region BLOCO INICIAL

            int numBytesInicio = BytesShort + BytesInt;
            int numBitsInicio = numBytesInicio * 8;

            //obter os pixéis da imagem que necessitam de ser lidos para obter o n.º de bits por componente e o tamanho da mensagem
            int numPixeisInicio = CalcularPixeisUtilizados(numBitsInicio, 1);
            byte[] bytesImagemInicio = TratamentoImagem.ConverterImagemEmBytes(imagemStego, 0, numPixeisInicio);
            byte[][] pixeisInicio = TratamentoImagem.ConverterBytesEmPixeis(bytesImagemInicio);

            //ler a sequência binária escrita nesses pixeis
            BitArray bitsInicio = LerBitsComponentes(pixeisInicio, 1, numBitsInicio);

            //converter a sequência binária nas variáveis que necessitamos para processar pixeis com a mensagem
            byte[] dadosInicio = SequenciaBinaria.SequenciaBinariaParaBytes(bitsInicio, numBytesInicio);
            short bitsAlteradosPorComponente;
            int numBytesMensagem;
            (bitsAlteradosPorComponente, numBytesMensagem) = DecomporBlocoInicial(dadosInicio);

            #endregion

            #region MENSAGEM

            int numBitsMensagem = numBytesMensagem * 8;

            //obter os pixéis da imagem que necessitam de ser lidos para obter a mensagem
            int numPixeisMensagem = CalcularPixeisUtilizados(numBitsMensagem, bitsAlteradosPorComponente);
            byte[] bytesImagemMensagem = TratamentoImagem.ConverterImagemEmBytes(imagemStego, (uint)numPixeisInicio, numPixeisMensagem);
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
        private static byte[] ComporBlocoInicial(short bitsAlterados, int tamanhoMensagem)
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
            byte[] bytesInicio = new byte[BytesShort + BytesInt];
            Buffer.BlockCopy(bytesNumeroBits, 0, bytesInicio, 0, BytesShort);
            Buffer.BlockCopy(bytesTamanhoMensagem, 0, bytesInicio, BytesShort, BytesInt);

            return bytesInicio;
        }

        /// <summary>
        /// Obtém o n.º de bits alterados por componente e o tamanho da mensagem a partir de um bloco de bytes
        /// </summary>
        /// <param name="bytesInicio"></param>
        /// <returns></returns>
        private static (short, int) DecomporBlocoInicial(byte[] bytesInicio)
        {
            //dividir bloco de bytes em dois conforme o espaço ocupado por cada variável
            byte[] bytesNumeroBits = bytesInicio.Take(BytesShort).ToArray();
            byte[] bytesTamanhoMensagem = bytesInicio.Skip(BytesShort).Take(BytesInt).ToArray();

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
