using BookAStar.Model.Workflow;
using BookAStar.ViewModels.EstimateAndBilling;
using Xamarin.Forms;

namespace BookAStar.ViewModels.Signing
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
    }
}
