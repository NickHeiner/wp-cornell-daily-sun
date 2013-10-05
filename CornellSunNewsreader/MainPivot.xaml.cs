// #define HYPERLINK_TEXTBLOCK_TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.Collections.Specialized;
using CornellSunNewsreader.ViewModels;
using System.ComponentModel;
using System.Collections.ObjectModel;
using CornellSunNewsreader.Models;
using CornellSunNewsreader.Data;
using CornellSunNewsreader.Views;

namespace CornellSunNewsreader
{
    public partial class MainPivot : PhoneApplicationPage, INotifyPropertyChanged
    {
        public IEnumerable<SectionViewModel> SectionViewModels
        {
            get
            {
                return SunData.GetSections().Select(section => new SectionViewModel(section));
            }
        }

        public ScrollBarVisibility VerticalScrolling
        {
            get
            {
                return NoFavorites == System.Windows.Visibility.Visible ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            }
        }

        public IEnumerable<SectionViewModel> FavoriteSections
        {
            get
            {
                return from item in SunData.GetFavorites()
                       where item is Section
                       select new SectionViewModel(item as Section);
            }
        }

        /// <summary>
        /// Visible if there are favorites, but no favorite sections.
        /// We should show the user text letting them know that they can add favorite sections.
        /// </summary>
        public Visibility DisplayFavoriteSectionsHelp
        {
            get
            {
                return FavoriteSections.Count() == 0 && FavoritesExist == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Visible if any of the favorites are stories.
        /// </summary>
        public Visibility FavoriteStoriesExist
        {
            get
            {
                return FavoriteStories.Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Visible if there are favorites, but no favorite stories.
        /// We should show the user text letting them know that they can add favorite stories.
        /// </summary>
        public Visibility DisplayFavoriteStoriesHelp
        {
            get
            {
                return FavoriteStories.Count() == 0 && FavoritesExist == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// View of favorites that only includes stories.
        /// </summary>
        public IEnumerable<StoryViewModel> FavoriteStories
        {
            get
            {
                // maybe it would be better to split Favorites into two collections,
                // one for Sections and one for Stories
                // to save the difficulty of this casting / manual type checking.
                return from item in SunData.GetFavorites()
                       where item is Story
                       select new StoryViewModel(item as Story);
            }
        }

        /// <summary>
        /// Visible if there are no favorites of any kind.
        /// </summary>
        public Visibility NoFavorites
        {
            get
            {
                return SunData.GetFavorites().Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Visibility _inverseVisibility(Visibility visibility)
        {
            return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Visible if any favorites exist.
        /// </summary>
        public Visibility FavoritesExist
        {
            get
            {
                return _inverseVisibility(NoFavorites);
            }
        }

        public double TextScale { get; set; }

        public MainPivot()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPivot_Loaded);
            LayoutRoot.DataContext = this;
            SunData.GetFavorites().CollectionChanged += new NotifyCollectionChangedEventHandler(Favorites_CollectionChanged);

#if HYPERLINK_TEXTBLOCK_TEST
            LayoutRoot.Children.Add(new HyperlinkTextBlock() { Text = "Oooo marmalade www.google.com I like cheese nth23@cornell.edu and horses <a href=\"http://www.bing.com\">bing</a> " +
                "and the moon <a href=\"http://www.google.com\">google</a> damn yeezy"
            });
#endif
        }

        void Favorites_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            onPropChanged("NoFavorites");
            onPropChanged("VerticalScrolling");
            onPropChanged("FavoriteSections");
            onPropChanged("DisplayFavoriteSectionsHelp");
            onPropChanged("FavoriteStories");
            onPropChanged("FavoriteStoriesExist");
            onPropChanged("DisplayFavoriteStoriesHelp");
            onPropChanged("FavoritesExist");
        }

        void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            SunData.DownloadFailed += new DownloadStringCompletedEventHandler(SunData_SectionDownloadFailed);
            SunData.SectionsAcquired += new EventHandler(SunData_SectionsAcquired);
        }

        void SunData_SectionsAcquired(object sender, EventArgs e)
        {
            onPropChanged("SectionViewModels");
            setDownloadSucceeded(true);
        }

        void SunData_SectionDownloadFailed(object sender, DownloadStringCompletedEventArgs e)
        {
            setDownloadSucceeded(false);
        }

        private void setDownloadSucceeded(bool success)
        {
            // if this line runs, the format will break when a listbox item is selected
            // I have no idea why
            // http://stackoverflow.com/questions/4565535
            // 
            // LoadingProgressBar.Visibility = Visibility.Collapsed;

            // Instead, here's a stupid hack:
            LoadingProgressBar.Margin = new Thickness(100000);

            bool someSectionsFound = success || SectionViewModels.Count() > 0;

            DownloadFailed.Visibility = someSectionsFound ? Visibility.Collapsed : Visibility.Visible;
            sectionList.Visibility = someSectionsFound ? Visibility.Visible : Visibility.Collapsed;
        }

        void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            Uri page = null;
            object selected = e.AddedItems[0];
            if (selected is NavigableItem)
            {
                NavigableItem selectedItem = (NavigableItem)selected;
                page = selectedItem.Page;
            }
            else if (selected is SectionViewModel)
            {
                SectionViewModel selectedVM = (SectionViewModel)selected;
                page = selectedVM.Section.Page;
            }
            else if (selected is StoryViewModel)
            {
                StoryViewModel selectedVM = (StoryViewModel)selected;
                page = selectedVM.Story.Page;
            }

            Debug.Assert(page != null, "What is the type of `selected`?");

            NavigationService.Navigate(page);

            ListBox selectedBox = (ListBox)sender;
            selectedBox.SelectedIndex = -1;
        }

        private void SectionContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SectionViewModel clicked = (SectionViewModel)((FrameworkElement)sender).DataContext;

            if (clicked.InFavorites)
            {
                SunData.GetFavorites().Remove(clicked.Section);
            }
            else
            {
                SunData.GetFavorites().Add(clicked.Section);
            }
        }

        private void FavoritesContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            NavigableItem clicked;
            object dataContext = ((FrameworkElement)sender).DataContext;
            if (dataContext is SectionViewModel)
            {
                clicked = ((SectionViewModel)dataContext).Section;
            }
            else
            {
                clicked = ((StoryViewModel)dataContext).Story;
            }
            SunData.GetFavorites().Remove(clicked);
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
