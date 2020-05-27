using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Navigation;

namespace Sparky.TrakApp.ViewModel
{
    /// <summary>
    /// The <see cref="BaseListViewModel{T}"/> is a simple view model that is used to represent any view which
    /// contains a list of items, specified by the type parameter. It also contains commands for manipulating and
    /// refreshing views, which can be bound and changed within any classes that inherit from this abstract one. 
    /// </summary>
    /// <typeparam name="T">The type to contain within the <see cref="Items"/> collection.</typeparam>
    public abstract class BaseListViewModel<T> : BaseViewModel where T : class
    {
        private bool _isEmpty;
        private ObservableCollection<T> _items;
        
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        protected BaseListViewModel(INavigationService navigationService) : base(navigationService)
        {
            Items = new ObservableCollection<T>();
        }

        /// <summary>
        /// Command that is used to first load a list of data for the given list view, it can be
        /// also be used to load additional pages of data.
        /// </summary>
        public ICommand LoadMoreCommand { get; protected set; }
        
        /// <summary>
        /// Command that can be used to bind to any pull to refresh functionality on any list view. 
        /// </summary>
        public ICommand RefreshCommand { get; protected set; }
        
        /// <summary>
        /// Used to specify whether the list view that is bound to this view model currently contains
        /// any items within the <see cref="Items"/>. Set to false by default.
        /// </summary>
        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        /// <summary>
        /// An <see cref="ObservableCollection{T}"/> of items that is used to represent each individual item
        /// within the list page that this view model is bound to. By default before loading it is set to an
        /// empty instantiated list.
        /// </summary>
        public ObservableCollection<T> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }
    }
}