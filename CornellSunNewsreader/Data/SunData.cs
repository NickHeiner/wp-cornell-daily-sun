using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using Microsoft.Phone.Shell;
using CornellSunNewsreader.Models;
using System.Collections.Specialized;
using CornellSunNewsreader.ViewModels;

namespace CornellSunNewsreader.Data
{
    // TODO:
    // if the app downloads something, like a section
    // then CornellSun removes that section
    // will the app handle that correctly?

    // This could be totally buggy, since I changed a lot of it without being able to test much
    public static class SunData
    {
        /// <summary>
        /// Append a story's nid to this to get its comments
        /// TODO: This may be unneeded - the json response now includes this info
        /// </summary>
        private static readonly string DisqusQuery = "http://disqus.com/api/3.0/threads/listPosts.json?" +
            "api_key=2nOgHjGpcpCtC8udyc4qlik8pGhvanbyppi4YUNttYrOdwoRFvgScXylXfBtBbN2&forum=thecornelldailysun&thread:ident=node/";

        public static event EventHandler SectionsAcquired;

        /// <summary>
        /// When a download starts or finishes. Will not pass meaningful arguments.
        /// </summary>
        public static event EventHandler DownloadStatusChanged;

        public static IEnumerable<Section> GetSections()
        {
            return getSectionStories().Keys;
        }

        public static IEnumerable<Story> GetAllStories()
        {
            return getSectionStories().SelectMany(kv => kv.Value);
        }

        private static IDictionary<Section, ObservableCollection<Story>> getSectionStories()
        {
            if (_sectionStories == null)
            {
                _sectionStories = new Dictionary<Section, ObservableCollection<Story>>();
                // possible optimization: don't block on this file reading while the page is being loaded
                if (Storage.ReadSections(_sectionStories))
                {
                    Storage.ReadFromSectionStoriesStorage(_sectionStories);
                }
                DownloadSections();
            }

            return _sectionStories;
        }

        private static ObservableCollection<NavigableItem> _favorites;

        /// <summary>
        /// Collection of favorite stories and sections designated by the user.
        /// </summary>
        internal static ObservableCollection<NavigableItem> GetFavorites()
        {
            if (_favorites == null)
            {
                _favorites = Storage.ReadFavorites();
                Debug.Assert(_favorites != null);
            }
            return _favorites;
        }

        private static void DownloadSections()
        {
            downloadData(SunApiAdapter.SectionsUrl, data_SectionDownloadCompleted);
        }

        static SunData()
        {
            PhoneApplicationService.Current.Closing += new EventHandler<ClosingEventArgs>(App_Ending);
            PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(App_Ending);
        }

        static void App_Ending(object sender, EventArgs e)
        {
            Storage.WriteUserFavorites(_favorites);
            Storage.WriteSectionStories(_sectionStories);
            Storage.WriteSections(GetSections());
        }

        private static void safeFireDownloadStatusChanged()
        {
            if (DownloadStatusChanged != null)
            {
                DownloadStatusChanged(null, new EventArgs());
            }
        }

        static void downloadData(string uri, Action<object, DownloadStringCompletedEventArgs> onComplete)
        {
            Debug.WriteLine("Downloading: " + uri);

            // TODO make WebClient static?
            // does WebClient automatically implement some form of caching?
            // http://stackoverflow.com/questions/5173052/how-do-you-disable-caching-with-webclient-and-windows-phone-7
            WebClient data = new WebClient();
            data.DownloadStringCompleted += new DownloadStringCompletedEventHandler((sender, e) =>
            {
                onComplete(sender, e);
                safeFireDownloadStatusChanged();
            });
            safeFireDownloadStatusChanged();
            data.Headers[HttpRequestHeader.Referer] = "http://cornellsun.com";
            data.DownloadStringAsync(new Uri(uri));
        }

        public static event DownloadStringCompletedEventHandler DownloadFailed;

        /// <summary>
        /// Callback for when the sections are downloaded from the Sun.
        /// Doesn't have anything to do with the stories themselves.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void data_SectionDownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                DownloadFailed(sender, e);
                return;
            }

            var sections = SunApiAdapter.SectionsOfApiResponse(e.Result)
                // Just ignore subsections. Maybe later this app will handle them in some 
                // interesting way, but for now let's simplify.
                .Where(section => !section.HasParent);

