using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CornellSunNewsreader.Models;
using Newtonsoft.Json;

namespace CornellSunNewsreader.Data
{
    /// <summary>
    /// The response will be part of the story json, and will look like this:
    /// 
    /// {
    ///   id: 7657,
    ///   name: "mxm123",
    ///   url: "",
    ///   date: "2014-01-23 11:51:00",
    ///   content: "<p>I have no idea what I'm talking about, but that sure won't stop me from spouting off emotion-based nonsense!</p>",
    ///   parent: 7656
    /// },
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CommentJson
    {
        // These properties must have a public set
        // and they must match the keys of the JSON object

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("content")]
        public string Message { get; set; }

        [JsonProperty("date")]
        public string CreatedAt { get; set; }

        [JsonProperty("parent")]
        public int Parent { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        // We also get a "url" field but I haven't seen it used.

        public bool HasParent
        {
            get
            {
                // 0 is the magic value in the Sun Api for having no parent.
                return Parent == 0;
            }
        }
    }
}
