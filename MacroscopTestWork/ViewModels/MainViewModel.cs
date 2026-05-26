using MacroscopTestWork.Commands;
using MacroscopTestWork.Options;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MacroscopTestWork.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ImageItemViewModel> ImageItems { get; } = new();
        private int _activeDownloads;
        public int ActiveDownloads
        {
            get => _activeDownloads;
            private set 
                { 
                    _activeDownloads = value; 
                    OnPropertyChanged(nameof(ActiveDownloads)); 
                }
        }

        public ICommand LoadAllCommand { get; }
        private readonly LoaderOptions _opts;

        public MainViewModel(IOptions<LoaderOptions> options, Func<ImageItemViewModel> slotFactory)
        {
            _opts = options.Value;
            for (int i = 0; i < _opts.DefaultSlotCount; i++) 
                AddImage(slotFactory());
            LoadAllCommand = new RelayCommand(async _ => await LoadAllAsync());
        }

        private void AddImage(ImageItemViewModel imageitem)
        {
            imageitem.LoadingStateChanged += RecalculateActive;
            imageitem.ErrorOccurred += ShowError;
            ImageItems.Add(imageitem);
            RecalculateActive();
        }



        private void RecalculateActive() =>
            ActiveDownloads = ImageItems.Count(s => s.IsLoading);

        private void ShowError(string msg) =>
            System.Windows.MessageBox.Show(msg, "Внимание", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

        private async Task LoadAllAsync() =>
            await Task.WhenAll(ImageItems.Select(s => s.StartDownloadIfNeeded()));
    }
}
