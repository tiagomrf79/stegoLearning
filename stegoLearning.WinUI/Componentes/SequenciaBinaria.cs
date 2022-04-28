using System.Collections;

namespace stegoLearning.WinUI.Componentes
{
    public static class SequenciaBinaria
    {
        /// <summary>
        /// Converte um byte em sequência binária.
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
        /// Converte uma sequência binária em bytes.
        /// </summary>
        /// <param name="sequenciaBinaria"></param>
        /// <param name="tamanho"></param>
        /// <returns></returns>
        public static byte[] SequenciaBinariaParaBytes(BitArray sequenciaBinaria)
        {
            int tamanho = sequenciaBinaria.Length / 8;
            byte[] aux = new byte[tamanho];
            sequenciaBinaria.CopyTo(aux, 0);
            return aux;
        }
    }
}