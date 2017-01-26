﻿using XLabs.Forms.Mvvm;

namespace ZicMoove.ViewModels
{
    using EstimateAndBilling;
    using UserProfile;

    public class HomeViewModel : ViewModel
    {
        public BookQueriesViewModel BookQueries { get; set;  }
        public UserProfileViewModel UserProfile { get; set; }
        
    }
}
