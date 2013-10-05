using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Phone.Tasks;
using System.ComponentModel;

namespace CornellSunNewsreader.Views
{
    // todo: consider making email addresses also clickable

    // todo: use http://www.jeff.wilcox.name/2010/12/updated-phone-hyperlink-button/ instead?
    public partial class HyperlinkTextBlock : UserControl
    {
        private static readonly Regex EMAIL_REGEX = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

        // URL requires "http://" to start. Hopefully this won't cause problems.
        private static readonly Regex A_HREF_OR_EMAIL_REGEX = new Regex(@"(<a\s+href=""(?<url>http://[^""]*)""[^>]*>(?<text>[^(</>)]*)</a>|" + EMAIL_REGEX.ToString() + ")");

        #region SuffixProperty
        /// <summary>
        /// Suffix is some text that will be appended to the control. (Used by comments for "Likes").
        /// </summary>
        public static readonly DependencyProperty SuffixProperty = DependencyProperty.RegisterAttached(
            "Suffix",
            typeof(string),
            typeof(HyperlinkTextBlock),
            new PropertyMetadata(null, new PropertyChangedCallback(onSuffixChanged)));

        public string Suffix
        {
            get { return (string)GetValue(SuffixProperty); }
            set { SetValue(SuffixProperty, value); }
        }

        /// <summary>
        /// Add a subtly-formatted paragraph at the end of the main paragraph. Does not automatically pick up hyperlinks.
        /// </summary>
        /// <param name="dependObj"></param>
        /// <param name="e"></param>
        private static void onSuffixChanged(DependencyObject dependObj, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox richText = ((HyperlinkTextBlock)dependObj).LayoutRoot;
            Paragraph suffixParagraph = new Paragraph();
            suffixParagraph.Inlines.Add(new Run()
            {
                Text = (string)e.NewValue,
                FontSize = (double)App.Current.Resources["PhoneFontSizeNormal"],
                Foreground = (Brush)App.Current.Resources["PhoneSubtleBrush"]
            });

            richText.Blocks.Add(suffixParagraph);
        }

        #endregion

        #region TextProperty
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(HyperlinkTextBlock),
            new PropertyMetadata(null, new PropertyChangedCallback(onTextChanged)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Formats a single paragraph in this control.
        /// </summary>
        /// <param name="dependObj"></param>
        /// <param name="e"></param>
        private static void onTextChanged(DependencyObject dependObj, DependencyPropertyChangedEventArgs e)
        {
            Paragraph basePara = ((HyperlinkTextBlock)dependObj).BaseParagraph;

            string newText = ((string)e.NewValue);
            int textLowerBoundIndex = 0;

            Match match = A_HREF_OR_EMAIL_REGEX.Match(newText);
            while (match.Success)
            {
                Group entireLink = match.Groups[0];

                Debug.Assert(entireLink.Index > textLowerBoundIndex);

                basePara.Inlines.Add(new Run() { Text = newText.Substring(textLowerBoundIndex, entireLink.Index - textLowerBoundIndex) });

                Hyperlink href = new Hyperlink();

                if (EMAIL_REGEX.IsMatch(entireLink.Value))
                {
                    
                    GestureService.GetGestureListener(href);
                    href.Click += (s, r) =>
                    {
                        MessageBox.Show("href clicked");
                        new EmailComposeTask() { To = entireLink.Value }.Show();

                    };
                    href.Inlines.Add(new Run() { Text = entireLink.Value });
                }
                else
                {
                    href.Inlines.Add(new Run() { Text = match.Groups["text"].Value });
                    href.NavigateUri = new Uri(match.Groups["url"].Value, UriKind.Absolute);
                    href.TargetName = "_blank";
                }


                basePara.Inlines.Add(href);

                textLowerBoundIndex = entireLink.Index + entireLink.Length;

                newText = newText.Substring(textLowerBoundIndex);
                textLowerBoundIndex = 0;
                match = A_HREF_OR_EMAIL_REGEX.Match(newText);
            }

            basePara.Inlines.Add(new Run() { Text = newText.Substring(textLowerBoundIndex) });
        }

        #endregion

        #region HyperlinkFontSizeProperty
        public static readonly DependencyProperty HyperlinkFontSizeProperty = DependencyProperty.Register(
            "HyperlinkFontSize",
            typeof(double),
            typeof(HyperlinkTextBlock),
            new PropertyMetadata(20.0, new PropertyChangedCallback(onHyperlinkFontSizeChanged)));

        public double HyperlinkFontSize
        {
            get { return (double)GetValue(HyperlinkFontSizeProperty); }
            set { SetValue(HyperlinkFontSizeProperty, value); }
        }

        private static void onHyperlinkFontSizeChanged(DependencyObject dependObj, DependencyPropertyChangedEventArgs e)
        {
            ((HyperlinkTextBlock)dependObj).BaseParagraph.FontSize = (double)e.NewValue;
        }
        #endregion


        public HyperlinkTextBlock()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }
    }
}
