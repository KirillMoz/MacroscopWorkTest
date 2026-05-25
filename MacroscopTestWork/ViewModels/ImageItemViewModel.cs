using MacroscopTestWork.Commands;
using MacroscopTestWork.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MacroscopTestWork.ViewModels
{
    public class ImageItemViewModel: ViewModelBase
    {
        private readonly IImageDownloadService _downloadService;
        private CancellationTokenSource? _cts;
        private Task? _downloadTask;
        public ICommand ToggleCommand { get; }
        public event Action? LoadingStateChanged;
        public event Action<string>? ErrorOccurred;

        private string _url = string.Empty;
        public string Url
        {
            get => _url;
            set 
            { 
                _url = value; 
                OnPropertyChanged(nameof(Url)); 
            }
        }

        private ImageSource? _image;
        public ImageSource? Image
        {
            get => _image;
            set 
            { 
                _image = value; 
                OnPropertyChanged(nameof(Image)); 
            }
        }

        private string _buttonText = "Старт";
        public string ButtonText
        {
            get => _buttonText;
            private set 
            { 
                _buttonText = value; 
                OnPropertyChanged(nameof(ButtonText)); 
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                ButtonText = _isLoading ? "Стоп" : "Старт";
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(ButtonText));
            }
        }

        private async Task StopAsync()
        {
            _cts?.Cancel();
            if (_downloadTask != null)
                try 
                { 
                    await _downloadTask; 
                } 
                catch 
                {
                    
                }
        }

        public async Task ToggleAsync()
        {
            if (IsLoading) 
                await StopAsync();
            else 
                if (!string.IsNullOrWhiteSpace(Url)) 
                    await StartDownloadAsync();
        }
        private async Task StartDownloadAsync()
        {
            IsLoading = true;
            _cts = new CancellationTokenSource();
            try
            {
                Image = await _downloadService.DownloadAsync(Url, _cts.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                _cts?.Dispose();
                _cts = null;
                LoadingStateChanged?.Invoke();
            }
        }

        public ImageItemViewModel(IImageDownloadService DownloadService)
        {
            _downloadService = DownloadService;
            ToggleCommand = new RelayCommand(async _ => await ToggleAsync());
        }

        public async Task StartDownloadIfNeeded()
        {
            if (!IsLoading && !string.IsNullOrWhiteSpace(Url)) await StartDownloadAsync();
        }
    }
}
