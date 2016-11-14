using Xamarin.Forms;
using System.Windows.Input;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels.Signing
{
    using Model.Settings;
    public class SignaturePadConfigViewModel : ViewModel
    {
        private readonly ICommand isConfiguringCommand;
        private bool isConfiguring;

        private string captionText;
        private string clearText;
        private string promptText;
        private int strokeWidth;

        private Color captionTextColor;
        private Color clearTextColor;
        private Color promptTextColor;
        private Color signaturePadBackground;
        private Color signatureLineColor;
        private Color strokeColor;

        public SignaturePadConfigViewModel()
        {
            isConfiguringCommand = new Command(() => IsConfiguring = !IsConfiguring);
        }

        public ICommand ConfigureCommand => isConfiguringCommand;

        public bool IsConfiguring
        {
            get { return isConfiguring; }
            set { SetProperty<bool>(ref isConfiguring, value); }
        }

        public string CaptionText
        {
            get { return captionText; }
            set { SetProperty(ref captionText, value); }
        }

        public string ClearText
        {
            get { return clearText; }
            set { SetProperty(ref clearText, value); }
        }

        public string PromptText
        {
            get { return promptText; }
            set { SetProperty(ref promptText, value); }
        }

        public Color CaptionTextColor
        {
            get { return captionTextColor; }
            set { if (SetProperty(ref captionTextColor, value))
                    NotifyPropertyChanged(nameof(CaptionTextColorIndex)); }
        }

        public Color ClearTextColor
        {
            get { return clearTextColor; }
            set { if (SetProperty(ref clearTextColor, value))
                    NotifyPropertyChanged(nameof(ClearTextColorIndex)); }
        }

        public Color PromptTextColor
        {
            get { return promptTextColor; }
            set { if (SetProperty(ref promptTextColor, value))
                    NotifyPropertyChanged(nameof(PromptTextColorIndex)); }
        }

        public Color SignaturePadBackground
        {
            get { return signaturePadBackground; }
            set { if (SetProperty(ref signaturePadBackground, value))
                    NotifyPropertyChanged(nameof(SignaturePadBackgroundIndex)); }
        }

        public Color SignatureLineColor
        {
            get { return signatureLineColor; }
            set { if (SetProperty(ref signatureLineColor, value))
                    NotifyPropertyChanged(nameof(SignatureLineColorIndex)); }
        }

        public Color StrokeColor
        {
            get { return strokeColor; }
            set { if (SetProperty(ref strokeColor, value))
                    NotifyPropertyChanged(nameof(StrokeColorIndex)); }
        }

        public int StrokeWidth
        {
            get { return strokeWidth; }
            set { SetProperty(ref strokeWidth, value); }
        }

        public int CaptionTextColorIndex
        {
            get { return SignatureSettings.Colors.IndexOf(CaptionTextColor); }
            set { CaptionTextColor = SignatureSettings.Colors[value]; }
        }

        public int ClearTextColorIndex
        {
            get { return SignatureSettings.Colors.IndexOf(ClearTextColor); }
            set { ClearTextColor = SignatureSettings.Colors[value]; }
        }

        public int PromptTextColorIndex
        {
            get { return SignatureSettings.Colors.IndexOf(PromptTextColor); }
            set { PromptTextColor = SignatureSettings.Colors[value]; }
        }

        public int SignaturePadBackgroundIndex
        {
            get { return SignatureSettings.Colors.IndexOf(SignaturePadBackground); }
            set { SignaturePadBackground = SignatureSettings.Colors[value]; }
        }

        public int SignatureLineColorIndex
        {
            get { return SignatureSettings.Colors.IndexOf(SignatureLineColor); }
            set { SignatureLineColor = SignatureSettings.Colors[value]; }
        }

        public int StrokeColorIndex
        {
            get { return SignatureSettings.Colors.IndexOf(StrokeColor); }
            set { StrokeColor = SignatureSettings.Colors[value]; }
        }
        public override void OnViewAppearing()
        {
            IsConfiguring = true;

            CaptionText = "signez ici";
            ClearText = "éffacer";
            PromptText = ">";
            StrokeWidth = 2;

            CaptionTextColor = Color.Gray;
            ClearTextColor = Color.Gray;
            PromptTextColor = Color.Gray;
            SignaturePadBackground = Color.Yellow;
            SignatureLineColor = Color.Black;
            StrokeColor = Color.Black;

            base.OnViewAppearing();
        }

    }
}

