using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sparky.TrakApp.Controls.Common
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RatingUserControl
    {
        public static readonly BindableProperty RatingProperty = 
            BindableProperty.Create(nameof(Rating), typeof(short), typeof(RatingUserControl));
        
        public static readonly BindableProperty OnTappedCommandProperty =
            BindableProperty.Create(nameof(OnTappedCommand), typeof(ICommand), typeof(RatingUserControl));
                
        public RatingUserControl()
        {
            InitializeComponent();
        }
        
        public short Rating
        {
            get => (short) GetValue(RatingProperty);
            set => SetValue(RatingProperty, value);
        }

        public ICommand OnTappedCommand
        {
            get => (ICommand) GetValue(OnTappedCommandProperty);
            set => SetValue(OnTappedCommandProperty, value);
        }
    }
}