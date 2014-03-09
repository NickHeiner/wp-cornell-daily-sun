using CornellSunNewsreader.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            return HttpUtility.HtmlDecode(
                doc
                .DocumentNode
                .Elements("p")
                .First()
                .InnerText
            );
        }

        private static IList<string> getTextFromTag(HtmlDocument doc, string tagname)
        {
            return getTextFromTag(doc, tagname, null);
        }

        private static bool doesNotHaveClass(HtmlNode elem, string klass)
        {
            return elem != null && (!elem.Attributes.Contains("class") || !elem.Attributes["class"].Value.Contains(klass));
        }

        private static IList<string> getTextFromTag(HtmlDocument doc, string tagname, string classToIgnore)
        {
            return doc
                .DocumentNode
                .Elements(tagname)
                .Where(elem => classToIgnore == null || 
                    (doesNotHaveClass(elem, classToIgnore) && 
                        elem.ChildNodes.All(child => doesNotHaveClass(child, classToIgnore))
                    )
                )
                .Select(elem => elem.InnerText)
                .Select(HttpUtility.HtmlDecode)
                .Where(str => str != "")
                .ToList();
        }

        /// <summary>
        /// We get the story as an html formatted blob. It's kind of a bummer to recreate a rendering engine in this app.
        /// Ideally, we would just use a WebBrowser control to display the html. But I'm not sure how to do that and still
        /// maintain the look-and-feel of the surrounding app. Figuring out how to translate App resource dictionary content
        /// into CSS sounds like a fucking pain. So I'll just settle for a few hacks here and there that will hopefully 
        /// be good enough. My first approach was to just pull out all the p tags, but then some Opinion articles have divs 
        /// intead of p's. I could easily see this approach becoming a patchwork mess of one-off rules meant to catch various
        /// cases that the API may return, whilst being utterly brittle against new changes. Woo.
        /// </summary>
        internal static IList<string> GetBody(string bodyHtml)
        {
            if (bodyHtml == "")
            {
                return new List<string>();
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(bodyHtml);

            return doc.DocumentNode.Elements("p").Count() > 0 ?
                getTextFromTag(doc, "p", "simplePullQuote") :
                getTextFromTag(doc, "div");
        }

        internal static string GetTitle(string title)
        {
            return HttpUtility.HtmlDecode(title);
        }
    }
}
