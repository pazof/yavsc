using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.ViewModels.Validation
{
    public class ModelState : BindableObject
    {
        public static readonly BindableProperty IsValidProperty =
            BindableProperty.Create("IsValid", typeof(bool), typeof(ModelState), false);

        public static readonly BindableProperty ErrorsProperty =
           BindableProperty.Create("Errors", typeof(Dictionary<string,List<InputError>>), typeof(ModelState), null);

        public ModelState()
        {
           Errors = new Dictionary<string, List<InputError>>();
        }

        public bool IsValid
        {
            get
            {
                return (bool) GetValue(IsValidProperty);
            }
        }

        public Dictionary<string, List<InputError>> Errors
        {
            get
            {
                return (Dictionary<string, List<InputError>>)GetValue(ErrorsProperty);
            }
            set
            {
                SetValue(ErrorsProperty, value);
            }
        } 

        public virtual void AddError(string propertyName, string errorMessage, ErrorSeverity severity = ErrorSeverity.Crippling)
        {
            InputError e = new InputError(errorMessage, severity) ;
            if (Errors.ContainsKey(propertyName))
            {
                var errList = Errors[propertyName];
                errList.Add(e);
            }
            else
            {
                Errors.Add(propertyName, new List<InputError>(new InputError [] { e }));
            }
            if (e.Severity < ErrorSeverity.Bearable)
                SetValue(IsValidProperty, false);
        }

        public virtual void Clear ()
        {
            Errors.Clear();
            SetValue(IsValidProperty, true);
        }
    }
}