            foreach (Section section in sections)
            {
                _sectionStories[section] = new ObservableCollection<Story>();
            }

            if (SectionsAcquired != null)
            {
                SectionsAcquired(null, null);
            }
        }

        // ReadOnlyObservableCollection?
        static IDictionary<Section, ObservableCollection<Story>> _sectionStories;

        private static IList<Section> _downloadedSectionStories = new List<Section>();
        private static IList<Section> _currentlyDownloadingSectionStories = new List<Section>();

        public static bool AnySectionCurrentlyDownloading
        {
            get
            {
#if DEBUG
                if (_downloadedSectionStories.Count == getSectionStories().Count)
                {
                    Debug.Assert(_currentlyDownloadingSectionStories.Count == 0);
                }
#endif
                return _currentlyDownloadingSectionStories.Count != 0;
            }
        }

        internal static ObservableCollection<Story> GetStories(Section section)
        {
            return GetStories(section, InsertStoriesAt.Beginning);
        }

        internal static ObservableCollection<Story> GetStories(Section section, InsertStoriesAt insertStoriesAt)
        {
            if (!_downloadedSectionStories.Contains(section) && !_currentlyDownloadingSectionStories.Contains(section))
            {
                _currentlyDownloadingSectionStories.Add(section);
                downloadData(SunApiAdapter.StoriesUrlOfSection(section), (sender, e) =>
                {
                    _downloadedSectionStories.Add(section);
                    _currentlyDownloadingSectionStories.Remove(section);

                    data_SectionStoriesDownloadCompleted(sender, e, getSectionStories()[section], section, insertStoriesAt);
                    section.LoadedPage++;
                });
            }

            return getSectionStories()[section];
        }

        internal enum InsertStoriesAt { Beginning, End }

        static void data_SectionStoriesDownloadCompleted(object sender,
                                                        DownloadStringCompletedEventArgs downloadCompletedEvent,
                                                        ObservableCollection<Story> storyCollection,
                                                        Section section,
                                                        InsertStoriesAt insertStoriesAt)
        {
            if (downloadCompletedEvent.Error != null)
            {
                DownloadFailed(sender, downloadCompletedEvent);
                return;
            }

            try
            {
                var stories = SunApiAdapter.StoriesOfApiResponse(downloadCompletedEvent.Result);
                var existingNids = storyCollection.Select(s => s.Nid);
                int countStoriesAdded = insertStoriesAt == InsertStoriesAt.Beginning ? 0 : storyCollection.Count;
                foreach (Story story in stories.Where(article => !existingNids.Contains(article.Nid)))
                {
                    story.Vid = section.Vid;

                    // cached stories will already be present
                    // so we want to add the new stuff to the beginning
                    // Do we need to sort anyway, or will this always work?
                    storyCollection.Insert(countStoriesAdded++, story);
                }
            }
            catch (JsonReaderException)
            {
                Debug.Assert(false, "Why couldn't the JSON be parsed?");
                DownloadFailed(sender, downloadCompletedEvent);
            }
        }

        internal static Story getStory(int nid)
        {
            // if the story download hasn't happened yet,
            // and there's nothing in file storage,
            // this will fail
            var sectionStories = getSectionStories();
            
            var storiesWithNid = from sectionStory in sectionStories
                          from story in sectionStory.Value
                          where story.Nid == nid
                          select story;

            Debug.Assert(storiesWithNid.Count() < 2, "Duplicate stories found.");
            Debug.Assert(storiesWithNid.Count() > 0, "No story found.");

            return storiesWithNid.Single();
        }

        internal static Section GetSection(int vid)
        {
            return getSectionStories().Where(keyValue => keyValue.Key.Vid == vid).Single().Key;
        }

        private class CommentTuple
        {
            public CommentJson CommentJson { get; set; }
            public String AuthorName { get; set; }
            public Uri AuthorAvatar { get; set; }
            private IList<CommentTuple> _children;

            public void SetChild(List<CommentTuple> children)
            {
                _children = children;
            }

            public Comment AsComment()
            {
                return new Comment(CommentJson, AuthorName, AuthorAvatar, _children.Select(commentTuple => commentTuple.AsComment()).ToList());
            }
        }

        // TODO: cache this
        internal static ObservableCollection<Comment> GetComments(int nid, Action onFailure)
        {
            ObservableCollection<Comment> commentCollection = new ObservableCollection<Comment>();
            WebClient webClient = new WebClient();

            // http://stackoverflow.com/questions/7216738/your-api-key-is-not-valid-on-this-domain-when-calling-disqus-from-wp7
            webClient.Headers[HttpRequestHeader.Referer] = "http://disqus.com";

            Uri queryUri = new Uri(DisqusQuery + nid, UriKind.Absolute);
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler((sender, downloadEvent) =>
            {
                if (downloadEvent.Error != null)
                {
                    Debug.WriteLine(String.Format("Download failed for comments ({0}): {1}", queryUri, downloadEvent.Error));
                    // use onFailure() instead of DownloadFailed.
                    // Those listening for DownloadFailed may not realize that the comments were the failure, as opposed to sections.
                    // Just take a onFailure callback.
                    // Really, this whole "load content and display it or an error message" could have a better generic solution.
                    onFailure();
                    return;
                }

                try
                {
                    // TODO: is this sorted by date?
                    JObject commentData = JObject.Parse(downloadEvent.Result);

                    JsonSerializer deserializer = new JsonSerializer();

                    Debug.Assert(commentData["code"].ToString() == "0", "API returned error code: " + commentData["code"]);

                    List<CommentTuple> commentTuples =
                                   (from comment in commentData["response"]
                                    where checkCondition(comment["isApproved"]) && !checkCondition(comment["isDeleted"]) && !checkCondition(comment["isSpam"])
                                    let commentJson = commentJsonOfJson(deserializer, comment)
                                    let avatarUri = comment["author"]["avatar"]["permalink"].AsString()
                                    select new CommentTuple()
                                    {
                                        CommentJson = commentJson,
                                        AuthorName = comment["author"]["name"].AsString(),
                                        AuthorAvatar = avatarUri.Contains(Comment.NO_AVATAR) ? null : new Uri(avatarUri, UriKind.Absolute)
                                    }).ToList();

                    foreach (CommentTuple parent in commentTuples)
                    {
                        parent.SetChild(commentTuples.Where(tuple => tuple.CommentJson.Parent == parent.CommentJson.Id).ToList());
                    }

                    commentCollection.AddAll((from tuple in commentTuples
                                              where tuple.CommentJson.Parent == null
                                              select tuple.AsComment()).ToList());
                }
                catch (JsonReaderException)
                {
                    Debug.Assert(false, "Why couldn't the JSON be parsed?");
                    DownloadFailed(sender, downloadEvent);
                }
            });

            webClient.DownloadStringAsync(queryUri);

            return commentCollection;
        }

        private static CommentJson commentJsonOfJson(JsonSerializer deserializer, JToken data)
        {
            return deserializer.Deserialize<CommentJson>(new JTokenReader(data));
        }

        private static bool checkCondition(JToken flag)
        {
            return bool.Parse(flag.ToString());
        }

        // Find the first page of story data that contains stories we don't already have,
        // then load that page.
        //
        // This could possibly be done much more simply on the server side
        // Just have a service that returns the page number we need
        // However, I'd rather minimize the dependency on Drupal as much as possible
        // and PHP sucks fat donkey dick.
        internal static void GetMoreStories(Section section)
        {
            // TODO: this seems to fire off two queries for the first page it checks
            // TODO: could the stories/nid view be used to save time when loading the 0th page?
            //       Perhaps it's not necessary to make that big call at all, if we're already fully cached.
            string queryUrl = SunApiAdapter.StoriesUrlOfSection(section);
            downloadData(queryUrl, (sender, e) =>
            {
                if (e.Error != null)
                {
                    Debug.WriteLine("Error getting story nids page: " + queryUrl);
                    return;
                }

                var stories = SunApiAdapter.StoriesOfApiResponse(e.Result);
                var nids = stories.Select(story => story.Nid);

                // if all the nids on this page are already in memory
                IEnumerable<int> existingNids = _sectionStories[section].Select(story => story.Nid);
                if (nids.Intersect(existingNids).Count() == nids.Count())
                {
                    // try the next page
                    section.LoadedPage++;
                    GetMoreStories(section);
                    return;
                }

                // We want more data for `section`,
                // so remove it from the collection of sections
                // we've already fully loaded.
                _downloadedSectionStories.Remove(section);
                GetStories(section, InsertStoriesAt.End);
            });
        }
    }
}
