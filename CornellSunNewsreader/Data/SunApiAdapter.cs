using CornellSunNewsreader.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CornellSunNewsreader.Data
{
    /// <summary>
    /// Contains all the logic specific to a certain version of the Cornell Sun API. 
    /// Anything that could change about how that interface should be encapsulated here.
    /// </summary>
    internal static class SunApiAdapter
    {
        /// <summary>
        /// My own arbitrary api versioning scheme, following semver. (http://semver.org/)
        /// </summary>
        internal static readonly string API_VERSION = "2.0.0";

        internal static readonly string CornellSunRootUrl = "http://cornellsun.com/";
        internal static readonly string SectionsUrl = CornellSunRootUrl + "?json=get_category_index";

        private static readonly string SECTIONS_JSON_KEY = "categories";
        private static readonly string POSTS_JSON_KEY = "posts";

        internal static IList<Section> SectionsOfApiResponse(string response)
        {
            JObject jsonData = JObject.Parse(response);
            JsonSerializer serializer = new JsonSerializer();

            return serializer.Deserialize<List<Section>>(new JTokenReader(jsonData[SECTIONS_JSON_KEY]));
        }

        internal static IEnumerable<Story> StoriesOfApiResponse(string response)
        {
            JObject jsonResponse = JObject.Parse(response);
            JsonSerializer deserializer = new JsonSerializer();

            return from post in jsonResponse[POSTS_JSON_KEY]
                   select storyOfJson(deserializer, post);
        }

        internal static string StoriesUrlOfSection(Section section)
        {
            return CornellSunRootUrl + string.Format("blog/category/{0}/?json=get_recent_posts", section.Slug);
        }

        private static Story storyOfJson(JsonSerializer serializer, JToken storyData)
        {
            StoryJson storyJson = serializer.Deserialize<StoryJson>(new JTokenReader(storyData));
            return storyJson.ToStory();
        }
    }
}
