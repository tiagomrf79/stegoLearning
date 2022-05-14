﻿using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

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
            int i = 0;
            foreach (char letra in texto)
            {
                ListaLetras.Add(new ItemLetra(letra, i));
                i++;
            }
        }
    }

    public class ItemLetra
    {
        private char _letra;
        public int Posicao { get; }
        public List<ItemByte> ListaBytes { get; }

        public int PixelUtilizado { get; set; } //falta atualizar isto

        public ItemLetra(char letra, int posicao)
        {
            _letra = letra;
            Posicao = posicao;

            ListaBytes = new List<ItemByte>();
            foreach (byte byteValue in Encoding.UTF8.GetBytes(_letra.ToString()))
            {
                ListaBytes.Add(new ItemByte(byteValue));
            }
        }

        public override string ToString() => _letra.ToString();
    }

    public class SeccaoImagem
    {
        public List<ItemPixel> ListaPixeis { get; }


        public SeccaoImagem(byte[][] pixeis, uint inicio)
        {
            ListaPixeis = new List<ItemPixel>();
            int i = (int)inicio;
            foreach (byte[] pixel in pixeis)
            {
                ListaPixeis.Add(new ItemPixel(pixel, i));
                i++;
            }
        }
    }

    public class ItemPixel
    {
        public WriteableBitmap ImagemPixel { get; }
        public int Posicao { get; }
        public List<ItemComponente> ListaComponentes { get; }

        public ItemPixel(byte[] pixel, int posicao)
        {
            ImagemPixel = CriarImagem(pixel).GetAwaiter().GetResult();
            Posicao = posicao;

            ListaComponentes = new List<ItemComponente>();
            int i = 0;
            foreach (byte componente in pixel)
            {
                ListaComponentes.Add(new ItemComponente(componente, i));
                i++;
            }
        }

        public async Task<WriteableBitmap> CriarImagem(byte[] dados)
        {
            WriteableBitmap imagem = new WriteableBitmap(1, 1);
            using (Stream stream = imagem.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(dados, 0, dados.Length);
            }
            return imagem;
        }
    }

    public class ItemComponente
    {
        //public Color CorComponente { get; }
        public Brush CorComponente { get; }
        public string NomeComponente { get; }
        public ItemByte ByteComponente { get; }

        public ItemComponente(byte valorByte, int posicao)
        {
            switch (posicao)
            {
                case 0:
                    CorComponente = new SolidColorBrush(Colors.LightSkyBlue);
                    NomeComponente = "B";
                    break;
                case 1:
                    CorComponente = new SolidColorBrush(Colors.LightGreen);
                    NomeComponente = "G";
                    break;
                case 2:
                    CorComponente = new SolidColorBrush(Colors.PaleVioletRed);
                    NomeComponente = "R";
                    break;
                case 3:
                    CorComponente = new SolidColorBrush(Colors.SlateGray);
                    NomeComponente = "A";
                    break;
            }
            ByteComponente = new ItemByte(valorByte);
        }
    }

    public class ItemByte
    {
        private byte _valorByte { get; set; }
        public List<ItemBit> ListaBits { get; }
        
        public ItemByte(byte valorByte)
        {
            _valorByte = valorByte;

            ListaBits = new List<ItemBit>();
            int i = 0;
            foreach (char bit in Convert.ToString(_valorByte, 2).PadLeft(8, '0'))
            {
                ListaBits.Add(new ItemBit(bit, i));
                i++;
            }
        }

        public override string ToString() => _valorByte.ToString();
    }

    public class ItemBit
    {
        public string TextoBit { get; }
        public int Posicao { get; }
        public Thickness BorderThickness { get; set; }

        public ItemBit(char valorBit, int posicao)
        {
            Posicao = posicao;
            TextoBit = valorBit.ToString();
            BorderThickness = new Thickness(0);
        }

        public override string ToString() => TextoBit;
    }
}
