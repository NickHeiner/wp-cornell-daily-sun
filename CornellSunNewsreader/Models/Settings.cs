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
using CornellSunNewsreader.Data;

namespace CornellSunNewsreader.Models
{
    public class Settings
    {
        private static Settings _instance;
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    Init();
                }
                return _instance;
            }
        }
        public static void Init()
        {
            _instance = Storage.ReadSettings();
        }
        public static void Save()
        {
            Storage.WriteSettings(Instance);
        }

        public static Settings Create()
        {
            return new Settings() { FontSize = (double)Application.Current.Resources["PhoneFontSizeNormal"] };
        }

        /// <summary>
        /// This only exists for deserialization. Aside from that, don't use - this class is meant to be a singleton.
        /// </summary>
        public Settings() { }

        public double FontSize { get; set; }
    }
}
