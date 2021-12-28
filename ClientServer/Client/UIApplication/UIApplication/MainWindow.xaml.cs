using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Ookii.Dialogs.Wpf;
using ClientLib;


namespace UIApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private string folderPath;

        public MainWindow()
        {
            RecognitionResults = new();
            UniqueCategories = new();
            InitializeComponent();
            DataContext = this;

            StartRecognition.IsEnabled = false;
            StopRecognition.IsEnabled = false;
            StorageOpen.IsEnabled = false;
            OpenFolder.IsEnabled = false;
            RecognitionOpen.IsEnabled = true;

            LoadData();
        }

        public ObservableResults RecognitionResults { get; }
        public ObservableCollection<string> UniqueCategories { get; }

        public string FolderPath
        {
            get => folderPath;
            set
            {
                if (value != folderPath)
                {
                    folderPath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderPath)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void StartRecognition_Click(object sender, RoutedEventArgs e)
        {
            StartRecognition.IsEnabled = false;
            StopRecognition.IsEnabled = true;
            RecognitionResults.Clear();
            await Task.Factory.StartNew(() =>
            {
                var client = new ClientSession();
                foreach (var imgInfo in client.PostAsync(FolderPath))
                {
                    dispatcher?.Invoke(() => RecognitionResults.Add(new(imgInfo)));
                    foreach (var label in imgInfo.RecognizedObjects.Select(obj => obj.Key))
                    {
                        if (!UniqueCategories.Contains(label))
                        {
                            dispatcher?.Invoke(() =>
                            {
                                UniqueCategories.Add(label);
                            });
                        }
                    }
                }
            });

            StopRecognition.IsEnabled = false;
            StartRecognition.IsEnabled = true;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new();
            bool open = (bool)folderDialog.ShowDialog();
            if (open)
            {
                StartRecognition.IsEnabled = true;
                FolderPath = folderDialog.SelectedPath;
            }
        }

        private void StopRecognition_Click(object sender, RoutedEventArgs e)
        {
            new ClientSession().Cancel();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = true;
            var item = (ImageInfo)e.Item;
            if (ObjectCategories.SelectedItems.Count > 0)
            {
                e.Accepted = false;
                foreach (var category in ObjectCategories.SelectedItems)
                {
                    if (item.RecognizedObjects.Select(cat => cat.Key).Contains(category))
                    {
                        e.Accepted = true;
                    }
                }
            }
        }

        private void ObjectCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecognitionResults?.OnCollectionChanged();
        }

        private void LoadData()
        {
            foreach (var imgInfo in ClientSession.Get())
            {
                RecognitionResults.Add(new(imgInfo));
            }
        }

        private void StorageOpen_Click(object sender, RoutedEventArgs e)
        {
            RecognitionOpen.IsEnabled = true;
            StartRecognition.IsEnabled = false;
            StopRecognition.IsEnabled = false;
            StorageOpen.IsEnabled = false;
            OpenFolder.IsEnabled = false;
            RemoveItem.IsEnabled = true;
            FolderPath = "";
            RecognitionResults.Clear();
            UniqueCategories.Clear();
            LoadData();
        }

        private void RecognitionOpen_Click(object sender, RoutedEventArgs e)
        {
            StorageOpen.IsEnabled = true;
            RecognitionOpen.IsEnabled = false;
            OpenFolder.IsEnabled = true;
            RemoveItem.IsEnabled = false;
            RecognitionResults.Clear();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {

            if (ImageContent.SelectedItems.Count > 0)
            {
                foreach (var elem in ImageContent.SelectedItems)
                {
                    var imgInfo = (ImageInfo)elem;
                    ClientSession.Delete(imgInfo.Id);
                    RecognitionResults.Remove(imgInfo);
                }
                RecognitionResults.OnCollectionChanged();
            }
        }
    }
}
