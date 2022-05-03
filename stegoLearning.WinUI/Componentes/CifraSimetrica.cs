using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace stegoLearning.WinUI.Componentes;

internal static class CifraSimetrica
{
    //tamanho em bytes
    private const int TamanhoChave = 32; // 256 bits
    private const int TamanhoSalt = 32; // 256 bits
    private const int TamanhoIV = 16; // 128 bits

    /// <summary>
    /// Encripta uma mensagem de texto usando a palavra-passe fornecida, retornando um array de bytes que inclui o salt e o IV para o processo inverso ser possível.
    /// </summary>
    /// <param name="mensagemOriginal"></param>
    /// <param name="palavraPasse"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] EncriptarMensagem(string mensagemOriginal, string palavraPasse)
    {
        byte[] mensagemParaEncriptar = Encoding.UTF8.GetBytes(mensagemOriginal);

        using (var aes = Aes.Create())
        {
            //gerar salt e iv aleatórios
            byte[] salt = GerarNumeroAleatorio(TamanhoSalt);
            byte[] iv = GerarNumeroAleatorio(TamanhoIV);

            //derivar a chave para ter 256 bits aleatórios
            byte[] chave = DerivarChave(palavraPasse, salt, 100000, TamanhoChave);
            aes.Key = chave;

            //encriptar mensagem
            byte[] mensagemEncriptada = aes.EncryptCbc(mensagemParaEncriptar, iv);

            //juntar mensagem, salt e iv
            byte[] dadosEncriptados = salt;
            dadosEncriptados = dadosEncriptados.Concat(iv).ToArray();
            dadosEncriptados = dadosEncriptados.Concat(mensagemEncriptada).ToArray();

            return dadosEncriptados;
        }
    }

    /// <summary>
    /// Desencripta um array de bytes que inclui a mensagem encriptada, a partir da palavra-passe fornecida.
    /// </summary>
    /// <param name="dadosEncriptados"></param>
    /// <param name="palavraPasse"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// 
    public static byte[] DesencriptarDados(byte[] dadosEncriptados, string palavraPasse)
    {
        using (var aes = Aes.Create())
        {
            //separar mensagem, salt e iv
            byte[] salt = dadosEncriptados.Take(TamanhoSalt).ToArray();
            byte[] iv = dadosEncriptados.Skip(TamanhoSalt).Take(TamanhoIV).ToArray();
            byte[] mensagemEncriptada = dadosEncriptados.Skip(TamanhoSalt + TamanhoIV).ToArray();

            //derivar a chave com o mesmo salt para obter os mesmos 256 bits aleatórios da encriptação
            byte[] chave = DerivarChave(palavraPasse, salt, 100000, TamanhoChave);
            aes.Key = chave;

            //desencriptar mensagem
            byte[] mensagemDesencriptada = aes.DecryptCbc(mensagemEncriptada, iv);

            return mensagemDesencriptada;
        }
    }    /// <summary>
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
        using (var rfc2898 = new Rfc2898DeriveBytes(palavraPasse, salt, numIteracoes, HashAlgorithmName.SHA256))
        {
            return rfc2898.GetBytes(tamanho);
        }
    }
}
