using MacroscopTestWork.Commands;
using MacroscopTestWork.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private int _downloadProgress;
        public int DownloadProgress
        {
            get => _downloadProgress;
            private set
            {
                _downloadProgress = value;
                OnPropertyChanged(nameof(DownloadProgress));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText => $"{DownloadProgress}%";

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
            DownloadProgress = 0;
            _cts = new CancellationTokenSource();

            try
            {
                // Симуляция прогресса (так как реальный прогресс сложно получить)
                var progressTask = SimulateProgressAsync(_cts.Token);
                var downloadTask = _downloadService.DownloadAsync(Url, _cts.Token);

                var result = await downloadTask;
                Image = result;
                DownloadProgress = 100;
                await progressTask; // Дожидаемся завершения симуляции прогресса
            }
            catch (OperationCanceledException)
            {
                DownloadProgress = 0;
            }
            catch (Exception ex)
            {
                DownloadProgress = 0;
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

        private async Task SimulateProgressAsync(CancellationToken token)
        {
            for (int i = 0; i <= 100; i += 10)
            {
                if (token.IsCancellationRequested)
                    break;

                DownloadProgress = i;
                await Task.Delay(100, token);
            }
        }

        public ImageItemViewModel(IImageDownloadService DownloadService)
        {
            _downloadService = DownloadService;
            ToggleCommand = new RelayCommand(async _ => await ToggleAsync());
        }

        public async Task StartDownloadIfNeeded()
        {
            if (!IsLoading && !string.IsNullOrWhiteSpace(Url)) 
                await StartDownloadAsync();
        }
    }
}
