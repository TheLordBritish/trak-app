
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using ButtonRenderer = SparkyStudios.TrakLibrary.Droid.Renderers.ButtonRenderer;

[assembly: ExportRenderer(typeof(Button), typeof(ButtonRenderer))]

namespace SparkyStudios.TrakLibrary.Droid.Renderers
{
    public class ButtonRenderer : Xamarin.Forms.Platform.Android.ButtonRenderer
    {
        public ButtonRenderer(Context context) : base(context)
        {
        }
        
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            var b = Control;
            b.SetAllCaps(false);
        }
    }
}