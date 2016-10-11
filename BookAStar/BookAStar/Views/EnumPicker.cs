using BookAStar.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.Views
{
    /// <summary>
    ///     A control for picking an element from an enumerated type.
    /// </summary>
    public class EnumPicker : Picker
    {
        #region Static Fields
        /// <summary>
        /// The source used to build values for this picker
        /// is any enum value from the desired enum type
        /// </summary>
        public static BindableProperty EnumSourceProperty =
            BindableProperty.Create(
                "EnumSource", typeof(Enum), typeof(EnumPicker),
                default(Enum), BindingMode.OneWay, propertyChanged: OnEnumSourceChanged);

        public static BindableProperty SelectedItemProperty =
            BindableProperty.Create("SelectedItem", typeof(Enum), typeof(EnumPicker),
                default(Enum), BindingMode.TwoWay, null, propertyChanged: OnSelectedItemChanged);

        #endregion

        #region Constructors and Destructors

        public EnumPicker()
        {
            this.SelectedIndexChanged += this.OnSelectedIndexChanged;
        }

        #endregion

        #region Public Properties

        public Enum EnumSource
        {
            get
            {
                return (Enum)this.GetValue(EnumSourceProperty);
            }
            set
            {
                this.SetValue(EnumSourceProperty, value);
            }
        }

        public Enum SelectedItem
        {
            get
            {
                return (Enum)this.GetValue(SelectedItemProperty);
            }
            set
            {
                this.SetValue(SelectedItemProperty, value);
            }
        }

        #endregion

        #region Methods
        private static void OnEnumSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = bindable as EnumPicker;
            if (picker == null)
            {
                return;
            }
            if (newValue == null)
            {
                return;
            }
            // Find the values not already contained in the Picker's list of items.
            var items = EnumExtensions.GetDescriptions(newValue.GetType());
            var toBeAdded = items.Where(item => !picker.Items.Contains(item));
            foreach (var item in toBeAdded)
            {
                picker.Items.Add(item);
            }
        }
        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = bindable as EnumPicker;
            if (newValue == null || oldValue == newValue || picker == null)
            {
                return;
            }
            picker.SelectedIndex = picker.Items.IndexOf(((Enum)newValue).GetDescription());
        }
        private void OnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            var picker = sender as EnumPicker;
            if (null == picker || (this.SelectedIndex < 0 || this.SelectedIndex > this.Items.Count - 1))
            {
                return;
            }

            var type = this.EnumSource.GetType();
            var values = EnumExtensions.GetDescriptions(type);
            var items = (from object item in values select item).ToList();
            picker.SelectedItem = EnumExtensions.GetEnumFromDescription(items[this.SelectedIndex].ToString(), type);
        }

        #endregion
    }
}
