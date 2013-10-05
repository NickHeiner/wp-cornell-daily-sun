using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CornellSunNewsreader.Models;

namespace CornellSunNewsreader
{
    // not sure what the best design is here
    // Favorite seems like overkill/too much copying
    // but the favorite items do need to display correctly
    public class Favorite
    {
        public NavigableItem Item { get; set; }
        public string Title { get; set; }
        public string Teaser { get; set; }

        public string Name { get { return Title; } }
    }
}
