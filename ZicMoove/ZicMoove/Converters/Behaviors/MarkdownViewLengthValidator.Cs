using ZicMoove.Views;
using Xamarin.Forms;

namespace ZicMoove.Behaviors
{
    public class MarkdownViewLengthValidator : Behavior<MarkdownView>
    {
        public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create("MaxLength", typeof(int), typeof(MarkdownViewLengthValidator), 0);
        public static readonly BindableProperty MinLengthProperty = BindableProperty.Create("MinLength", typeof(int), typeof(MarkdownViewLengthValidator), 0);
        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }
        public int MinLength
        {
            get { return (int)GetValue(MinLengthProperty); }
            set { SetValue(MinLengthProperty, value); }
        }
        protected override void OnAttachedTo(MarkdownView bindable)
        {
            bindable.Modified += bindable_TextChanged;
        }

        public bool IsValid { get; set; }

        private void bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsValid = e.NewTextValue == null ? false : (e.NewTextValue.Length >= MinLength && e.NewTextValue.Length <= MaxLength);
            if (!IsValid) if (e.NewTextValue != null) if (e.NewTextValue.Length > MaxLength)
                        ((Editor)sender).Text = e.NewTextValue.Substring(0, MaxLength);
        }

        protected override void OnDetachingFrom(MarkdownView bindable)
        {
            bindable.Modified -= bindable_TextChanged;
        }
    }
}
