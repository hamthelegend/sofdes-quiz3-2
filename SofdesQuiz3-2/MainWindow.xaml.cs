using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SofdesQuiz3_2
{

    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private List<User> _users;
        public List<User> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        StorageFile Picture;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            UpdatePicture();
        }

        private async void BrowseImage(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new()
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            Picture = await picker.PickSingleFileAsync();
            if (Picture != null)
            {
                UpdatePicture();
            }
        }

        private async void Save(object sender, RoutedEventArgs e)
        {
            var user = await ParseUserAsync();
            if (user != null)
            {
                UsersDb.InsertUpdate(user);
                LoadData();
                Clear();
            }
        }

        private void View(object sender, RoutedEventArgs e)
        {
            View(true);
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void First(object sender, RoutedEventArgs e)
        {
            usersDataGrid.SelectedIndex = 0;
            View();
        }

        private void Previous(object sender, RoutedEventArgs e)
        {
            if (usersDataGrid.SelectedIndex > 0) usersDataGrid.SelectedIndex--;
            View();
        }

        private void Next(object sender, RoutedEventArgs e)
        {
            if (usersDataGrid.SelectedIndex != -1 && usersDataGrid.SelectedIndex < Users.Count - 1) usersDataGrid.SelectedIndex++;
            View();
        }

        private void Last(object sender, RoutedEventArgs e)
        {
            usersDataGrid.SelectedIndex = Users.Count - 1;
            View();
        }

        private void LoadData()
        {
            Users = UsersDb.GetAll();
        }

        private async void View(bool showError = false)
        {
            var user = usersDataGrid.SelectedItem as User;
            if (user == null)
            {
                if (showError)
                {
                    await new ContentDialog
                    {
                        Title = "No user selected",
                        Content = "Select a user to view first.",
                        CloseButtonText = "Okay",
                        XamlRoot = Content.XamlRoot,
                    }.ShowAsync();
                }
                return;
            }
            LoadUser(user);
        }

        private void Clear()
        {
            idInput.Text = string.Empty;
            nameInput.Text = string.Empty;
            Picture = null;
            usersDataGrid.SelectedItem = null;
            UpdatePicture();
        }

        private async void LoadUser(User user)
        {
            idInput.Text = user.Id.ToString();
            nameInput.Text = user.Name;

            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync("picture", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, user.Picture);
            Picture = file;
            UpdatePicture();
        }

        private async void UpdatePicture()
        {
            StorageFile file;
            if (Picture == null)
            {
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png"));
            }
            else
            {
                file = Picture;
            }
            using var fileStream = await file.OpenAsync(FileAccessMode.Read);
            BitmapImage bitmapImage = new();
            await bitmapImage.SetSourceAsync(fileStream);
            pictureImage.Source = bitmapImage;
        }

        private async Task<User> ParseUserAsync()
        {
            if (string.IsNullOrEmpty(idInput.Text) ||
                string.IsNullOrEmpty(nameInput.Text) ||
                Picture == null)
            {
                await new ContentDialog
                {
                    Title = "Save failed",
                    Content = "None of the fields can be empty.",
                    CloseButtonText = "Okay",
                    XamlRoot = Content.XamlRoot,
                }.ShowAsync();
                return null;
            }

            var id = int.Parse(idInput.Text);
            var name = nameInput.Text;

            using var inputStream = await Picture.OpenSequentialReadAsync();
            var readStream = inputStream.AsStreamForRead();
            var byteArray = new byte[readStream.Length];
            await readStream.ReadAsync(byteArray);
            return new User(id, name, byteArray);
        }

        private void NaturalNumbersOnly(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private void HidePictureColumn(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Picture")
            {
                e.Cancel = true;
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
