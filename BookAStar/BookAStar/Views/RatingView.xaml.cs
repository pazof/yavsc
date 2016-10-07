using BookAStar.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BookAStar.Views
{
    public partial class RatingView : ContentView
    {
        protected int rating;

        public int Rating {
            get
            {
                return rating;
            }
            set
            {
                if (value != rating)
                {
                    if (value < 0 || value > 5)
                    {
                        SetValue(RatingProperty, 0);
                        rating = 0;
                    }
                    else
                    {
                        SetValue(RatingProperty, value);
                        rating = value;
                    }
                    starBehaviors[4].SetValue(StarBehavior.RatingProperty, rating);
                    for (int i = 0; i < rating && i < 5; i++)
                        starBehaviors[i].SetValue(StarBehavior.IsStarredProperty, true);
                    for (int i = rating; i < 5 && i >= 0; i++)
                        starBehaviors[i].SetValue(StarBehavior.IsStarredProperty, false);
                }
            }
        }

        private bool isStarred;
        public bool IsStarred
        {
            get
            {
                return isStarred;
            }
            set
            {
                SetValue(IsStarredProperty, value);
            }
        }


        StarBehavior[] starBehaviors;
        public static BindableProperty IsStarredProperty = BindableProperty.Create(
            propertyName: "IsStarred",
            returnType: typeof(bool),
            declaringType: typeof(RatingView),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay
                );

        public static BindableProperty RatingProperty = BindableProperty.Create(
            propertyName: "Rating",
            returnType: typeof(int),
            declaringType: typeof(RatingView),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay
                );

        public RatingView()
        {
            InitializeComponent();
            starBehaviors = new StarBehavior[5]
            {
                starOne, starTwo, starThree, starFour, starFive
            };

        }
    }
}
