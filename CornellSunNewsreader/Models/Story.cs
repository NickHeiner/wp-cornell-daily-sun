using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using CornellSunNewsreader;
using CornellSunNewsreader.Data;
using HtmlAgilityPack;

namespace CornellSunNewsreader.Models
{
    [JsonObject(MemberSerialization.OptOut)]   
    public class Story : NavigableItem
    {
        public IList<String> Body { get; private set; }
        public string Teaser { get; private set; }
        public string Title { get; private set; }
        public string Date { get; private set; }
        public Uri ImageSrc { get; private set; }
        public int Nid { get; private set; }
        public int Vid { get; set; }
        public Uri CornellSunOnlineUri { get; private set; }

        public Story(string body, string teaser, string title, Uri imageSrc, int nid, int vid, string date, Uri cornellSunOnlineUrl)
        {
            CornellSunOnlineUri = cornellSunOnlineUrl;

            // these newlines and spaces can break the formatting
            title = title.Remove("\n").Trim();

            body = HttpUtility.HtmlDecode(body).Trim();
            teaser = HttpUtility.HtmlDecode(teaser);
            title = HttpUtility.HtmlDecode(title);

            Body = SunResponseParser.GetBody(body);
            Teaser = SunResponseParser.GetTeaser(teaser);
            Title = title;
            ImageSrc = imageSrc;
            Nid = nid;
            Vid = vid;
            // Legacy: date may not exist in locally cached stories
            Date = date != null ? date.Trim() : "";
        }

        public StoryJson ToStoryJson()
        {
            string body = String.Join("\n", Body.ToArray<String>());
            string imageSrcStr = ImageSrc == null ? null : ImageSrc.AbsolutePath;

            return new StoryJson() { Title = Title, Body = body, field_images_nid = imageSrcStr, Nid = Nid, Teaser = Teaser, Vid = Vid, Date = Date };
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null || obj.GetType() != typeof(Story))
            {
                return false;
            }

            Story otherStory = (Story)obj;

            return Nid == otherStory.Nid;
        }

        public override int GetHashCode()
        {
            return Nid;
        }

        #region NavigableItem Members

        public Uri Page
        {
            get 
            {
                return new Uri(Constants.MakePathInViews("storyPage.xaml?nid=") + Nid, UriKind.Relative);
            }
        }

        #endregion
    }
}
