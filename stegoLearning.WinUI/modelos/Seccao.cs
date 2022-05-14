using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;

namespace stegoLearning.WinUI.modelos
{
    public class Seccao
    {
        public WriteableBitmap imagem { get; set; }
        public string mensagem { get; set; }
        public IEnumerable<PixelOld> listaPixeis { get; set; }
    }
}
