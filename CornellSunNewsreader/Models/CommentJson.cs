using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CornellSunNewsreader.Models;

namespace CornellSunNewsreader.Data
{
    public class CommentJson
    {
        // These properties must have a public set
        // and they must match the keys of the JSON object
        public string Message { get; set; }
        public string CreatedAt { get; set; }
        public int Likes { get; set; }
        public int? Parent { get; set; }
        public int Id { get; set; }
    }
}
