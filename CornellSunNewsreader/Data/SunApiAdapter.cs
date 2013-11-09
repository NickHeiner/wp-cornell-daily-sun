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

        internal static IList<Section> SectionsOfApiResponse(string p)
        {
            JObject jsonData = JObject.Parse(p);
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize<List<Section>>(new JTokenReader(jsonData[SECTIONS_JSON_KEY]));
        }

        internal static Uri StoryUriOfNid(int nid)
        {
            return new Uri(CornellSunRootUrl + "node/" + nid, UriKind.Absolute);
        }
    }
}
