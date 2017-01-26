using ZicMoove.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ZicMoove.Views
{
    public partial class RatingView : ContentView
    {
        public static BindableProperty RatingProperty = BindableProperty.Create(
            "Rating", typeof(int), typeof(RatingView), 0, BindingMode.TwoWay,
            propertyChanged: OnRatingChanged);

        private static void OnRatingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (RatingView) bindable;
            int newValueInt = (int)newValue;
            if (oldValue!=newValue)
            {
                // This will set starBehaviors[4].Rating, five times
                for (int i = 0; i < 5 && i < newValueInt; i++)
                    view.starBehaviors[i].IsStarred = true;
                for (int i = newValueInt; i < 5; i++)
                    view.starBehaviors[i].IsStarred = false;
            }
        }

        StarBehavior[] starBehaviors;

        public RatingView()
        {
            InitializeComponent();
            starBehaviors = new StarBehavior[5]
            {
                starOne, starTwo, starThree, starFour, starFive
            };
            StarBehavior.StarTapped += StarBehavior_StarTapped;
        }

        private void StarBehavior_StarTapped(object sender, EventArgs e)
        {
            if (starBehaviors.Contains(sender))
             SetValue(RatingProperty, starFive.Rating);
        }
    }
}
