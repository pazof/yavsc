using BookAStar.Model.Blog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;
using Yavsc.Models;

namespace BookAStar
{


    public class BlogPage : ContentPage
    {
        HtmlWebViewSource _source;
        HtmlWebViewSource _sourceTitle;
        MarkdownDeep.Markdown _md;
        WebView titleLabel;
        WebView contentView;

        public BlogPage()
        {
            _source = new HtmlWebViewSource();
            _sourceTitle = new HtmlWebViewSource();
            _md = new MarkdownDeep.Markdown();
            _sourceTitle.BaseUrl = _source.BaseUrl = MainSettings.YavscHomeUrl;
            _sourceTitle.Html = "Hello";
            titleLabel = new WebView { Source = _sourceTitle };
            contentView = new WebView { Source = _source };
           
            Content = new StackLayout
            {
                Children = {
                    titleLabel,
                   contentView
                }
            };
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            var blog = BindingContext as Blog;
            if (blog == null)
            {
                _sourceTitle.Html = _source.Html = "";
            }
            else
            {
                _sourceTitle.Html = _md.Transform(blog.bcontent);
                _source.Html = _md.Transform(blog.bcontent);
            }
        }
    }
}
