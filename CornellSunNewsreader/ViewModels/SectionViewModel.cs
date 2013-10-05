using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;
using CornellSunNewsreader.Models;
using CornellSunNewsreader.Data;

namespace CornellSunNewsreader.ViewModels
{
    public class SectionViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// If the name is longer than this, an acronym will be used instead
        /// to avoid breaking the UI.
        /// </summary>
        public const int MaxNameLength = 13;

        /// <summary>
        /// The count of stories that will be displayed in this section
        /// on the Favorites page
        /// </summary>
        public const int TopStoriesCount = 2;

        public IEnumerable<StoryViewModel> TopStoryViewModels
        {
            get
            {
                return StoryViewModels.Take(TopStoriesCount);
            }
        }

        public Section Section { get; private set; }

        public bool InFavorites
        {
            get
            {
                return SunData.GetFavorites().Contains(Section);
            }
        }

        public string ContextMenuText
        {
            get
            {
                return InFavorites ? string.Format(App.RemoveFromFavoritesText, "section") : string.Format(App.AddToFavoritesText, "section");
            }
        }

        public ObservableCollection<Story> Stories { get; private set; }

        // I'd rather have this in Sections.xaml.cs, but I can't make the binding work.
        // http://stackoverflow.com/questions/4618466/silverlight-binding-to-a-layoutroot-value-from-within-a-datatemplate
        // http://stackoverflow.com/questions/4556894/silverlight-why-doesnt-this-binding-expression-work
        public Visibility NoStoryContent
        {
            get
            {
                return SunData.GetAllStories().Count() == 0 && !SunData.AnySectionCurrentlyDownloading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void OnDownloadFailed()
        {
            onPropChanged("NoStoryContent");
        }

        public IEnumerable<StoryViewModel> StoryViewModels
        {
            get
            {
                ObservableCollection<Story> dataStories = SunData.GetStories(Section);

                if (dataStories != Stories)
                {
                    Stories = dataStories;
                    Stories.CollectionChanged += new NotifyCollectionChangedEventHandler(stories_CollectionChanged);
                }

                return Stories.Select(story => new StoryViewModel(story));
            }
        }

        public override string ToString()
        {
            return "SectionViewModel: " + Section.ToString();
        }

        void stories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            onPropChanged("StoryViewModels");
            onPropChanged("TopStoryViewModels");
        }

        public SectionViewModel(Section section)
        {
            Section = section;

            SunData.GetFavorites().CollectionChanged += new NotifyCollectionChangedEventHandler(SectionViewModel_CollectionChanged);
        }

        void SectionViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((e.NewItems != null && e.NewItems.Contains(Section)) || (e.OldItems != null && e.OldItems.Contains(Section)))
            {
                onPropChanged("ContextMenuText");
            }
        }

        public string DisplayName
        {
            get
            {
                return Section.Name.Length > MaxNameLength ? makeAcronym(Section.Name) : Section.Name;
            }
        }

        string makeAcronym(string str)
        {
            return new string(str.Where((c, i) => !Char.IsWhiteSpace(c) && (i == 0 || Char.IsWhiteSpace(str[i - 1]))).ToArray());
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // make this an extension?
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
