using CornellSunNewsreader.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CornellSunNewsreader.Models
{
    /// <summary>
    /// Representation of the story response that comes from the Sun Api
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SunSpecificStory
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

        // TODO: Right now this will pull the creation date, but we could also do interesting things with date_modified.
        [JsonProperty("date")]
        public string Date { get; set; }

        // Deprecated
        public string field_images_nid { get; set; }

        [JsonProperty("url")]
        public string CornellSunOnlineUrl { get; set; }

        [JsonProperty("categories")]
        public IList<Section> sections { get; set; }

        [JsonProperty("attachments")]
        public IList<Attachment> attachments { get; set; }

        [JsonProperty("comments")]
        public IList<CommentJson> comments { get; set; }

        /// <summary>
        /// Sometimes, we'll request the stories for a section, with a url like 
        /// 
        ///     http://cornellsun.com/blog/category/sports/?json=get_recent_posts
        ///     
        /// but we will get a story response that has the categories entry:
        /// 
        ///     categories: [{
        ///        id: 24,
        ///        slug: "columns-sports",
        ///        title: "Columns",
        ///        description: "",
        ///        parent: 16,
        ///        post_count: 23
        ///     }]
        ///     
        /// We only want to consider the parent section. Thus, we will just pretend that that is the 
        /// only section this story belongs to.
        /// </summary>
        /// <returns></returns>
        private bool hasValidSection()
        {
            return sections.Where(SunData.ShouldAcceptSection).Count() > 0;
        }

        public StoryJson ToStoryJson()
        {
            IEnumerable<int> vids;

            if (hasValidSection())
            {
                vids = sections
                    // fuck me if this method is too restrictive and we don't find a section for this story.
                .Where(SunData.ShouldAcceptSection)
                .Select(section => section.Vid);
            }
            else
            {
                vids = sections
                    .Select(section => section.ParentId);
            }

            int vid = 
                vids
                // just to make sure we consistently get the same vid for the same story
                .OrderBy(id => id)
                .First();

            string imageSrc = null;

            if (attachments != null && attachments.Count > 0)
            {
                var images = attachments.Where(a => a.mimeType.StartsWith("image"));
                if (images.Count() > 0) 
                {
                    imageSrc = images.First().url;
                }
            }

            // TODO it may be necessary to use HttpUtility.HtmlDecode on some fields, or trim() them
            return new StoryJson()
            {
                Body = SunResponseParser.GetBody(Body),
                Title = SunResponseParser.GetTitle(Title),
                Teaser = SunResponseParser.GetTeaser(Teaser),
                Nid = Nid,
                Vid = vid,
                Date = Date,
                CornellSunOnlineUrl = CornellSunOnlineUrl,
                imageSrc = imageSrc,
                Comments = comments
            };
        }
    }

    /// <summary>
    /// Represents an object in the "attachments" array.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Attachment
    {
        [JsonProperty("url")]
        public string url { get; set; }

        [JsonProperty("mime_type")]
        public string mimeType { get; set; }
    }
}
