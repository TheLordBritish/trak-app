using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using SparkyStudios.TrakLibrary.Controls;
using SparkyStudios.TrakLibrary.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderedPicker), typeof(BorderedPickerRenderer))]

namespace SparkyStudios.TrakLibrary.Droid.Renderers
{
    public class BorderedPickerRenderer : PickerRenderer
    {
        public BorderedPickerRenderer(Context context) : base(context)
        {
            
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == BorderedPicker.BorderColorProperty.PropertyName)
            {
                UpdateBorder((BorderedPicker) sender);
            }
        }
        
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && e.NewElement is BorderedPicker picker)
            {
                UpdateBorder(picker);
            }
        }
        
        private void UpdateBorder(BorderedPicker picker)
        {
            var gradientDrawable = new GradientDrawable();
            gradientDrawable.SetStroke(picker.BorderThickness, picker.BorderColor.ToAndroid());
            gradientDrawable.SetColor(picker.BackgroundColor.ToAndroid());
            Control.SetBackground(gradientDrawable);
                
            Control.SetBackground(gradientDrawable);
                
            Control.SetPadding(50, Control.PaddingTop, Control.PaddingRight,  
                Control.PaddingBottom);
        }
    }
}