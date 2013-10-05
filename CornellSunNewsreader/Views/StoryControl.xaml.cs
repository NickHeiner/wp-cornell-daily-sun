using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using CornellSunNewsreader.Models;
using CornellSunNewsreader.ViewModels;

namespace CornellSunNewsreader
{
	public partial class StoryControl : UserControl
	{
		public StoryControl()
		{
			InitializeComponent();
		}

        internal StoryControl(Story story) : this()
        {
            DataContext = new StoryViewModel(story);
        }

        private void ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((StoryViewModel)DataContext).ToggleInFavorites();
        }
    }
}