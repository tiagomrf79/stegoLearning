using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace stegoLearning.WinUI.Componentes
{
    internal static class CifraSimetrica
    {
        //tamanho em bytes
        private const int TamanhoChave = 32; // 256 bits
        private const int TamanhoSalt = 32; // 256 bits
        private const int TamanhoIV = 16; // 128 bits

        /// <summary>
        /// Gera um array de bytes aleatórios com o tamanho pretendido.
        /// </summary>
        /// <param name="tamanho"></param>
        /// <returns></returns>
        private static byte[] GerarNumeroAleatorio(int tamanho)
        {
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                byte[] numeroAleatorio = new byte[tamanho];
                randomNumberGenerator.GetBytes(numeroAleatorio);

                return numeroAleatorio;
            }
        }

        /// <summary>
        /// Transforma a palavra-passe escolhida num array de bytes com o tamanho pretendido, derivando a mesma várias vezes.
        /// </summary>
        /// <param name="palavraPasse"></param>
        /// <param name="salt"></param>
        /// <param name="numIteracoes"></param>
        /// <param name="tamanho"></param>
        /// <returns></returns>
        private static byte[] DerivarChave(string palavraPasse, byte[] salt, int numIteracoes, int tamanho)
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(palavraPasse, salt, numIteracoes))
            {
                return rfc2898.GetBytes(tamanho);
            }
        }

        /// <summary>
        /// Encripta uma mensagem de texto usando a palavra-passe fornecida, retornando um array de bytes que inclui o salt e o IV para o processo inverso ser possível.
        /// </summary>
        /// <param name="mensagemOriginal"></param>
        /// <param name="palavraPasse"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] EncriptarMensagem(string mensagemOriginal, string palavraPasse)
        {
            if (string.IsNullOrEmpty(palavraPasse)) throw new ArgumentException("palavraPasse");
            if (string.IsNullOrEmpty(mensagemOriginal)) throw new ArgumentException("mensagemOriginal");

            byte[] mensagemParaEncriptar = Encoding.UTF8.GetBytes(mensagemOriginal);

            using (var aes = Aes.Create())
            {
                byte[] salt = GerarNumeroAleatorio(TamanhoSalt);
                byte[] chave = DerivarChave(palavraPasse, salt, 100000, TamanhoChave);
                byte[] iv = GerarNumeroAleatorio(TamanhoIV);

                aes.Key = chave;
                aes.IV = iv;

                using (var memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(mensagemParaEncriptar, 0, mensagemParaEncriptar.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    var dadosEncriptados = salt;
                    dadosEncriptados = dadosEncriptados.Concat(iv).ToArray();
                    dadosEncriptados = dadosEncriptados.Concat(memoryStream.ToArray()).ToArray();

                    return dadosEncriptados;
                }
            }
        }

        /// <summary>
        /// Desencripta um array de bytes que inclui a mensagem encriptada, a partir da palavra-passe fornecida.
        /// </summary>
        /// <param name="dadosEncriptados"></param>
        /// <param name="palavraPasse"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] DesencriptarDados(byte[] dadosEncriptados, string palavraPasse)
        {
            if (dadosEncriptados == null || dadosEncriptados.Length == 0) throw new ArgumentException("dadosEncriptados");
            if (string.IsNullOrEmpty(palavraPasse)) throw new ArgumentException("palavraPasse");

            using (var aes = Aes.Create())
            {
                byte[] salt = dadosEncriptados.Take(TamanhoSalt).ToArray();
                byte[] iv = dadosEncriptados.Skip(TamanhoSalt).Take(TamanhoIV).ToArray();
                byte[] mensagemEncriptada = dadosEncriptados.Skip(TamanhoSalt + TamanhoIV).ToArray();

                byte[] chave = DerivarChave(palavraPasse, salt, 100000, TamanhoChave);

                aes.Key = chave;
                aes.IV = iv;

                using (var memoryStream = new MemoryStream())
                {
                    try
                    {
                        //var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(mensagemEncriptada, 0, mensagemEncriptada.Length);
                            cryptoStream.FlushFinalBlock();
                        }

                        var mensagemDesencriptada = memoryStream.ToArray();
                        return mensagemDesencriptada;
                    }
                    catch (Exception)
                    {
                        //se falhar a desencriptação (p.ex. chave errada)
                        //retornar null e mostrar erro
                        return Encoding.UTF8.GetBytes("Desencriptação falhou!");
                    }
                }
            }
        }
    }
}
