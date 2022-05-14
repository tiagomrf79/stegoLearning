using System.Collections.Generic;

namespace stegoLearning.WinUI.modelos
{
    public class PixelOld
    {
        public int indicePixel { get; set; }
        public string corHexadecimal { get; set; } //necessário?
        public IEnumerable<Componente> listaComponentes { get; set; }
        public bool pixelOriginal { get; set; }
        public PixelOld pixelAlterado { get; set; }
    }
}
