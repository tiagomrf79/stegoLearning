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

namespace stegoLearning.WinUI.ui;

public sealed partial class DetalhadoPage : Page
{
    //variáveis para navegação na página
    private List<Exemplo> _listaExemplos;
    private Exemplo _exemploAtual;

    private List<ItemLetra> _listaLetras;
    private ItemLetra _letraAtual;

    private List<ItemPixel> _listaPixeisOriginais;
    private ItemPixel _pixelOriginalAtual;

    private List<ItemPixel> _listaPixeisStego;
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

        //ativar ou desativar botões de navegação
        exemploAnterior.IsEnabled = false;
        if (_listaExemplos.Count == 1)
        {
            exemploSeguinte.IsEnabled = false;
        }

        //carregar o 1.º exemplo da lista
        _exemploAtual = _listaExemplos.First();
        await CarregarExemplo(_exemploAtual);
    }

    /// <summary>
    /// Carregar lista com exemplos definidos no XML
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
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

    /// <summary>
    /// Carregar letras e pixéís de um exemplo
    /// </summary>
    /// <param name="exemplo"></param>
    /// <returns></returns>
    private async Task CarregarExemplo(Exemplo exemplo)
    {
        #region CARREGAR MENSAGEM

        //colocar mensagem no UI
        txtMensagem.Text = _exemploAtual.MensagemEscondida;

        //decompor mensagem em letras, bytes e bits
        Mensagem mensagem = new Mensagem(_exemploAtual.MensagemEscondida);

        //atualizar variáveis da mensagem
        _listaLetras = mensagem.ListaLetras;
        _letraAtual = _listaLetras.First();

        #endregion

        #region CARREGAR IMAGEM ORIGINAL

        //obter imagem especificada no objeto obtido do xml
        WriteableBitmap bitmapOriginal = null;
        try
        {
            bitmapOriginal = await CarregarImagem(_exemploAtual.FicheiroOriginal);
        }
        catch (Exception ex)
        {
            txtErros.Text = "Não foi possível carregar os exemplos. Tente novamente e reinicie a aplicação caso o erro persista.";
            ErrosLog.EscreverErroEmLog(ex);
            return;
        }

        //colocar imagem no UI
        imgOriginal.Source = bitmapOriginal;

        //atualizar variáveis dos pixéis originais
        _listaPixeisOriginais = CarregarListaDePixeis(bitmapOriginal, _exemploAtual);
        _pixelOriginalAtual = _listaPixeisOriginais.First();

        //colorir último bit do pixel que vai ser mostrado
        ColorirBitsDoPixel(_pixelOriginalAtual);

        //ativar ou desativar botões de navegação
        pixelAnterior.IsEnabled = false;
        if (_listaPixeisOriginais.Count == 1)
        {
            pixelSeguinte.IsEnabled = false;
        }

        #endregion

        #region CARREGAR IMAGEM ESTEGANOGRAFADA

        //obter imagem especificada no objeto obtido do xml
        WriteableBitmap bitmapStego = null;
        try
        {
            bitmapStego = await CarregarImagem(_exemploAtual.FicheiroStego);
        }
        catch (Exception ex)
        {
            txtErros.Text = "Não foi possível carregar os exemplos. Tente novamente e reinicie a aplicação caso o erro persista.";
            ErrosLog.EscreverErroEmLog(ex);
            return;
        }

        //colocar imagem no UI
        imgStego.Source = bitmapStego;

        //atualizar variáveis dos pixéis originais
        _listaPixeisStego = CarregarListaDePixeis(bitmapStego, _exemploAtual);
        _pixelStegoAtual = _listaPixeisStego.First();

        //colorir último bit do pixel que vai ser mostrado
        ColorirBitsDoPixel(_pixelStegoAtual);

        #endregion

        //associar pixéis esteganografados aos bits alterados e respetivas letras (para navegação entre letras e border nos bits)
        int ultimoPixel = 0;
        foreach (ItemLetra letra in _listaLetras)
        {
            foreach (ItemByte valorByte in letra.ListaBytes)
            {
                for (int i = valorByte.ListaBits.Count - 1; i >= 0; i -= 4)
                {
                    _listaPixeisStego[ultimoPixel].LetraAssociada = letra;
                    _listaPixeisStego[ultimoPixel].BitsAssociados = valorByte.ListaBits.Skip(i - 3).Take(4).ToList();
                    ultimoPixel++;
                }
            }
        }

        //colorir bits da letra que foram escritos no pixel
        ColorirBitsDaLetra(_pixelStegoAtual.BitsAssociados, 2);

        //forçar binding para atualizar UI
        gridLetra.DataContext = _letraAtual;
        gridPixelOriginal.DataContext = _pixelOriginalAtual;
        gridPixelStego.DataContext = _pixelStegoAtual;
    }

    /// <summary>
    /// Obter imagem guardada na pasta da aplicação
    /// </summary>
    /// <param name="ficheiro"></param>
    /// <returns></returns>
    private async Task<WriteableBitmap> CarregarImagem(string ficheiro)
    {
        //obter ficheiro da pasta da aplicação
        StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
        StorageFile storageFile = await storageFolder.GetFileAsync($@"imagens\{ficheiro}");

        //converter ficheiro em bitmap
        WriteableBitmap writeableBitmap = await ImagemIO.ConverterFicheiroEmBitmap(storageFile);

        return writeableBitmap;
    }

    /// <summary>
    /// Obter pixéis de uma das imagens de exemplo
    /// </summary>
    /// <param name="imagem"></param>
    /// <param name="exemplo"></param>
    /// <returns></returns>
    private List<ItemPixel> CarregarListaDePixeis(WriteableBitmap imagem, Exemplo exemplo)
    {
        //obter pixeis que foram esteganografados
        byte[] bytes = TratamentoImagem.ConverterImagemEmBytes(imagem, exemplo.PrimeiroPixel, exemplo.NumeroPixeis);
        byte[][] pixeis = TratamentoImagem.ConverterBytesEmPixeis(bytes);

        //decompor esses pixeis em pixéis, componentes, bytes e bits
        SeccaoImagem seccao = new SeccaoImagem(pixeis, exemplo.PrimeiroPixel);

        return seccao.ListaPixeis;
    }

    /// <summary>
    /// Atualizar UI ao carregar novo pixel
    /// </summary>
    /// <param name="posicao"></param>
    private void CarregarPixel(int posicao)
    {
        //remover border dos bits da letra antiga
        ColorirBitsDaLetra(_pixelStegoAtual.BitsAssociados, 0);

        //obter novos pixéis
        _pixelOriginalAtual = _listaPixeisOriginais[posicao];
        _pixelStegoAtual = _listaPixeisStego[posicao];

        //colorir último bit dos novos pixéis
        ColorirBitsDoPixel(_pixelOriginalAtual);
        ColorirBitsDoPixel(_pixelStegoAtual);

        //obter nova letra (pode ser a mesma)
        _letraAtual = _pixelStegoAtual.LetraAssociada;

        //colocar border nos bits da nova letra
        ColorirBitsDaLetra(_pixelStegoAtual.BitsAssociados, 2);

        //forçar binding para atualizar UI
        gridPixelOriginal.DataContext = _pixelOriginalAtual;
        gridPixelStego.DataContext = _pixelStegoAtual;
        gridLetra.DataContext = null;
        gridLetra.DataContext = _letraAtual;
    }
    
    /// <summary>
    /// Colorir último bit de cada pixel esteganografado
    /// </summary>
    /// <param name="pixel"></param>
    private void ColorirBitsDoPixel(ItemPixel pixel)
    {
        foreach (ItemComponente componente in pixel.ListaComponentes)
        {
            componente.ByteComponente.ListaBits.Last().BorderThickness = new Thickness(2);
        }
    }

    /// <summary>
    /// Colorir bits da letra que foram esteganografados no pixel atual
    /// </summary>
    /// <param name="bits"></param>
    /// <param name="valor"></param>
    private void ColorirBitsDaLetra(List<ItemBit> bits, int valor)
    {
        foreach (ItemBit bit in bits)
        {
            bit.BorderThickness = new Thickness(valor);
        }
    }

    private async void exemploAnterior_Click(object sender, RoutedEventArgs e)
    {
        if (_listaExemplos.Any())
        {
            int index = _listaExemplos.IndexOf(_exemploAtual) - 1;
            if (index >= 0)
            {
                _exemploAtual = _listaExemplos[index];
                await CarregarExemplo(_exemploAtual);

                exemploSeguinte.IsEnabled = true;
                if (index == 0)
                {
                    exemploAnterior.IsEnabled = false;
                }
            }
        }
    }

    private async void exemploSeguinte_Click(object sender, RoutedEventArgs e)
    {
        if (_listaExemplos.Any())
        {
            int index = _listaExemplos.IndexOf(_exemploAtual) + 1;
            if (index <= _listaExemplos.Count - 1)
            {
                _exemploAtual = _listaExemplos[index];
                await CarregarExemplo(_exemploAtual);

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
                CarregarPixel(index);

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
                CarregarPixel(index);

                pixelAnterior.IsEnabled = true;
                if (index == _listaPixeisOriginais.Count - 1)
                {
                    pixelSeguinte.IsEnabled = false;
                }
            }
        }
    }
}
