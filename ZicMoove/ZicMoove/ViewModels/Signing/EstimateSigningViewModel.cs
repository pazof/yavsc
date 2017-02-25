using ZicMoove.Model.Workflow;
using ZicMoove.ViewModels.EstimateAndBilling;
using Xamarin.Forms;

namespace ZicMoove.ViewModels.Signing
{
    public class EstimateSigningViewModel: EditEstimateViewModel
    {
        public EstimateSigningViewModel(Estimate document): base(document)
        {
        }

        public Command<bool> ValidationCommand { get; set; }

        public ImageSource ProSignImage {
            get
            {
                return new FileImageSource();
            }
        }

        public ImageSource CliSignImage
        {
            get
            {
                return new FileImageSource();
            }
        }

        private bool isClientView=false;

        public bool IsClientView {
            get
            {
                return isClientView;
            }
            set {
                bool old = isClientView;
                SetProperty<bool>(ref isClientView, value);
                if (old!=value)
                {
                    if (value != !isProviderView)
                        IsProviderView = !value;
                }
            }
        }

        private bool isProviderView=true;

        public bool IsProviderView
        {
            get
            {
                return isProviderView;
            }
            set
            {
                bool old = isProviderView;
                SetProperty<bool>(ref isProviderView, value);
                if (old != value)
                {
                    if (value != !isClientView)
                        IsClientView = !value;
                }
            }
        }
        public string CaptionText
        {
            get
            {
                return Strings.EstimateSigningCaption;
            }
        }

        public string PromptText
        {
            get
            {
                return Strings.EstimateSigningPrompt;
            }
        }
    }
}
