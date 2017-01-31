using Xamarin.Forms;

namespace ZicMoove.Behaviors
{
    public class IntegerEntryBehavior : Behavior<Entry>
    {
        public static readonly BindableProperty MinProperty = BindableProperty.Create("Min", typeof(int), typeof(IntegerEntryBehavior), 0);
        public static readonly BindableProperty MaxProperty = BindableProperty.Create("Max", typeof(int), typeof(IntegerEntryBehavior), 0);
        public static readonly BindableProperty IsValidProperty =
            BindableProperty.Create("IsValid", typeof(bool), typeof(IntegerEntryBehavior), false);
        public static readonly BindableProperty ErrorProperty =
            BindableProperty.Create("Error", typeof(string), typeof(IntegerEntryBehavior), null);
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
                Error = string.Format(Strings.MinMaxIntError, Min, Max);
            }
            else
            {
                IsValid = false;
                Error = "";
            }
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= bindable_TextChanged;

        }
        public bool IsValid
        {
            get {
                return (bool) GetValue(IsValidProperty);
            }
            set
            {
                SetValue(IsValidProperty, value);
            }
        }
        public string Error
        {
            get
            {
                return (string) GetValue(ErrorProperty);
            }
            set
            {
                SetValue(ErrorProperty, value);
            }
        }
    }
}
