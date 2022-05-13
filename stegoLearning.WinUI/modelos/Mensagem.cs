using System;
using System.Collections.Generic;
using System.Text;

namespace stegoLearning.WinUI.modelos
{
    public class Mensagem
    {
        public string TextoMensagem { get; }
        public List<ItemLetra> ListaLetras { get; }

        public Mensagem(string texto)
        {
            TextoMensagem = texto;
            
            ListaLetras = new List<ItemLetra>();
            foreach (char letra in texto)
            {
                ListaLetras.Add(new ItemLetra(letra));
            }
        }
    }

    public class ItemLetra
    {
        public string TextoLetra { get; }
        public List<ItemByte> ListaBytes { get; }

        public int PixelUtilizado { get; set; } //falta atualizar isto

        public ItemLetra(char letra)
        {
            TextoLetra = letra.ToString();

            ListaBytes = new List<ItemByte>();
            foreach (byte byteValue in Encoding.UTF8.GetBytes(TextoLetra))
            {
                ListaBytes.Add(new ItemByte(byteValue));
            }
        }
    }

    public class ItemByte
    {
        public string TextoByte { get; }
        public List<ItemBit> ListaBits { get; }
        
        public ItemByte(byte valorByte)
        {
            TextoByte = valorByte.ToString();

            ListaBits = new List<ItemBit>();
            foreach (char bit in Convert.ToString(valorByte, 2).PadLeft(8, '0'))
            {
                ListaBits.Add(new ItemBit(bit));
            }
        }
    }

    public class ItemBit
    {
        public string TextoBit { get; }
        public bool BitMarcado { get; set; }
        public int PixelUtilizado { get; set; } //falta atualizar isto
        
        public ItemBit(char valorBit)
        {
            TextoBit = valorBit.ToString();
            BitMarcado = false;
        }
    }
}
