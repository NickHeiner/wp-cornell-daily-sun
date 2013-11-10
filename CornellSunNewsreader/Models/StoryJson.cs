using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CornellSunNewsreader.Models;
using CornellSunNewsreader.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CornellSunNewsreader
{
    /// <summary>
    /// Matches stories as found in Fixtures\sample-stories.json.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StoryJson
    {
        // It's better to use the JsonProperty attribute than forcing the property name to
        // match the json response. This allows us sto change the argument to JsonProperty
        // without modifying the rest of the code.
        [JsonProperty("content")]
        public string Body { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("excerpt")]
        public string Teaser { get; set; }

        [JsonProperty("id")]
        public int Nid { get; set; }

        // TODO I'm not totally sure what this field is used for.
        public int Vid { get; set; }

        // TODO: Right now this will pull the creation date, but we could also do interesting things with date_modified.
        [JsonProperty("date")]
        public string Date { get; set; }

        // Deprecated
        public string field_images_nid { get; set; }

        [JsonProperty("url")]
        public string CornellSunOnlineUrl { get; set; }

        [JsonProperty("categories")]
        public IList<Section> section { get; set; }

        public Story ToStory()
        {
            // TODO: Actually handle images
            Uri imageSrcUri = new Uri("http://cornellsun.com/wp-content/uploads/2013/11/Pg-20-Hockey-Joel-Lowry-by-Michelle-Feldman-Staff.jpg", UriKind.Absolute);
            return new Story(Body, Teaser, Title, imageSrcUri, Nid, Vid, Date, new Uri(CornellSunOnlineUrl, UriKind.Absolute));
        }

    }
}
