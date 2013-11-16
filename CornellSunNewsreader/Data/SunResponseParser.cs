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
    }
}
