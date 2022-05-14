using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using stegoLearning.WinUI.Componentes;
using stegoLearning.WinUI.modelos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace stegoLearning.WinUI.ui
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetalhadoPage : Page
    {
        private List<Exemplo> _listaExemplos;
        private Exemplo _exemploAtual;

        //private Mensagem _mensagem;
        private List<ItemLetra> _listaLetras;
        private ItemLetra _letraAtual;

        private List<ItemPixel> _listaPixeisOriginais;
        private List<ItemPixel> _listaPixeisStego;
        private ItemPixel _pixelOriginalAtual;
        private ItemPixel _pixelStegoAtual;

        public DetalhadoPage()
        {
            this.InitializeComponent();

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //carregar lista de exemplos
            try
            {
                _listaExemplos = await CarregarListaExemplos();
            }
            catch (Exception ex)
            {
                txtErros.Text = "Não foi possível carregar os exemplos. Tente novamente e reinicie a aplicação caso o erro persista.";
                ErrosLog.EscreverErroEmLog(ex);
                return;
            }

            //carregar primeiro exemplo
            _exemploAtual = _listaExemplos.First();
            exemploAnterior.IsEnabled = false;
            if (_listaExemplos.Count == 1)
            {
                exemploSeguinte.IsEnabled = false;
            }

            //carregar mensagem e imagens no UI
            txtMensagem.Text = _exemploAtual.MensagemEscondida;
            try
            {
                imgOriginal.Source = await CarregarImagem(_exemploAtual.FicheiroOriginal);
                imgStego.Source = await CarregarImagem(_exemploAtual.FicheiroStego);
            }
            catch (Exception ex)
            {
                txtErros.Text = "Não foi possível carregar os exemplos. Tente novamente e reinicie a aplicação caso o erro persista.";
                ErrosLog.EscreverErroEmLog(ex);
                return;
            }

            //decompor imagem original
            WriteableBitmap bitmapOriginal = (WriteableBitmap)imgOriginal.Source;
            byte[] bytesOriginal = TratamentoImagem.ConverterImagemEmBytes(bitmapOriginal, _exemploAtual.PrimeiroPixel, _exemploAtual.NumeroPixeis);
            byte[][] pixeisOriginal = TratamentoImagem.ConverterBytesEmPixeis(bytesOriginal);

            SeccaoImagem seccaoOriginal = new SeccaoImagem(pixeisOriginal, _exemploAtual.PrimeiroPixel);
            _listaPixeisOriginais = seccaoOriginal.ListaPixeis;
            _pixelOriginalAtual = _listaPixeisOriginais.First();
            pixelAnterior.IsEnabled = false;
            if (_listaPixeisOriginais.Count == 1)
            {
                pixelSeguinte.IsEnabled = false;
            }

            ColorirBitsDoPixel(_pixelOriginalAtual);

            //decompor imagem esteganografada
            WriteableBitmap bitmapStego = (WriteableBitmap)imgStego.Source;
            byte[] bytesStego = TratamentoImagem.ConverterImagemEmBytes(bitmapStego, _exemploAtual.PrimeiroPixel, _exemploAtual.NumeroPixeis);
            byte[][] pixeisStego = TratamentoImagem.ConverterBytesEmPixeis(bytesStego);

            SeccaoImagem seccaoStego = new SeccaoImagem(pixeisStego, _exemploAtual.PrimeiroPixel);
            _listaPixeisStego = seccaoStego.ListaPixeis;
            _pixelStegoAtual = _listaPixeisStego.First();

            ColorirBitsDoPixel(_pixelStegoAtual);


            //decompor mensagem
            Mensagem mensagem = new Mensagem(_exemploAtual.MensagemEscondida);
            _listaLetras = mensagem.ListaLetras;
            _letraAtual = _listaLetras.First();


            gridPixelOriginal.DataContext = _pixelOriginalAtual;
            gridPixelStego.DataContext = _pixelStegoAtual;
            gridLetra.DataContext = _letraAtual;

        }

        private void ColorirBitsDoPixel(ItemPixel pixel)
        {
            foreach (ItemComponente componente in pixel.ListaComponentes)
            {
                componente.ByteComponente.ListaBits.Last().BorderThickness = new Thickness(2);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _letraAtual = _listaLetras.SkipWhile(x => x != _letraAtual).Skip(1).DefaultIfEmpty(_listaLetras[0]).FirstOrDefault();
            gridLetra.DataContext = _letraAtual;
        }

        private async Task<List<Exemplo>> CarregarListaExemplos()
        {
            //abrir ficheiro XML que estará numa pasta da aplicação
            StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile storageFile = await storageFolder.GetFileAsync(@"imagens\exemplos.xml");
            Stream stream = await storageFile.OpenStreamForReadAsync();

            //converter XML no tipo de objeto especificado na classe Exemplos
            var serializer = new XmlSerializer(typeof(Exemplos));
            Exemplos exemplos = (Exemplos)serializer.Deserialize(stream);
            stream.Dispose();

            //se o objeto não tem lista ou está vazia => disparar erro
            if (exemplos.ListaExemplos == null || exemplos.ListaExemplos.Count == 0)
            {
                throw new ArgumentNullException("Lista de exemplos vazia.");
            }

            return exemplos.ListaExemplos;
        }

        private async Task<WriteableBitmap> CarregarImagem(string ficheiro)
        {
            StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile storageFile = await storageFolder.GetFileAsync($@"imagens\{ficheiro}");
            WriteableBitmap writeableBitmap = await ImagemIO.ConverterFicheiroEmBitmap(storageFile);
            return writeableBitmap;
        }

        private void exemploAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (_listaExemplos.Any())
            {
                int index = _listaExemplos.IndexOf(_exemploAtual) - 1;
                if (index >= 0)
                {
                    _exemploAtual = _listaExemplos[index];
                    exemploSeguinte.IsEnabled = true;
                    if (index == 0)
                    {
                        exemploAnterior.IsEnabled = false;
                    }
                }
            }
        }

        private void exemploSeguinte_Click(object sender, RoutedEventArgs e)
        {
            if (_listaExemplos.Any())
            {
                int index = _listaExemplos.IndexOf(_exemploAtual) + 1;
                if (index <= _listaExemplos.Count - 1)
                {
                    _exemploAtual = _listaExemplos[index];
                    exemploAnterior.IsEnabled = true;
                    if (index == _listaExemplos.Count - 1)
                    {
                        exemploSeguinte.IsEnabled = false;
                    }
                }
            }
        }

        private void pixelAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (_listaPixeisOriginais.Any())
            {
                int index = _listaPixeisOriginais.IndexOf(_pixelOriginalAtual) - 1;
                if (index >= 0)
                {
                    _pixelOriginalAtual = _listaPixeisOriginais[index];
                    _pixelStegoAtual = _listaPixeisStego[index];
                    
                    gridPixelOriginal.DataContext = _pixelOriginalAtual;
                    gridPixelStego.DataContext = _pixelStegoAtual;

                    ColorirBitsDoPixel(_pixelOriginalAtual);
                    ColorirBitsDoPixel(_pixelStegoAtual);

                    pixelSeguinte.IsEnabled = true;
                    if (index == 0)
                    {
                        pixelAnterior.IsEnabled = false;
                    }
                }
            }
        }

        private void pixelSeguinte_Click(object sender, RoutedEventArgs e)
        {
            if (_listaPixeisOriginais.Any())
            {
                int index = _listaPixeisOriginais.IndexOf(_pixelOriginalAtual) + 1;
                if (index <= _listaPixeisOriginais.Count - 1)
                {
                    _pixelOriginalAtual = _listaPixeisOriginais[index];
                    _pixelStegoAtual = _listaPixeisStego[index];

                    gridPixelOriginal.DataContext = _pixelOriginalAtual;
                    gridPixelStego.DataContext = _pixelStegoAtual;

                    ColorirBitsDoPixel(_pixelOriginalAtual);
                    ColorirBitsDoPixel(_pixelStegoAtual);

                    pixelAnterior.IsEnabled = true;
                    if (index == _listaPixeisOriginais.Count - 1)
                    {
                        pixelSeguinte.IsEnabled = false;
                    }
                }
            }
        }
    }
}
