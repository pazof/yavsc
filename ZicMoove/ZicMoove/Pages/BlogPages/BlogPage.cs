using ZicMoove.Model.Blog;

using Xamarin.Forms;

namespace ZicMoove.Pages.BlogPages
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
            _sourceTitle.BaseUrl = _source.BaseUrl = Constants.YavscHomeUrl;
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
                _sourceTitle.Html = _md.Transform(blog.Title);
                _source.Html = _md.Transform(blog.Content);
            }
        }
    }
}
