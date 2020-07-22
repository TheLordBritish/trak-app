using Sparky.TrakApp.Common;
using Xamarin.Forms;

namespace Sparky.TrakApp
{
    public partial class BaseNavigationPage
    {
        public static readonly BindableProperty TransitionTypeProperty =
            BindableProperty.Create("TransitionType", typeof(TransitionType), typeof(BaseNavigationPage), TransitionType.Default);
        
        public BaseNavigationPage()
        {
            InitializeComponent();
        }
        
        public TransitionType TransitionType
        {
            get => (TransitionType)GetValue(TransitionTypeProperty);
            set => SetValue(TransitionTypeProperty, value);
        }
    }
}