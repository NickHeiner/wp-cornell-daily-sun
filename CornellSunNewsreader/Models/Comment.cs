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
using CornellSunNewsreader.Data;
using System.Collections.Generic;

namespace CornellSunNewsreader.Models
{
    // TODO: handle comment threads
    public class Comment
    {
        public static string NO_AVATAR = "http://mediacdn.disqus.com/1316029528/images/noavatar32.png";

        private IList<Comment> _children;
        public IEnumerable<Comment> Children
        {
            get
            {
                return _children;
            }
        }

        public IList<string> Paragraphs { get; private set; }
        public string AuthorName { get; private set; }
        public Uri AuthorAvatar { get; private set; }

        private DateTime _created;
        public DateTime Created
        {
            get
            {
                // If we give a date in the future to the converter, it will crash
                return _created > DateTime.Now ? DateTime.Now : _created;
            }
        }

        // TODO none of these are returned by the new API. Let's get rid of them.
        public Visibility AvatarVisiblity { get { return Visibility.Collapsed; } }
        public string LikesText { get { return Likes > 0 ? string.Format("({0} likes)", Likes) : string.Empty; } }
        public int Likes { get; private set; }

        /// <summary>
        /// These values are only used for re-serialization (for caching). We could dynamically re-assign ids
        /// at re-serialization time, but that seems more complicated than just not forgetting about them in the first place.
        /// </summary>
        public int Id { get; private set; }
        public int ParentId { get; private set; }

        public Comment(IList<Comment> children, IList<string> paragraphs, string authorName, DateTime created, int id, int parentId)
        {
            _children = children;
            Paragraphs = paragraphs;
            AuthorName = authorName;
            _created = created;

            Id = id;
            ParentId = parentId;
        }
    }
}
