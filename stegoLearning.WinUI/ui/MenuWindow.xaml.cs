using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using stegoLearning.WinUI.ui;
using System;
using System.Collections.Generic;
using System.Linq;

/*
adaptado de exemplo em:
https://docs.microsoft.com/en-us/windows/apps/design/controls/navigationview
*/

namespace stegoLearning.WinUI.UI;

public sealed partial class MenuWindow : Window
{
    private double NavViewCompactModeThresholdWidth { get { return NavView.CompactModeThresholdWidth; } }

    //Lista para associar Tag do item à respetiva página
    private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
    {
        ("steg", typeof(EsteganografarPage)),
        ("unsteg", typeof(DesteganografarPage)),
        ("exemplos", typeof(DetalhadoPage))
    };

    public MenuWindow()
    {
        this.InitializeComponent();
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Não foi possível abrir a página " + e.SourcePageType.FullName);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        //associar método a executar quando evento Navigated ocorre
        ContentFrame.Navigated += On_Navigated;

        //carregar a página de esteganografia por defeito
        NavView.SelectedItem = NavView.MenuItems[0];
        NavView_Navigate("steg", new EntranceNavigationTransitionInfo());
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer != null)
        {
            //obter tag do item seleccionado
            var navItemTag = args.InvokedItemContainer.Tag.ToString();

            //sair da aplicação se clicar no botão Sair
            if (navItemTag == "exit")
            {
                Environment.Exit(0);
            }

            //chamar método que abre uma nova página
            NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
    {
        //obter da lista qual a página que deve abrir com base no tag
        Type _page = null;
        var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
        _page = item.Page;

        //obter página atual para comparar com a nova página
        var preNavPageType = ContentFrame.CurrentSourcePageType;

        //verificar que página foi seleccionada e que é diferente da atual
        if (!(_page is null) && !Type.Equals(preNavPageType, _page))
        {
            ContentFrame.Navigate(_page, null, transitionInfo);
        }
    }

    private void On_Navigated(object sender, NavigationEventArgs e)
    {
        if (ContentFrame.SourcePageType != null)
        {
            var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

            NavView.SelectedItem = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .First(n => n.Tag.Equals(item.Tag));

            NavView.Header =
                ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
        }
    }

    private void svConteudo_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        //chama os métodos específicos de cada página para ajustar o conteúdo ao espaço disponível
        if (ContentFrame.SourcePageType != null)
        {
            string tag = ((NavigationViewItem)NavView.SelectedItem).Tag.ToString();
            if (tag == "steg")
            {
                (ContentFrame.Content as EsteganografarPage).AjustarTamanhoImagem(e.NewSize.Width);
            }
            else if (tag == "unsteg")
            {
                (ContentFrame.Content as DesteganografarPage).AjustarTamanhoImagem(e.NewSize.Width);
            }
        }
    }
}
