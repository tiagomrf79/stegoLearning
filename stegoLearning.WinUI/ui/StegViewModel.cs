using stegoLearning.WinUI.model;
using stegoLearning.WinUI.repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace stegoLearning.WinUI.views
{
    public class StegViewModel : INotifyPropertyChanged
    {
        private StegObj _stego;
        private StegRepo _repository = new StegRepo();

        public StegViewModel()
        {
            EsteganografarCommand = new RelayCommand(OnEsteganografar);
            AbrirImagemCommand = new RelayCommand(OnAbrirImagem);
            GuardarImagemCommand = new RelayCommand(OnGuardarImagem);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public StegObj Stego
        {
            get { return _stego; }
            set
            {
                if (value != _stego)
                {
                    _stego = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Stego"));
                }
            }
        }

        public ICommand EsteganografarCommand { get; private set; }
        public ICommand AbrirImagemCommand { get; private set; }
        public ICommand GuardarImagemCommand { get; private set; }

        public void LoadSteg()
        {
            Stego = _repository.returnStegRepo();
        }

        private void OnAbrirImagem()
        {
            IList<string> lista = Enum.GetNames(typeof(TiposFicheiroOrigem));
            lista = lista.Select(x => "*." + x).ToList();

        }

        public void OnEsteganografar()
        {
            //abrir imagem aqui ou chamar método no repositório
        }
        private async void OnGuardarImagem()
        {
            IList<string> lista = Enum.GetNames(typeof(TiposFicheiroDestino));
            lista = lista.Select(x => "*." + x).ToList();

            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            fileSavePicker.FileTypeChoices.Add("Bitmap files", new List<string>() { ".bmp" });
            fileSavePicker.SuggestedFileName = "stego";

            //var window = (Application.Current as App)?.m_window as MenuWindow;
            //IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(fileSavePicker, App.appWindowHandle);

            StorageFile storageFile = await fileSavePicker.PickSaveFileAsync();

        }

    }
}