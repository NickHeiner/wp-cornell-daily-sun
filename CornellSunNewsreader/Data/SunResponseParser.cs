using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CornellSunNewsreader.Data
{
    /// <summary>
    /// Contains all the logic specific to how the Sun formats its responses. 
    /// Anything that could change about how that interface should be encapsulated here.
    /// </summary>
    class SunResponseParser
    {
        internal static string GetTeaser(string teaserHtml)
        {
            if (teaserHtml == "")
            {
                return "";
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(teaserHtml);
            return doc.DocumentNode.Elements("p").First().InnerText;
        }

        internal static IList<string> GetBody(string bodyHtml)
        {
            if (bodyHtml == "")
            {
                return new List<string>();
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(bodyHtml);

            // TODO There is more html in the story, like <a>, <img>, and <em>, and this will ignore all that.
            // It would be nice to handle that appropriately.
            return doc.DocumentNode.Elements("p").Select(p => p.InnerText).ToList();
        }
    }
}
