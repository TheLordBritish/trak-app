using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Prism.Mvvm;

namespace SparkyStudios.TrakLibrary.ViewModel.Common
{
    public class ListItemViewModel : BindableBase
    {
        private Uri _imageUrl;
        private IEnumerable<ItemEntryViewModel> _headerDetails;
        private string _itemTitle;
        private string _itemSubTitle;
        private bool _showRating;
        private short _rating;

        public ICommand TapCommand { get; set; }

        public Uri ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public IEnumerable<ItemEntryViewModel> HeaderDetails
        {
            get => _headerDetails;
            set
            {
                if (value != null)
                {
                    var details = value.OrderBy(x => !x.IsSelected)
                        .ThenBy(x => x.Name)
                        .ToList();

                    for (var i = 0; i < details.Count - 1; i++)
                    {
                        details.ElementAt(i).HasNext = true;
                    }
                    
                    SetProperty(ref _headerDetails, details);
                }
                else
                {
                    SetProperty(ref _headerDetails, null);
                }
            }
        }

        public string ItemTitle
        {
            get => _itemTitle;
            set => SetProperty(ref _itemTitle, value);
        }

        public string ItemSubTitle
        {
            get => _itemSubTitle;
            set => SetProperty(ref _itemSubTitle, value);
        }

        public bool ShowRating
        {
            get => _showRating;
            set => SetProperty(ref _showRating, value);
        }

        public short Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }
    }
}