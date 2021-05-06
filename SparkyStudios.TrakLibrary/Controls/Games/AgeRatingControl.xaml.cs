using System.Collections.Generic;
using SparkyStudios.TrakLibrary.Model.Games;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparkyStudios.TrakLibrary.Controls.Games
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgeRatingControl
    {
        public static readonly BindableProperty AgeRatingsProperty =
            BindableProperty.Create(nameof(AgeRatings), typeof(IEnumerable<AgeRating>), typeof(AgeRatingControl));
        
        public AgeRatingControl()
        {
            InitializeComponent();
        }
        
        public IEnumerable<AgeRating> AgeRatings
        {
            get => (IEnumerable<AgeRating>)GetValue(AgeRatingsProperty);
            set => SetValue(AgeRatingsProperty, value);
        }
    }
}