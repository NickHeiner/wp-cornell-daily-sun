using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace CornellSunNewsreader
{
    public static class ExtensionMethods
    {
        public static String Remove(this string str, string toRemove)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            return str.Replace(toRemove, string.Empty);
        }

        public static void AddAll<T>(this ICollection<T> dest, ICollection<T> source)
        {
            if (dest == null)
            {
                throw new ArgumentNullException("dest");
            }

            foreach (T t in source)
            {
                dest.Add(t);
            }
        }

        public static String AsString(this JToken jToken)
        {
            string stringRep = jToken.ToString();
            Debug.Assert(stringRep[0] == '"' && stringRep[stringRep.Length - 1] == '"');
            return stringRep.Substring(1, stringRep.Length - 2);
        }
    }
}
