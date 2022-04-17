using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections;
using System.IO;
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
        /// Cria uma imagem esteganografada a partir de uma imagem original e de uma mensagem
        /// </summary>
        /// <param name="bytesMensagem"></param>
        /// <param name="numeroBits"></param>
        /// <param name="imagemOriginal"></param>
        /// <returns></returns>
        public static WriteableBitmap AdicionarMensagem(byte[] bytesMensagem, short numeroBits, WriteableBitmap imagemOriginal)
        {
            int tamanhoMensagem = bytesMensagem.Length;

            byte[] bytesImagem = imagemOriginal.PixelBuffer.ToArray(); //cada item do array tem um componente (b, g, r ou alpha)
            byte[] bytesNumeroBits = BitConverter.GetBytes(numeroBits);
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

            //n.º de bits e tamanho vão ficar no mesmo bloco
            byte[] bytesInicio = new byte[BytesShort + BytesInt];
            Buffer.BlockCopy(bytesNumeroBits, 0, bytesInicio, 0, BytesShort);
            Buffer.BlockCopy(bytesTamanhoMensagem, 0, bytesInicio, BytesShort, BytesInt);

            //criar array com tudo o que irá ser escrito na imagem:
            byte[] bytesConteudo = new byte[BytesShort + BytesInt + bytesMensagem.Length];
            Buffer.BlockCopy(bytesInicio, 0, bytesConteudo, 0, BytesShort + BytesInt);
            Buffer.BlockCopy(bytesMensagem, 0, bytesConteudo, BytesShort + BytesInt, bytesMensagem.Length);

            //converter em sequência binária todo o conteúdo que se pretende guardar na mensagem
            BitArray bitsConteudo = new BitArray(bytesConteudo);

            //obter n.º de componentes que serão utilizados
            int numComponentesMensagem = CalcularNumeroComponentes(bytesMensagem.Length, numeroBits);
            int numComponentesInicio = CalcularNumeroComponentes(BytesShort + BytesInt, 1);

            int totalBits = bitsConteudo.Length;
            int bitsEscritos = 0;

            for (int i = 0; i < numComponentesInicio + numComponentesMensagem; i++) //percorrer tantos componentes quantos os necessários
            {
                if (i % 4 == 3) //saltar o componente alpha
                {
                    continue;
                }

                byte componente = bytesImagem[i];

                for (int k = 0; k < numeroBits && bitsEscritos < totalBits; k++) //percorrer tantos bits quanto os que queremos alterar por componente
                {
                    componente = Esteganografia.AlterarBitComponenteBGR(componente, k, bitsConteudo.Get(bitsEscritos));
                    bitsEscritos++;

                    //os bits iniciais (n.º de bits por componente e o tamanho da mensagem) só utilizam 1 bit por componente
                    if (bitsEscritos <= (BytesShort + BytesInt) * 8)
                    {
                        break;
                    }
                }

                bytesImagem[i] = componente;
            }

            WriteableBitmap imagemAlterada = new WriteableBitmap(imagemOriginal.PixelWidth, imagemOriginal.PixelHeight);
            using (Stream stream = imagemAlterada.PixelBuffer.AsStream())
            {
                stream.Write(bytesImagem, 0, bytesImagem.Length);
            }
            
            return imagemAlterada;
        }

        /// <summary>
        /// Extrai uma mensagem (criptografada ou não) de uma imagem esteganografada.
        /// </summary>
        /// <param name="imagemStego"></param>
        /// <returns></returns>
        public static byte[] DesteganografarImagemOld(WriteableBitmap imagemStego)
        {
            short numeroBits;
            int tamanhoMensagem;

            int numComponentesInicio = CalcularNumeroComponentes(BytesShort + BytesInt, 1);
            byte[] blocoInicial = imagemStego.PixelBuffer.ToArray(0, numComponentesInicio);

            byte[] dadosInicio = ExtrairDadosImagem(blocoInicial, 1, BytesShort + BytesInt);
            (numeroBits, tamanhoMensagem) = ObterParametrosMensagem(dadosInicio);

            int numComponentesMensagem = CalcularNumeroComponentes(tamanhoMensagem, numeroBits);
            byte[] blocoMensagem = imagemStego.PixelBuffer.ToArray((uint)numComponentesInicio, numComponentesMensagem);

            byte[] dadosMensagem = ExtrairDadosImagem(blocoMensagem, numeroBits, tamanhoMensagem);

            return dadosMensagem;
        }

        /// <summary>
        /// Calcula quantos componentes são necessários percorrer para guardar o conteúdo pretendido, em tantos bits por componente.
        /// </summary>
        /// <param name="conteudo"></param>
        /// <param name="numBits"></param>
        /// <returns></returns>
        private static int CalcularNumeroComponentes(int numBytes, int numBits)
        {
            //cada pixel tem 4 componentes (inclui alpha) e vão ser alterados os 3 componentes r, g e b
            double numComponentesAlterados = (double)numBytes * 8 / numBits;
            double numPixeisAlterados = numComponentesAlterados / 3;
            int numComponentesPercorridos = (int)(numPixeisAlterados * 4);
            return numComponentesPercorridos;
        }

        /// <summary>
        /// Altera um determinado bit de um componente da imagem.
        /// </summary>
        /// <param name="componente"></param>
        /// <param name="posicaoBit"></param>
        /// <param name="valorNovo"></param>
        /// <returns></returns>
        private static byte AlterarBitComponenteBGR(byte componente, int posicaoBit, bool valorNovo)
        {
            BitArray componenteBits = new BitArray(new byte[] { componente });
            componenteBits.Set(posicaoBit, valorNovo);
            byte[] componenteNovo = new byte[1];
            componenteBits.CopyTo(componenteNovo, 0);
            return componenteNovo[0];
        }

        /// <summary>
        /// Obtém um determinado bit de um componente da imagem.
        /// </summary>
        /// <param name="componente"></param>
        /// <param name="posicaoBit"></param>
        /// <returns></returns>
        private static bool ObterBitComponenteBGR(byte componente, int posicaoBit)
        {
            BitArray componenteBits = new BitArray(new byte[] { componente });
            return componenteBits.Get(posicaoBit);
        }

        /// <summary>
        /// Retorna o n.º bits utilizados e o tamanho da mensagem, a partir dos dados esteganografados da zona inicial da imagem
        /// </summary>
        /// <param name="dadosInicio"></param>
        /// <returns></returns>
        private static (short, int) ObterParametrosMensagem(byte[] dadosInicio)
        {
            byte[] bytesNumeroBits = dadosInicio.Take(BytesShort).ToArray();
            byte[] bytesTamanhoMensagem = dadosInicio.Skip(BytesShort).Take(BytesInt).ToArray();
            
            //BitConverter vai criar o array conforme a arquitectura do computador onde corre
            //para assegurar que é coerente entre máquinas:
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytesNumeroBits);
                Array.Reverse(bytesTamanhoMensagem);
            }

            short numeroBits = BitConverter.ToInt16(bytesNumeroBits, 0);
            int tamanhoMensagem = BitConverter.ToInt32(bytesTamanhoMensagem, 0);

            return (numeroBits, tamanhoMensagem);
        }

        /// <summary>
        /// Lê um determinado bloco da imagem e retorna os dados esteganografados respetivos
        /// </summary>
        /// <param name="bytesImagem"></param>
        /// <param name="numeroBits"></param>
        /// <param name="tamanhoMensagem"></param>
        /// <returns></returns>
        private static byte[] ExtrairDadosImagem(byte[] bytesImagem, short numeroBits, int tamanhoMensagem)
        {
            BitArray bitArray = new BitArray(tamanhoMensagem * 8);
            int bitsLidos = 0;

            for (int i = 0; i < bytesImagem.Length; i++)
            {
                if (i % 4 == 3) //saltar o componente alpha
                {
                    continue;
                }

                for (int j = 0; j < numeroBits; j++)
                {
                    bitArray.Set(bitsLidos, ObterBitComponenteBGR(bytesImagem[i], j));
                    bitsLidos++;
                }
            }

            byte[] byteArray = new byte[tamanhoMensagem];
            bitArray.CopyTo(byteArray, 0);

            return byteArray;
        }



        /********************************************************************
         * TESTES DE NOVA VERSÃO
         ********************************************************************/


        public static byte[] DesteganografarImagem(WriteableBitmap imagemStego)
        {
            int numBytesInicio = BytesShort + BytesInt;
            int numPixeisInicio = CalcularPixeisUtilizados(numBytesInicio * 8, 1);
            byte[] bytesInicio = LerDadosImagem(imagemStego, numBytesInicio, 0, 1, numPixeisInicio);

            short numeroBits;
            int tamanhoMensagem;
            (numeroBits, tamanhoMensagem) = DecomporBlocoInicial(bytesInicio);

            int numPixeisMensagem = CalcularPixeisUtilizados(tamanhoMensagem * 8, numeroBits);
            byte[] bytesMensagem = LerDadosImagem(imagemStego, tamanhoMensagem, (uint)numPixeisInicio, numeroBits, numPixeisMensagem);

            return null;
        }

        /// <summary>
        /// Cria uma imagem com uma mensagem esteganografada
        /// </summary>
        /// <param name="imagemOriginal"></param>
        /// <param name="bytesMensagem"></param>
        /// <param name="bitsAlterados"></param>
        /// <returns></returns>
        public static WriteableBitmap EsteganografarImagem(WriteableBitmap imagemOriginal, byte[] bytesMensagem, short bitsAlterados)
        {
            int largura = imagemOriginal.PixelWidth;
            int altura = imagemOriginal.PixelHeight;
            int tamanhoMensagem = bytesMensagem.Length;

            //construir um array com os bits alterados e o tamanho da mensagem, que irá ocupar 48 bits
            byte[] bytesInicio = ComporBlocoInicial(bitsAlterados, tamanhoMensagem);

            //incluir esse array no início dos dados da imagem
            byte[] dadosImagemInicio = IncluirDadosImagem(imagemOriginal, bytesInicio, 0, 1);

            //incluir a mensagem (encriptada ou não) na zona seguinte dos dados da imagem
            byte[] dadosImagemMensagem = IncluirDadosImagem(imagemOriginal, bytesMensagem, (uint)dadosImagemInicio.Length, bitsAlterados);

            int numBytesAlterados = dadosImagemInicio.Length + dadosImagemMensagem.Length;
            int totalPixeis = largura * altura;
            int totalBytes = totalPixeis * 4; //cada pixel ocupa 4 bytes, um byte por componente

            byte[] bytesImagemRestante = imagemOriginal.PixelBuffer.ToArray((uint)numBytesAlterados, totalBytes - numBytesAlterados);

            //juntar tudo no mesmo array (início + mensagem + restante)
            byte[] bytesImagem = new byte[totalBytes];
            Buffer.BlockCopy(dadosImagemInicio, 0, bytesImagem, 0, dadosImagemInicio.Length);
            Buffer.BlockCopy(dadosImagemMensagem, 0, bytesImagem, dadosImagemInicio.Length, dadosImagemMensagem.Length);
            Buffer.BlockCopy(bytesImagemRestante, 0, bytesImagem, numBytesAlterados, totalBytes - numBytesAlterados);

            //criar imagem a partir do array obtido
            WriteableBitmap imagemAlterada = TratamentoImagem.ConverterBytesEmImagem(bytesImagem, largura, altura);
            return imagemAlterada;
        }


        private static int CalcularPixeisUtilizados(int totalBits, int bitsPorComponente)
        {
            double numComponentesAlterados = (double)totalBits / bitsPorComponente;
            int numPixeisAlterados = (int)Math.Ceiling(numComponentesAlterados / 3); //pixéis alterados parcialmente também contam
            return numPixeisAlterados;
        }

        /// <summary>
        /// Altera os bytes de uma imagem com o conteudo que se pretende esconder nos bits menos significativos
        /// </summary>
        /// <param name="writeableBitmap"></param>
        /// <param name="conteudo"></param>
        /// <param name="pixelInicial"></param>
        /// <param name="bitsPorComponente"></param>
        /// <returns></returns>
        private static byte[] IncluirDadosImagem(WriteableBitmap writeableBitmap, byte[] conteudo, uint pixelInicial, int bitsPorComponente)
        {
            //converter os dados em sequência binária
            BitArray bits = new BitArray(conteudo);
            int totalBits = bits.Length;

            //obter pixeis onde pretendo escrever os dados
            int numPixeis = CalcularPixeisUtilizados(totalBits, bitsPorComponente);
            byte[] bytesImagem = TratamentoImagem.ConverterImagemEmBytes(writeableBitmap, pixelInicial, numPixeis);
            byte[][] pixeis = TratamentoImagem.ConverterBytesEmPixeis(bytesImagem);

            //percorrer os pixeis e alterar os bits dos respetivos componentes B, G e R
            int bitsEscritos = 0;
            for (int i = 0; i < numPixeis; i++)
            {
                for (int j = 0; j < 3 && bitsEscritos < totalBits; j++) //ignorar componente alpha
                {
                    for (int k = 0; k < bitsPorComponente && bitsEscritos < totalBits; k++) //k indica a posição do bit a alterar (começa no menos significativo)
                    {
                        pixeis[i][j] = AlterarBitComponenteBGR(pixeis[i][j], k, bits.Get(bitsEscritos));
                        bitsEscritos++;
                    }
                }
            }

            //retornar dados da imagem alterada
            bytesImagem = TratamentoImagem.ConverterPixeisEmBytes(pixeis);
            return bytesImagem;
            //return ConverterPixeis_EmDadosImagem(pixeis);
        }

        private static byte[] LerDadosImagem(WriteableBitmap writeableBitmap, int tamanho, uint pixelInicial, int bitsPorComponente, int numPixeis)
        {
            byte[] bytesImagem = TratamentoImagem.ConverterImagemEmBytes(writeableBitmap, pixelInicial, numPixeis);
            byte[][] pixeis = TratamentoImagem.ConverterBytesEmPixeis(bytesImagem);

            int totalBits = tamanho * 8;
            BitArray bitArray = new BitArray(totalBits);
            int bitsLidos = 0;

            for (int i = 0; i < pixeis.Length; i++)
            {
                for (int j = 0; j < 3 && bitsLidos < totalBits; j++) //ignorar componente alpha
                {
                    for (int k = 0; k < bitsPorComponente && bitsLidos < totalBits; k++) //k indica a posição do bit a alterar (começa no menos significativo)
                    {
                        bool valorBit = ObterBitComponenteBGR(pixeis[i][j], k);
                        bitArray.Set(bitsLidos, valorBit);
                        bitsLidos++;
                    }
                }
            }

            byte[] byteArray = new byte[tamanho];
            bitArray.CopyTo(byteArray, 0);

            return byteArray;
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
