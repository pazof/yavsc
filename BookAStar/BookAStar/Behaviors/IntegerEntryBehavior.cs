using Xamarin.Forms;

namespace BookAStar.Behaviors
{
    public class IntegerEntryBehavior : Behavior<Entry>
    {
        public static readonly BindableProperty MinProperty = BindableProperty.Create("Min", typeof(int), typeof(IntegerEntryBehavior), 0);
        public static readonly BindableProperty MaxProperty = BindableProperty.Create("Max", typeof(int), typeof(IntegerEntryBehavior), 0);

        public int Min
        {
            get { return (int)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public int Max
        {
            get { return (int)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            bindable.TextChanged += bindable_TextChanged;
        }

        private void bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            int val;
            if (int.TryParse(e.NewTextValue, out val))
            {
                IsValid = (Min > Max) || (Max >= val && val >= Min);
            }
            else IsValid = false;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= bindable_TextChanged;

        }
        public bool IsValid { get; private set; }
    }
}
