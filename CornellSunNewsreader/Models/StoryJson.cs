using System;
using System.Linq;
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
    /// Used for caching.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StoryJson
    {
        // It's better to use the JsonProperty attribute than forcing the property name to
        // match the json response. This allows us sto change the argument to JsonProperty
        // without modifying the rest of the code.
        [JsonProperty("content")]
        public IList<string> Body { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("excerpt")]
        public string Teaser { get; set; }

        [JsonProperty("id")]
        public int Nid { get; set; }

        /// <summary>
        /// Originally, all we got from the API was the Vid, which is the section id of the story.
        /// This is used to identify which section the story belongs to during caching.
        /// Now the API gives us much more details about sections, which are in the sections property of this class,
        /// and it's possible for a single story to be classified in several sections. Given this schema change,
        /// there may be an more elegant solution for how this app maintains that state, but for now I'm just going
        /// to grab an arbitrary section id and call it good.
        /// 
        /// It's also a bummer that StoryJson is a slightly different format from what we get from the Cornell Sun api,
        /// so when we deserialize from the web json, it's different from deserializing from the cache. For instance, 
        /// this JsonProperty attribute it only used when deserializing from the cache - when it's from the web, we pull
        /// in the categories property and determine the vid from that. It kinda sucks but I don't think it's worth 
        /// fixing at the moment.
        /// </summary>
        [JsonProperty("Vid")]
        public int Vid { get; set; }

        // TODO: Right now this will pull the creation date, but we could also do interesting things with date_modified.
        [JsonProperty("date")]
        public string Date { get; set; }

        // Deprecated
        public string field_images_nid { get; set; }

        [JsonProperty("url")]
        public string CornellSunOnlineUrl { get; set; }

        [JsonProperty("imageSrc")]
        public string imageSrc { get; set; }

        [JsonProperty("comments")]
        public IList<CommentJson> Comments { get; set; }

        public Story ToStory()
        {
            Uri imageUri = null;

            if (imageSrc != null) 
            {
                imageUri = new Uri(imageSrc, UriKind.Absolute);
            }

            return new Story(
                Body, 
                Teaser, 
                Title, 
                imageUri, 
                Nid, 
                Vid, 
                Date, 
                new Uri(CornellSunOnlineUrl, UriKind.Absolute), 
                SunApiAdapter.CommentsOfCommentJsons(Comments)
            );
        }

    }

}
