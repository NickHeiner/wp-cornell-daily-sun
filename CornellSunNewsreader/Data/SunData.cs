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
        public static event EventHandler SectionsAcquired;

        /// <summary>
        /// When a download starts or finishes. Will not pass meaningful arguments.
        /// </summary>
        public static event EventHandler DownloadStatusChanged;

        public static IEnumerable<Section> GetSections()
        {
            return getSectionStories()
                .Keys
                .OrderBy(section => SunApiAdapter.SECTIONS_WHITELIST.IndexOf(section.Vid));
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

        internal static bool ShouldAcceptSection(Section section)
        {
            // Just ignore subsections. Maybe later this app will handle them in some 
            // interesting way, but for now let's simplify.
            return !section.HasParent &&

                // In practice, this may be redundant with the previous check.
                SunApiAdapter.SECTIONS_WHITELIST.Contains(section.Vid);
        }

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

            var sections = SunApiAdapter.SectionsOfApiResponse(e.Result).Where(ShouldAcceptSection);

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

        // Find the first page of story data that contains stories we don't already have,
        // then load that page.
        //
        // This could possibly be done much more simply on the server side
        // Just have a service that returns the page number we need
        // However, I'd rather minimize the serverside dependency, since it can break without notice.
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
