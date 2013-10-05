using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CornellSunNewsreader.Models
{
    public interface NavigableItem
    {
        Uri Page { get; }
    }
}
