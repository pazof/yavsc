using System;
using Xamarin.Forms;

namespace ZicMoove.Behaviors
{
    public class EditorMaxLengthValidator : Behavior<Editor>
    {
        public static readonly BindableProperty MaxLengthProperty = 
            BindableProperty.Create("MaxLength", typeof(int), typeof(EditorMaxLengthValidator), int.MaxValue);

        public static readonly BindableProperty MinLengthProperty = 
            BindableProperty.Create("MinLength", typeof(int), typeof(EditorMaxLengthValidator), 0);

        public static readonly BindableProperty IsValidProperty = 
            BindableProperty.Create("IsValid", typeof(bool), typeof(EditorMaxLengthValidator), false);

        public static readonly BindableProperty ErrorProperty = 
            BindableProperty.Create("Error", typeof(string), typeof(EditorMaxLengthValidator), null);

        public int MaxLength
        {
            get { return (int) GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        public int MinLength
        {
            get { return (int) GetValue(MinLengthProperty); }
            set { SetValue(MinLengthProperty, value); }
        }

        protected override void OnAttachedTo(Editor bindable)
        {
            bindable.TextChanged += bindable_TextChanged;
        }

        public bool IsValid {
            get { return (bool) GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value);
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
        private void bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue != null && e.NewTextValue.Length >= MinLength)
            {
                if (e.NewTextValue.Length > MaxLength)
                {
                    ((Editor)sender).Text = e.NewTextValue.Substring(0, MaxLength);
                    Error = Strings.YourTextWasTooLong;
                }
                else Error = "";
                IsValid = true;
            }
            else
            {
                Error = string.Format(Strings.MinMaxStringValidationError,
                    MinLength, MaxLength);
                IsValid = false;
            }
        }

        protected override void OnDetachingFrom(Editor bindable)
        {
            bindable.TextChanged -= bindable_TextChanged;
        }
    }
    
}
