using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using CornellSunNewsreader.ViewModels;
using CornellSunNewsreader.Models;
using CornellSunNewsreader.Data;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System.ComponentModel;

namespace CornellSunNewsreader
{
    // TODO does flipping the phone orientation change the text of the currently active story without changing the image?
    public partial class StoryPage : PhoneApplicationPage
    {
        public static readonly double APP_BAR_ACTIVE_OPACITY = 1;
        public static readonly double APP_BAR_INACTIVE_OPACITY = .5;

        private readonly double MIN_FONT_SIZE = 13;
        private readonly double MAX_FONT_SIZE = 45;
        private double initialScale;

        private StoryViewModel getCurrentStory()
        {
            return ((StoryViewModel)LayoutRoot.DataContext);
        }

        public StoryPage()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(StoryPage_Loaded);
        }

        void StoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            int nid = int.Parse(NavigationContext.QueryString["nid"]);

            Story story = SunData.getStory(nid);
            LayoutRoot.DataContext = new StoryViewModel(story);
        }

        void SectionTitle_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            getCurrentStory().ToggleInFavorites();            
        }

        private void AppBarShare_Click(object sender, EventArgs e)
        {
            new EmailComposeTask {
                Subject = "Cornell Sun: " + getCurrentStory().Title,
                Body = getCurrentStory().Teaser + "\n\nFull article: " + getCurrentStory().Story.CornellSunOnlineUri
            }.Show();
        }

        private void AppBarViewOnline_Click(object sender, EventArgs e)
        {
            new WebBrowserTask { Uri = new Uri(getCurrentStory().Story.CornellSunOnlineUri.AbsoluteUri, UriKind.Absolute) }.Show();
        }

        private void AppBarShareSms_Click(object sender, EventArgs e)
        {
            // It would be nice to do this as a shortened link, if there is enough demand.
            // It would be a bit tricky, because either we'd have to make a RPC to a link shortening service
            // after the user taps "share via sms", and make them wait, or shorten eagerly, but create
            // many more links than necessary.
            new SmsComposeTask { Body = getCurrentStory().Story.CornellSunOnlineUri.AbsoluteUri }.Show();
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            ApplicationBar.Opacity = e.IsMenuVisible ? APP_BAR_ACTIVE_OPACITY : APP_BAR_INACTIVE_OPACITY;
        }

        private void ShareViaSocialNetwork_Click(object sender, EventArgs e)
        {
            new ShareLinkTask { LinkUri = getCurrentStory().Story.CornellSunOnlineUri, Title = getCurrentStory().Title, Message = getCurrentStory().Teaser }.Show();
        }
    }
}
