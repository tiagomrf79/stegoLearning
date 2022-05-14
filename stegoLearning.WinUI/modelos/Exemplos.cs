using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace stegoLearning.WinUI.modelos
{
    [XmlRoot(ElementName = "exemplos")]
    public class Exemplos
    {
        [XmlElement(ElementName = "exemplo")]
        public List<Exemplo> ListaExemplos { get; set; }
    }

    [XmlRoot(ElementName = "exemplo")]
    public class Exemplo
    {
        [XmlElement(ElementName = "ficheiro_original")]
        public string FicheiroOriginal { get; set; }

        [XmlElement(ElementName = "ficheiro_stego")]
        public string FicheiroStego { get; set; }

        [XmlElement(ElementName = "mensagem")]
        public string MensagemEscondida { get; set; }

        [XmlElement(ElementName = "primeiro_pixel")]
        public uint PrimeiroPixel { get; set; }

        [XmlElement(ElementName = "numero_pixeis")]
        public int NumeroPixeis { get; set; }
    }
}
