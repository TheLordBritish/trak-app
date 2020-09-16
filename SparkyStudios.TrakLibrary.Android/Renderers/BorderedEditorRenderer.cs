using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using SparkyStudios.TrakLibrary.Controls;
using SparkyStudios.TrakLibrary.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderedEditor), typeof(BorderedEditorRenderer))]

namespace SparkyStudios.TrakLibrary.Droid.Renderers
{
    public class BorderedEditorRenderer : EditorRenderer
    {
        public BorderedEditorRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && e.NewElement is BorderedEditor editor)
            {
                UpdateBorder(editor);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == BorderedEntry.BorderColorProperty.PropertyName)
            {
                UpdateBorder((BorderedEditor) sender);
            }
        }

        private void UpdateBorder(BorderedEditor editor)
        {
            var gradientDrawable = new GradientDrawable();
            gradientDrawable.SetStroke(editor.BorderThickness, editor.BorderColor.ToAndroid());
            gradientDrawable.SetColor(editor.BackgroundColor.ToAndroid());
            Control.SetBackground(gradientDrawable);
                
            Control.SetBackground(gradientDrawable);
                
            Control.SetPadding(50, Control.PaddingTop, Control.PaddingRight,  
                Control.PaddingBottom);
        }
    }
}