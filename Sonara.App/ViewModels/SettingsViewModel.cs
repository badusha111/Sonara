using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonara.Core;
using System.Collections.ObjectModel;

namespace Sonara.App
{
    public partial class SettingsViewModel(ILibraryRepository repository) : ObservableObject
    {
        public ObservableCollection<string> LibraryFolders { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddFolderCommand))]
        private string folderPath = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        [NotifyCanExecuteChangedFor(nameof(AddFolderCommand))]
        private bool isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string errorMessage = "";

        public bool IsNotBusy => !IsBusy;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public async Task LoadAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = "";
            try
            {
                await RefreshFoldersAsync();
            }
            catch (Exception)
            {
                ErrorMessage = "Не удалось загрузить папки. Откройте настройки ещё раз.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanAddFolder() => IsNotBusy && !string.IsNullOrWhiteSpace(FolderPath);

        [RelayCommand(CanExecute = nameof(CanAddFolder))]
        private async Task AddFolderAsync()
        {
            if (!CanAddFolder()) return;

            var path = FolderPath.Trim();
            IsBusy = true;
            ErrorMessage = "";
            try
            {
                await repository.AddLibraryFolderAsync(path);
                FolderPath = "";
                try
                {
                    await RefreshFoldersAsync();
                }
                catch (Exception)
                {
                    ErrorMessage = "Папка добавлена, но список не обновился. Откройте настройки ещё раз.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Не удалось добавить папку. Проверьте путь и попробуйте ещё раз.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshFoldersAsync()
        {
            var folders = await repository.GetLibraryFoldersAsync();
            LibraryFolders.Clear();
            foreach (var folder in folders) LibraryFolders.Add(folder);
        }
    }
}
