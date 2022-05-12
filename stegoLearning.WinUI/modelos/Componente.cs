using System.Collections.Generic;

namespace stegoLearning.WinUI.modelos
{
    public class Componente
    {
        public string nomeComponente { get; set; } //blue, green, red, alpha
        public char abreviaturaComponente { get; set; } //b, g, r, a
        public byte valorHexadecimal { get; set; }
        public int valorNumerico { get; set; }
        public IEnumerable<bool> listaBits { get; set; }
        public bool componenteOriginal { get; set; }
        public Componente componenteAlterado { get; set; }
    }
}
