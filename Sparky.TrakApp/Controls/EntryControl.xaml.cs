using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sparky.TrakApp.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EntryControl
    {
        private static readonly BindableProperty EntryTextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(EntryControl));
        private static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(EntryControl));

        public EntryControl()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string) GetValue(EntryTextProperty);
            set => SetValue(EntryTextProperty, value);
        }

        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }
    }
}