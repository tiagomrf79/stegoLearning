using System.Collections;

namespace stegoLearning.WinUI
{
    public static class SequenciaBinaria
    {
        /// <summary>
        /// Converte byte em sequência binária.
        /// </summary>
        /// <param name="valorByte"></param>
        /// <returns></returns>
        public static BitArray BytesParaSequenciaBinaria(byte valorByte)
        {
            return new BitArray(new byte[] { valorByte });
        }

        /// <summary>
        /// Converte bytes em sequência binária.
        /// </summary>
        /// <param name="valorByte"></param>
        /// <returns></returns>
        public static BitArray BytesParaSequenciaBinaria(byte[] valorByte)
        {
            return new BitArray(valorByte);
        }

        /// <summary>
        /// Converte uma sequência binária em byte.
        /// </summary>
        /// <param name="sequenciaBinaria"></param>
        /// <returns></returns>
        public static byte SequenciaBinariaParaBytes(BitArray sequenciaBinaria)
        {
            byte[] aux = new byte[1];
            sequenciaBinaria.CopyTo(aux, 0);
            return aux[0];
        }

        /// <summary>
        /// Converte uma sequência binária em bytes.
        /// </summary>
        /// <param name="sequenciaBinaria"></param>
        /// <param name="tamanho"></param>
        /// <returns></returns>
        public static byte[] SequenciaBinariaParaBytes(BitArray sequenciaBinaria, int tamanho)
        {
            byte[] aux = new byte[tamanho];
            sequenciaBinaria.CopyTo(aux, 0);
            return aux;
        }

        /// <summary>
        /// Altera o valor de um bit numa sequência binária.
        /// </summary>
        /// <param name="sequenciaBinaria"></param>
        /// <param name="posicao"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static BitArray AlterarSequenciaBinaria(BitArray sequenciaBinaria, int posicao, bool valor)
        {
            sequenciaBinaria.Set(posicao, valor);
            return sequenciaBinaria;
        }

        /// <summary>
        /// Obtém o valor de um bit numa sequência binária.
        /// </summary>
        /// <param name="sequenciaBinaria"></param>
        /// <param name="posicao"></param>
        /// <returns></returns>
        public static bool ObterBitSequenciaBinaria(BitArray sequenciaBinaria, int posicao)
        {
            return sequenciaBinaria.Get(posicao);
        }
    }
}