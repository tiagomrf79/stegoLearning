using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using stegoLearning.WinUI.modelos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace stegoLearning.WinUI.ui
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetalhadoPage : Page
    {
        public List<Seccao> listaSeccoes;

        public DetalhadoPage()
        {
            this.InitializeComponent();
            listaSeccoes = new();

            Seccao seccaoA = new Seccao();
            seccaoA.mensagem = "hel";
            listaSeccoes.Add(seccaoA);

            Seccao seccaoB = new Seccao();
            seccaoB.mensagem = "lo ";
            listaSeccoes.Add(seccaoB);

            Seccao seccaoC = new Seccao();
            seccaoC.mensagem = "wor";
            listaSeccoes.Add(seccaoC);

            Seccao seccaoD = new Seccao();
            seccaoD.mensagem = "ld!";
            listaSeccoes.Add(seccaoD);


            string mensagem = "çHello world!";
            Mensagem novaMensagem = new Mensagem(mensagem);
        }
    }
}
