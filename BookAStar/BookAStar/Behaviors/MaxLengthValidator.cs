using Xamarin.Forms;

namespace BookAStar.Behaviors
{
    public class EditorMaxLengthValidator : Behavior<Editor>
    {
        public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create("MaxLength", typeof(int), typeof(MaxLengthValidator), 0);

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        protected override void OnAttachedTo(Editor bindable)
        {
            bindable.TextChanged += bindable_TextChanged;
        }

        public bool IsValid { get; set; }

        private void bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsValid = e.NewTextValue == null? false : ( e.NewTextValue.Length > 0 && e.NewTextValue.Length <= MaxLength ) ;
            if (!IsValid) if (e.NewTextValue!=null) if (e.NewTextValue.Length > MaxLength)
                ((Editor)sender).Text = e.NewTextValue.Substring(0, MaxLength);
        }

        protected override void OnDetachingFrom(Editor bindable)
        {
            bindable.TextChanged -= bindable_TextChanged;
        }
    }
    public class MaxLengthValidator : Behavior<Entry>
    {
        public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create("MaxLength", typeof(int), typeof(MaxLengthValidator), 0);

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            bindable.TextChanged += bindable_TextChanged;
        }

        private void bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (MaxLength != null && MaxLength.HasValue)
            if (e.NewTextValue.Length > MaxLength)
                ((Entry)sender).Text = e.NewTextValue.Substring(0, MaxLength);
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= bindable_TextChanged;
            
        }
        
    }
}
