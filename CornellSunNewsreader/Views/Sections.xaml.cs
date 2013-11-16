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
using System.Collections.ObjectModel;
using System.Diagnostics;
using CornellSunNewsreader.ViewModels;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Navigation;
using CornellSunNewsreader.Data;
using CornellSunNewsreader.Models;
using DanielVaughan.WindowsPhone7Unleashed;
using System.Threading;
using Microsoft.Phone.Shell;

namespace CornellSunNewsreader
{
    public partial class Sections : PhoneApplicationPage, INotifyPropertyChanged
    {
        private ICollection<PivotItem> sectionsCurrentlyLoading = new List<PivotItem>();

        public ICommand FetchMoreDataCommand { get; private set; }

        public Visibility LoadingBarVisibility
        {
            get
            {
                return SunData.AnySectionCurrentlyDownloading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private ObservableCollection<SectionViewModel> _sectionViewModels;
        public ObservableCollection<SectionViewModel> SectionViewModels
        {
            get
            {
                // this returns a new collection each time, ordered such that the section 
                // we are currently viewing is at the front of the list.

                // don't do anything if we haven't loaded yet
                if (NavigationContext == null)
                {
                    return null;
                }

                if (_sectionViewModels == null)
                {
                    _sectionViewModels = new ObservableCollection<SectionViewModel>();
                    _sectionViewModels.AddAll(SunData.GetSections().Select(section => new SectionViewModel(section)).ToList());

                    foreach (SectionViewModel sectionViewModel in _sectionViewModels)
                    {
                        SunData.GetStories(sectionViewModel.Section);
                    }
                }

                // re-index the source so we don't get an annoying transition animation
                // http://stackoverflow.com/questions/4541020/
                int activeVID = int.Parse(NavigationContext.QueryString[Section.SectionsKey]);

                SectionViewModel selectedItem = _sectionViewModels.Where(sectionVM => sectionVM.Section.Vid == activeVID).Single();
                int selectedIndex = _sectionViewModels.IndexOf(selectedItem);

                Queue<SectionViewModel> reordered = new Queue<SectionViewModel>(_sectionViewModels);
                for (int i = 0; i < selectedIndex; i++)
                {
                    // pop an item from the list and push it to the end
                    reordered.Enqueue(reordered.Dequeue());
                }

                ObservableCollection<SectionViewModel> sectionVMs = new ObservableCollection<SectionViewModel>();
                sectionVMs.AddAll(reordered.ToList());

                return sectionVMs;
            }
        }

        public Sections()
        {
            InitializeComponent();
            SunData.DownloadFailed += new DownloadStringCompletedEventHandler(SunData_DownloadFailed);
            SunData.DownloadStatusChanged += new EventHandler((s, e) => onPropChanged("LoadingBarVisibility"));

            Loaded += new RoutedEventHandler(Sections_Loaded);
            LayoutRoot.DataContext = this;

            FetchMoreDataCommand = new DelegateCommand(
                obj =>
                {
                    if (LoadingBarVisibility == System.Windows.Visibility.Visible)
                    {
                        // if we're already loading something, don't fire off a new request.
                        return;
                    }

                    // Stories are kept in an observable collection, so it shouldn't be necessary to use an onFinished 
                    // callback to manually fire onPropChanged().
                    SunData.GetMoreStories(SectionViewModels[pivotControl.SelectedIndex].Section);
                });
        }

        public void refreshPinToStartText()
        {
            // this can't be done through XAML because the app bar isn't a Silverlight element
            // http://blogs.msdn.com/b/ptorr/archive/2010/06/18/why-are-the-applicationbar-objects-not-frameworkelements.aspx
            var menuItem = ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]);
            var selectedSection = ((SectionViewModel)pivotControl.SelectedItem);
            menuItem.Text = String.Format("pin {0} to start", selectedSection.DisplayName);
            menuItem.IsEnabled = getTileForSection(selectedSection) == null;
        }

        void Sections_Loaded(object sender, RoutedEventArgs e)
        {
            onPropChanged("SectionViewModels");
        }

        void SunData_DownloadFailed(object sender, DownloadStringCompletedEventArgs e)
        {
            foreach (SectionViewModel sectionVM in _sectionViewModels)
            {
                sectionVM.OnDownloadFailed();
            }
        }

        // TODO: remember what the scroll value is, and reset the user to that position when we return
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // if nothing has been selected, then the page is still loading
            // don't bother setting QueryString to be something different
            if (pivotControl.SelectedItem != null)
            {
                NavigationContext.QueryString[Section.SectionsKey] = ((SectionViewModel)pivotControl.SelectedItem).Section.Vid.ToString();
                // pivotControl.ItemTemplate.
                // NavigationContext.QueryString["scrollVerticalOffset"] = ((VisualTreeHelper.GetChild(listBox, 0) as FrameworkElement).FindName("ScrollViewer") as ScrollViewer).VerticalOffset;
            }
            base.OnNavigatedFrom(e);
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

        private void AppBarPinToStart_Click(object sender, EventArgs e)
        {
            var selectedSection = (SectionViewModel)pivotControl.SelectedItem;
            ShellTile sectionTile = getTileForSection(selectedSection);
            Debug.Assert(sectionTile == null, "How was the 'pin to start' app bar item clicked if the section tile already exists?");
            ShellTile.Create(selectedSection.Section.Page, new StandardTileData
            {
                Title = selectedSection.DisplayName,
                BackgroundImage = new Uri("Tile Size.png", UriKind.Relative)
            });
        }

        private static ShellTile getTileForSection(SectionViewModel selectedSection)
        {
            return ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri.Equals(selectedSection.Section.Page));
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            ApplicationBar.Opacity = e.IsMenuVisible ? StoryPage.APP_BAR_ACTIVE_OPACITY : StoryPage.APP_BAR_INACTIVE_OPACITY;
        }

        private void pivotControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When we first set pivotControl.SelectedItem to be something, it will fire an event
            // while the selected item is null. If we get that event, just ignore it.
            if (pivotControl.SelectedItem != null)
            {
                refreshPinToStartText();
            }
        }

        private void AppBarColophon_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Colophon.xaml", UriKind.Relative));
        }
    }
}
