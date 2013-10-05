using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;

namespace CornellSunNewsreader
{
    public static class Constants
    {
        public static readonly string IconDir = "Icons";

        public static readonly string ViewsDir = "Views";
        public static string MakePathInViews(string path)
        {
            return "/" + ViewsDir + "/" + path;
        }

        /// <summary>
        /// The number of stories that are displayed for a ContainedItem
        /// is set server side. (It displays however many results the View returns.)
        /// </summary>
    }
}
