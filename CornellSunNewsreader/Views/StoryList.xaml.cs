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
using CornellSunNewsreader.ViewModels;
using System.Windows.Navigation;
using DanielVaughan.WindowsPhone7Unleashed;
using CornellSunNewsreader.Data;
using Microsoft.Phone.Shell;

namespace CornellSunNewsreader.Views
{
    public partial class StoryList : UserControl
    {
        public StoryList()
        {
            InitializeComponent();

            // would it make more sense for the DataContext to be Host?
            LayoutRoot.DataContext = this;

            Loaded += (s, e) =>
                {
                    //MessageBox.Show(ToString() + " loaded. StateId is: " + StateId);
                    
                    if (!double.IsNaN(StateVerticalOffset))
                    {
                        CurrentVerticalOffset = StateVerticalOffset;
                    }
                };
        }

        private double CurrentVerticalOffset
        {
            get
            {
                return FindScrollViewer(listBox).VerticalOffset;
            }
            set
            {
                FindScrollViewer(listBox).ScrollToVerticalOffset(value);
            }
        }

        private double StateVerticalOffset
        {
            get
            {
                object verticalScrollOffset;
                PhoneApplicationService.Current.State.TryGetValue(StateId, out verticalScrollOffset);
                return verticalScrollOffset == null ? double.NaN : (double)verticalScrollOffset;
            }
            set
            {
                PhoneApplicationService.Current.State[StateId] = value;
            }
        }

        // from http://stackoverflow.com/questions/4951812/how-do-i-get-the-exact-scroll-position-in-an-listbox
        static ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var elt = VisualTreeHelper.GetChild(parent, i);
                if (elt is ScrollViewer)
                {
                    return (ScrollViewer)elt;
                };
                var result = FindScrollViewer(elt);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static readonly DependencyProperty StateIdProperty = DependencyProperty.RegisterAttached(
            "StateId",
            typeof(string),
            typeof(StoryList),
            new PropertyMetadata(null));

        /// <summary>
        /// A unique identifier this story list will use to store its scroll offset in the State dictionary.
        /// </summary>
        public string StateId
        {
            get { return (string)GetValue(StateIdProperty); }
            set { SetValue(StateIdProperty, value); }
        }

        public static readonly DependencyProperty StoryViewModelsProperty = DependencyProperty.RegisterAttached(
            "StoryViewModels",
            typeof(IEnumerable<StoryViewModel>),
            typeof(StoryList),
            new PropertyMetadata(null, new PropertyChangedCallback(onStoryViewModelsChanged)));

        private static void onStoryViewModelsChanged(DependencyObject dependObj, DependencyPropertyChangedEventArgs e)
        {
            // MessageBox.Show("Story view models set to " + e.NewValue.ToString());
        }

        public IEnumerable<StoryViewModel> StoryViewModels
        {
            get { return (IEnumerable<StoryViewModel>)GetValue(StoryViewModelsProperty); }
            set { SetValue(StoryViewModelsProperty, value); }
        }

        public static readonly DependencyProperty HostProperty = DependencyProperty.RegisterAttached(
            "Host",
            typeof(Sections),
            typeof(StoryList),
            new PropertyMetadata(null));

        public Sections Host
        {
            get { return (Sections)GetValue(HostProperty); }
            set { SetValue(HostProperty, value); }
        }

        private void StoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            StateVerticalOffset = CurrentVerticalOffset;

            StoryViewModel selected = (StoryViewModel)e.AddedItems[0];
            Host.NavigationService.Navigate(selected.Story.Page);
        }
    }
}
