using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Xamarin.Forms.Color;

[assembly: ExportRenderer(typeof(SearchBar), typeof(SparkyStudios.TrakLibrary.Droid.Renderers.SearchBarRenderer))]

namespace SparkyStudios.TrakLibrary.Droid.Renderers
{
    public class SearchBarRenderer : Xamarin.Forms.Platform.Android.SearchBarRenderer
    {
        public SearchBarRenderer(Context context) : base(context)
        {
        }
        
        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                var searchView = Control;

                //Get the Id for your search icon
                var searchIconId = Resources.GetIdentifier("android:id/search_mag_icon", null, null);
                if (searchIconId > 0)
                {
                    var searchPlateIcon = searchView.FindViewById(searchIconId);
                    (searchPlateIcon as ImageView)?.SetColorFilter(Color.White.ToAndroid(), PorterDuff.Mode.SrcIn);
                    
                    var searchViewIcon = searchView.FindViewById<ImageView>(searchIconId);
                    var linearLayoutSearchView = (ViewGroup)searchViewIcon.Parent;

                    searchViewIcon.SetAdjustViewBounds(true);

                    //Remove the search icon from the view group and add it once again to place it at the end of the view group elements
                    linearLayoutSearchView.RemoveView(searchViewIcon);
                    linearLayoutSearchView.AddView(searchViewIcon);
                }
                
                var plateId = Resources.GetIdentifier("android:id/search_plate", null, null);
                var plate = Control.FindViewById(plateId);
                plate.SetBackgroundColor(Android.Graphics.Color.Transparent);
            }
        }
    }
}