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
using CornellSunNewsreader.Models;
using CornellSunNewsreader.Data;

namespace CornellSunNewsreader
{
    public class StoryJson
    {
        // These properties must have a public set
        // and they must match the keys of the JSON object
        public string Body { get; set; }
        public string Title { get; set; }
        public string Teaser { get; set; }
        public int Nid { get; set; }
        public int Vid { get; set; }
        public string field_images_nid { get; set; }
        public string Date { get; set; }

        public Story ToStory()
        {
            // "field_images_nid":"http:\/\/cornellsun.com\/files\/images\/Pg-3-tcat---VGao-S.preview.jpg"
            Uri imageSrcUri = field_images_nid == null ? null : new Uri(field_images_nid, UriKind.Absolute);
            return new Story(Body, Teaser, Title, imageSrcUri, Nid, Vid, Date);
        }

    }
}
