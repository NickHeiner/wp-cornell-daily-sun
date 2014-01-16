﻿using CornellSunNewsreader.Data;
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

        public StoryJson ToStoryJson()
        {
            int vid = sections
                // we're basically pretending that sub-sections don't exist throughout this app
                .Where(section => !section.HasParent)
                .Select(section => section.Vid)
                // just to make sure we consistently get the same vid for the same story
                .OrderBy(id => id)
                .First();

            // TODO it may be necessary to use HttpUtility.HtmlDecode on some fields, or trim() them
            return new StoryJson()
            {
                Body = SunResponseParser.GetBody(Body),
                Title = SunResponseParser.GetTitle(Title),
                Teaser = SunResponseParser.GetTeaser(Teaser),
                Nid = Nid,
                Vid = vid,
                Date = Date,
                CornellSunOnlineUrl = CornellSunOnlineUrl
            };
        }
    }
}
