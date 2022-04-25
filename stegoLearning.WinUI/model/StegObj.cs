using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace stegoLearning.WinUI.model
{
    public class StegObj
    {
        public StegObj(StorageFile file)
        {
            OpcoesBits = new List<string> { "1", "2", "4" };
            OpcoesGuardar = new List<string> { "PNG", "BMP" };

        }
        public string Mensagem { get; set; }
        public string OpcaoBit { get; set; }
        public List<string> OpcoesBits { get; set; }
        public List<string> OpcoesGuardar { get; set; } //usar enum e converter?
        public string PalavraPasse { get; set; }
        public WriteableBitmap ImagemOriginal { get; set; }
        public WriteableBitmap ImagemStego { get; set; }
        public ImageSource ImageSource { get; set; }

        public StorageFile ImageFile { get; set; }
    }
}
