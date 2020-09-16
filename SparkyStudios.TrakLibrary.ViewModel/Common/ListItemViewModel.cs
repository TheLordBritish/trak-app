using System;
using System.Windows.Input;
using Prism.Mvvm;

namespace SparkyStudios.TrakLibrary.ViewModel.Common
{
    public class ListItemViewModel : BindableBase
    {
        private Uri _imageUrl;
        private string _header;
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

        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
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