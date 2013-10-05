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
using System.Collections.ObjectModel;

namespace CornellSunNewsreader
{
    public class StoryListViewModel
    {
        public ObservableCollection<StoryControl> StoryControls { get; private set; }

        private int vid;

        public Boolean DownloadFailed { get; private set; }

        public StoryListViewModel(int vid)
        {
            this.vid = vid;
        }

        internal void DownloadStories()
        {
            throw new NotImplementedException();
        }
    }
}
