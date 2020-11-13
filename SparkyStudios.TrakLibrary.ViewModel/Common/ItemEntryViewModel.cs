using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Fody.Helpers;

namespace SparkyStudios.TrakLibrary.ViewModel.Common
{
    [ExcludeFromCodeCoverage]
    public class ItemEntryViewModel
    {
        /**
         * The unique ID of the item that this view model wraps around.
         */
        [Reactive] 
        public long Id { get; set; }

        /**
         * The name that will displayed by the view for the item that this view model wraps around.
         */
        [Reactive] 
        public string Name { get; set; }

        /**
         * Whether the item within this view model has been selected.
         */
        [Reactive] 
        public bool IsSelected { get; set; }
        
        /**
         * Specifies whether there is another item in a list of <see cref="ItemEntryViewModel"/> items.
         */
        [Reactive]
        public bool HasNext { get; set; }
    }
}