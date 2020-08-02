using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sparky.TrakApp.Common;
using Sparky.TrakApp.ViewModel.Common;

namespace Sparky.TrakApp.ViewModel
{
    /// <summary>
    /// The <see cref="BaseListViewModel{TModel, TViewModel}"/> is a simple view model that is used to represent any view which
    /// contains a list of items, specified by the type parameter. It also contains commands for manipulating and
    /// refreshing views, which can be bound and changed within any classes that inherit from this abstract one. 
    /// </summary>
    /// <typeparam name="TModel">The type to contain within any requests made by the commands.</typeparam>
    /// <typeparam name="TViewModel">The type to contain within the <see cref="Items"/> collection.</typeparam>
    public abstract class BaseListViewModel<TModel, TViewModel> : ReactiveViewModel where TModel : class where TViewModel : class
    {
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        protected BaseListViewModel(IScheduler scheduler, INavigationService navigationService) : base(scheduler,
            navigationService)
        {
            Items = new ObservableRangeCollection<TViewModel>();
            IsEmpty = true;
        }
        
        /// <summary>
        /// Used to specify whether the list view that is bound to this view model currently contains
        /// any items within the <see cref="Items"/>. Set to false by default.
        /// </summary>
        [Reactive]
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Used to specify whether the list view that is bound to this view model is currently
        /// refreshing or loading additional data.
        /// </summary>
        public bool IsRefreshing { [ObservableAsProperty] get; }
        
        /// <summary>
        /// An <see cref="ObservableCollection{T}"/> of items that is used to represent each individual item
        /// within the list page that this view model is bound to. By default before loading it is set to an
        /// empty instantiated list.
        /// </summary>
        [Reactive]
        public ObservableRangeCollection<TViewModel> Items { get; set; }
        
        /// <summary>
        /// Command that can be used to bind to any element that will refresh all data. 
        /// </summary>
        public ReactiveCommand<Unit, IEnumerable<TModel>> LoadCommand { get; protected set; }

        /// <summary>
        /// Command that is used to first load a list of data for the given list view, it can be
        /// also be used to load additional pages of data.
        /// </summary>
        public ReactiveCommand<Unit, IEnumerable<TModel>> LoadMoreCommand { get; protected set; }
    }
}