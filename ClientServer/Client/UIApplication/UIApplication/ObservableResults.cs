using System.Collections.Generic;
using System.Collections.Specialized;


namespace UIApplication
{
    public partial class MainWindow
    {
        public class ObservableResults : List<ImageInfo>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public new void Add(ImageInfo pictureInfo)
            {
                base.Add(pictureInfo);
                OnCollectionChanged();
            }            

            public new void Clear()
            {
                base.Clear();
                OnCollectionChanged();
            }

            /*public new bool Remove(PictureInfo elem)
            {
                bool res = base.Remove(elem);
                OnCollectionChanged();
                return res;
            }*/

            public void OnCollectionChanged() => 
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
