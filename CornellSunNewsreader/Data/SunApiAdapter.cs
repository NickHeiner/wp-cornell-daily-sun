using CornellSunNewsreader.Models;
using HtmlAgilityPack;
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

        // Ideally, this would just come via our API response.
        internal static readonly IList<int> SECTIONS_WHITELIST = new List<int>()
        {
            11, // News
            12, // Opinion
            16, // Sports (we like Sports, and we don't care who knows!)
            3,  // Arts
            13, // Science
            6,  // Dining
            // 9,  // Multimedia - taking that out for now because it requires more sophistication to properly display.
            4,  // Blogs
        };

        internal static IList<Section> SectionsOfApiResponse(string response)
        {
            return handleJsonWithSanitization(
                (json) =>
                {
                    JObject jsonData = JObject.Parse(json);
                    JsonSerializer serializer = new JsonSerializer();

                    return serializer.Deserialize<List<Section>>(new JTokenReader(jsonData[SECTIONS_JSON_KEY]));
                },
                null,
                response
            );
        }

        internal static IEnumerable<Story> StoriesOfApiResponse(string response)
        {
            return handleJsonWithSanitization(
                (json) =>
                {
                    JObject jsonResponse = JObject.Parse(json);
                    JsonSerializer deserializer = new JsonSerializer();

                    return from post in jsonResponse[POSTS_JSON_KEY]
                           select storyOfJson(deserializer, post);
                },
                null,
                response
            );
        }

        internal static string StoriesUrlOfSection(Section section)
        {
            // We could also pass a `count` query param here if we wanted.
            return CornellSunRootUrl + string.Format("blog/category/{0}/?json=get_posts&page={1}", section.Slug, section.LoadedPage);
        }

        private static Story storyOfJson(JsonSerializer serializer, JToken storyData)
        {
            SunSpecificStory sunSpecificStory = serializer.Deserialize<SunSpecificStory>(new JTokenReader(storyData));
            return sunSpecificStory.ToStoryJson().ToStory();
        }

        internal static IList<CommentJson> CommentJsonsOfComments(IEnumerable<Comment> comments)
        {
            return comments
                .Select(comment => CommentJsonsOfComments(comment.Children).Prepend(CommentJsonOfComment(comment)))
                .Flatten()
                .ToList();
        }

        private static CommentJson CommentJsonOfComment(Comment comment)
        {
            // Ugh this is gross. But we need to make the message content appear precisely 
            // as it would from the CornellSun API, so that the parsing code can be the same whether
            // we're reading from the cache or the API.
            var messageHtml = "<p>" + string.Join("</p><p>", comment.Paragraphs) + "</p>";

            return new CommentJson()
            {
                Name = comment.AuthorName,
                Message = messageHtml,
                CreatedAt = comment.Created.ToString(),
                Id = comment.Id,
                Parent = comment.ParentId
            };
        }

        internal static IList<Comment> CommentsOfCommentJsons(IEnumerable<CommentJson> commentJsons)
        {
            return commentsOfCommentJsons(commentJsons, null);
        }

        private static IList<Comment> commentsOfCommentJsons(IEnumerable<CommentJson> commentJsons, CommentJson parent)
        {
            return commentJsons
                .Where(commentJson => parent == null ? commentJson.IsRoot : commentJson.isChildOf(parent))

                // It would be nice to get some partial application here. F#, anyone?
                .Select(commentJson => commentOfCommentJson(commentJsons, commentJson))

                .OrderBy(comment => comment.Created)
                .Reverse()
                .ToList();
        }

        private static Comment commentOfCommentJson(IEnumerable<CommentJson> possibleChildren, CommentJson commentJson)
        {
            return new Comment(
                commentsOfCommentJsons(getChildrenOf(possibleChildren, commentJson), commentJson),
                SunResponseParser.GetBody(commentJson.Message),
                commentJson.Name,
                DateTime.Parse(commentJson.CreatedAt),
                commentJson.Id,
                commentJson.Parent
            );
        }

        private static IEnumerable<CommentJson> getChildrenOf(IEnumerable<CommentJson> allComments, CommentJson parent)
        {
            return allComments.Where(commentJson => commentJson.isChildOf(parent));
        }

        /// <summary>
        /// Amusingly, the Sun's JSON responses are malformed because they include this stupid comment at the end.
        /// I've pinged the Sun's web editor(s) and they are unresponsive, so I'll just hack around it.
        /// </summary>
        /// 
        internal static T handleJsonWithSanitization<T>(Func<string, T> handleJson, Func<string, T> onFail, string json)
        {
            try
            {
                // Try the json by itself first; we only
                // want to sanitize if there's a problem
                // in case sanitization has a risk of
                // screwing something else up.
                return handleJson(json);
            }
            catch (JsonReaderException)
            {
                try
                {
                    return handleJson(SunApiAdapter.SanitizedJson(json));
                }
                catch (JsonReaderException e)
                {
                    if (onFail != null)
                    {
                        return onFail(json);
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        internal static void handleJsonWithSanitization(Action<string> handleJson, Action onFail, string json)
        {
            try
            {
                // Try the json by itself first; we only
                // want to sanitize if there's a problem
                // in case sanitization has a risk of
                // screwing something else up.
                handleJson(json);
            }
            catch (JsonReaderException)
            {
                try
                {
                    handleJson(SunApiAdapter.SanitizedJson(json));
                }
                catch (JsonReaderException e)
                {
                    if (onFail != null)
                    {
                        onFail();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        private static readonly string JSON_COMMENT = "<!-- Page not cached by WP Super Cache. No closing HTML tag. Check your theme. -->";
        internal static string SanitizedJson(string rawJson)
        {
            return rawJson.EndsWith(JSON_COMMENT) ? rawJson.Substring(0, rawJson.Length - JSON_COMMENT.Length) : rawJson;
        }

    }
}
