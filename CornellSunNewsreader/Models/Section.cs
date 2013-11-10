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
using System.Runtime.Serialization;
using System.Linq;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace CornellSunNewsreader.Models
{
    /// <summary>
    /// 
    /// The Section json that we get looks like:
    /// 
    /// {
    ///    id: 10,
    ///    slug: "music", // this is the param to the story endpoint to look up stories for this section
    ///    title: "Music",
    ///    description: "",
    ///    parent: 3, // id of parent section. if 0, this is a toplevel section
    ///    post_count: 43
    ///  }
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Section : NavigableItem
    {
        public static readonly string SectionsKey = "section";

        public bool Enabled { get; set; }

        [JsonProperty("title")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Vid { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("parent")]
        public int ParentId { get; set; }

        public bool HasParent
        {
            get
            {
                return ParentId != 0;
            }
        }

        public Uri Page
        {
            get
            {
                return new Uri(Constants.MakePathInViews(String.Format("sections.xaml?{0}={1}", SectionsKey, Vid)), UriKind.Relative);
            }
        }

        /// <summary>
        /// The largest value of the currently loaded page of data (as in "?page=LoadedPage" when querying Drupal).
        /// This is cached because figuring it out by querying the server is a bitch.
        /// </summary>
        public int LoadedPage { get; set; }

        public Section(string name, int tid)
        {
            Name = name;
            Vid = tid;

            // Nothing has been loaded yet
            LoadedPage = -1;
        }

        public override string ToString()
        {
            return String.Format("Section: name={0}, vid={1}", Name, Vid);
        }

        public override bool Equals(object obj)
        {
            return obj is Section && (obj as Section).Vid == Vid;
        }

        public override int GetHashCode()
        {
            return Vid;
        }
    }
}
