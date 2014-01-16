using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using CornellSunNewsreader.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Diagnostics;

namespace CornellSunNewsreader.Data
{
    /// <summary>
    /// TODO Implement versioning so a cache with old data won't cause a problem.
    /// </summary>
    public static class Storage
    {
        private static readonly string SettingsFile = "settings.json";
        private static readonly string StoriesFile = "stories.json";
        private static readonly string FavoriteSectionsFile = "favoriteSections.json";
        private static readonly string FavoriteStoriesFile = "favoriteStories.json";
        private static readonly string SectionsFile = "sections.json";

        public static Settings ReadSettings()
        {
            return readFromStorage(contents => JsonConvert.DeserializeObject<Settings>(contents), SettingsFile) ?? Settings.Create();
        }

        public static void WriteSettings(Settings settings)
        {
            writeData(SettingsFile, settings);
        }

        public static ObservableCollection<NavigableItem> ReadFavorites()
        {
            ObservableCollection<NavigableItem> results = new ObservableCollection<NavigableItem>();

            readFromStorage(contents =>
            {
                IList<Section> favoritesList = JsonConvert.DeserializeObject<IList<Section>>(contents);
                foreach (NavigableItem fav in favoritesList)
                {
                    results.Add(fav);
                }

                return results;

            }, FavoriteSectionsFile);

            readFromStorage(contents =>
            {
                IList<StoryJson> favoritesList = JsonConvert.DeserializeObject<IList<StoryJson>>(contents);
                foreach (NavigableItem fav in favoritesList.Select(storyJson => storyJson.ToStory()))
                {
                    results.Add(fav);
                }

                return results;

            }, FavoriteStoriesFile);

            return results;
        }

        internal static bool ReadSections(IDictionary<Section, ObservableCollection<Story>> _sectionStories)
        {
            IList<Section> sections = readFromStorage(contents => JsonConvert.DeserializeObject<IList<Section>>(contents), SectionsFile);

            if (sections != null)
            {
                foreach (Section section in sections.Where(sect => !_sectionStories.ContainsKey(sect)))
                {
                    _sectionStories[section] = new ObservableCollection<Story>();
                }
            }

            return sections != null;
        }

        private static T readFromStorage<T>(Func<string, T> deserialize, string filePath)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.FileExists(filePath))
                {
                    return default(T);
                }

                string contents;
                using (IsolatedStorageFileStream stream = storage.OpenFile(filePath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        contents = reader.ReadToEnd();
                    }
                }

                if (contents.Count() == 0)
                {
                    return default(T);
                }

                return deserialize(contents);
            }
        }

        public static IDictionary<Section, ObservableCollection<Story>> ReadFromSectionStoriesStorage(IDictionary<Section, ObservableCollection<Story>> result)
        {
            return readFromStorage<IDictionary<Section, ObservableCollection<Story>>>(contents =>
            {
                IList<StoryJson> storyJsons = JsonConvert.DeserializeObject<IList<StoryJson>>(contents);

                foreach (StoryJson storyJson in storyJsons)
                {
                    Section section = SunData.GetSection(storyJson.Vid);
                    if (!result.ContainsKey(section))
                    {
                        result[section] = new ObservableCollection<Story>();
                    }

                    result[section].Add(storyJson.ToStory());
                }

                return result;

            }, StoriesFile) ?? result;
        }

        private static void writeData<T>(String fileName, T toSerialize)
        {
            // we want to write even if toSerialize is empty
            // eg, if the user deletes all their favorites,
            // we want to record that they have no favorites.

            Debug.Assert(toSerialize != null, "Why are we trying to serialize something that's null?");

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = storage.OpenFile(fileName, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(JsonConvert.SerializeObject(toSerialize));
                    }
                }
            }
        }

        public static void WriteSections(IEnumerable<Section> sections)
        {
            writeData(SectionsFile, sections.ToList());
        }

        public static void WriteUserFavorites(ICollection<NavigableItem> favorites)
        {
            IList<Section> favoriteSections = favorites.Where(fav => fav is Section).Select(section => (Section)section).ToList();
            IList<StoryJson> favoriteStoryJsons = favorites.Where(fav => fav is Story).Select(story => ((Story)story).ToStoryJson()).ToList();

            writeData(FavoriteSectionsFile, favoriteSections);
            writeData(FavoriteStoriesFile, favoriteStoryJsons);
        }

        public static void WriteSectionStories(IDictionary<Section, ObservableCollection<Story>> sectionStories)
        {
            IList<StoryJson> serializeList = (from stories in sectionStories.Values
                                              from story in stories
                                              select story.ToStoryJson()).ToList();

            writeData(StoriesFile, serializeList);
        }
    }
}
