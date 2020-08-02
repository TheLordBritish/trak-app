using Xamarin.Forms;

namespace Sparky.TrakApp.Controls
{
    public class BorderedEntry : Entry
    {
        public static readonly BindableProperty BorderThicknessProperty = 
            BindableProperty.Create(nameof(BorderThickness), typeof(int), typeof(BorderedEntry));
        
        public static readonly BindableProperty BorderColorProperty = 
            BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(BorderedEntry));
        
        public int BorderThickness
        {
            get => (int) GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public Color BorderColor
        {
            get => (Color) GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }
    }
}
