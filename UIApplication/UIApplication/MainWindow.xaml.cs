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
using ClassLib;


namespace UIApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PicProcessing pictureProcessing;
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
            pictureProcessing = new();
            RecognitionResults.Clear();
            await Task.Factory.StartNew(() =>
            {
                foreach (var results in pictureProcessing.ObjectDetecting(FolderPath))
                {
                    dispatcher?.Invoke(() =>
                    {
                        ImageInfo picInfo = new(results);
                        RecognitionResults.Add(picInfo);
                        DbClient.Record(picInfo);
                    });
                    foreach (var res in results.ReconizedObjects)
                    {
                        if (!UniqueCategories.Contains(res.Label))
                        {
                            dispatcher?.Invoke(() =>
                            {
                                UniqueCategories.Add(res.Label);
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
            pictureProcessing?.Cancel();
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
            foreach (var picInfo in DbClient.SelectData())
            {
                RecognitionResults.Add(picInfo);
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
                    DbClient.RemoveItem((ImageInfo)elem);
                    RecognitionResults.Remove((ImageInfo)elem);
                }
                RecognitionResults.OnCollectionChanged();
            }
        }
    }
}
