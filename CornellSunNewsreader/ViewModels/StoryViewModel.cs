using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Collections.Generic;
using CornellSunNewsreader.Models;
using CornellSunNewsreader.Data;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace CornellSunNewsreader.ViewModels
{
    public class StoryViewModel : INotifyPropertyChanged
    {
        public Story Story { get; private set; }

        public IList<Comment> Comments
        {
            get
            {
                return Story.Comments;
            }
        }

        public bool InFavorites
        {
            get
            {
                return SunData.GetFavorites().Contains(Story);
            }
        }

        public string ContextMenuText
        {
            get
            {
                return InFavorites ? string.Format(App.RemoveFromFavoritesText, "story") : string.Format(App.AddToFavoritesText, "story");
            }
        }

        public void ToggleInFavorites()
        {
            if (InFavorites)
            {
                SunData.GetFavorites().Remove(Story);
                return;
            }

            SunData.GetFavorites().Add(Story);
        }

        public StoryViewModel(Story story)
        {
            this.Story = story;
            SunData.GetFavorites().CollectionChanged += new NotifyCollectionChangedEventHandler(StoryViewModel_CollectionChanged);
        }

        void StoryViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((e.NewItems != null && e.NewItems.Contains(Story)) || (e.OldItems != null && e.OldItems.Contains(Story)))
            {
                onPropChanged("ContextMenuText");
            }
        }

        // it would be nice to just bind to Story directly
        // but for some reason that wasn't working
        public IList<String> Paragraphs
        {
            get
            {
                return Story.Body;
            }
        }

        public string Teaser
        {
            get
            {
                return Story == null ? "" : Story.Teaser;
            }
        }

        public string SectionName
        {
            get
            {
                // I'm not super happy about ToUpper(), but I can't find a better
                // way to do it that doesn't introduce a whole lot of complexity.
                return SunData.GetSections().Where(section => section.Vid == Story.Vid).Single().Name.ToUpper();
            }
        }

        public string Title
        {
            get
            {
                return Story == null ? "" : Story.Title;
            }
        }

        public string Date
        {
            get
            {
                return Story == null || Story.Date == null ? "" : "Updated " + Story.Date;
            }
        }

        public bool IsThumbnailVisible
        {
            get
            {
                return Story != null && Story.ImageSrc != null;
            }
        }

        public BitmapImage Thumbnail
        {
            get
            {
                return IsThumbnailVisible ? new BitmapImage(Story.ImageSrc) : null;
            }
        }

        private double _textScale = Settings.Instance.FontSize;
        public double TextScale
        {
            get
            {
                return _textScale;
            }
            set
            {
                Settings.Instance.FontSize = _textScale = value;
                onPropChanged("TextScale");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void onPropChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
