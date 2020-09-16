using System;
using System.Threading.Tasks;
using Prism.Behaviors;
using Prism.Common;
using Prism.Ioc;
using Prism.Logging;
using Prism.Navigation;
using Prism.Plugin.Popups;
using Rg.Plugins.Popup.Contracts;
using SparkyStudios.TrakLibrary.Common;
using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary
{
#if __IOS__
    [Foundation.Preserve(AllMembers = true)]
#elif __ANDROID__
    [Android.Runtime.Preserve(AllMembers = true)]
#endif
    public class TransitionPopupPageNavigationService : PopupPageNavigationService
    {
        public TransitionPopupPageNavigationService(IPopupNavigation popupNavigation, IContainerExtension container,
            IApplicationProvider applicationProvider, IPageBehaviorFactory pageBehaviorFactor, ILoggerFacade logger) :
            base(popupNavigation, container, applicationProvider, pageBehaviorFactor, logger)
        {
        }

        protected override Task<INavigationResult> GoBackInternal(INavigationParameters parameters,
            bool? useModalNavigation, bool animated)
        {
            if (parameters != null)
            {
                var hasTransition = parameters.TryGetValue<TransitionType>("transition-type", out var transitionType);

                if (hasTransition)
                {
                    MessagingCenter.Send<object, TransitionType>(this, "PageTransition", transitionType);
                }
            }
            else
            {
                MessagingCenter.Send<object, TransitionType>(this, "PageTransition", TransitionType.Default);
            }

            return base.GoBackInternal(parameters, useModalNavigation, animated);
        }

        protected override Task<INavigationResult> GoBackToRootInternal(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                var hasTransition = parameters.TryGetValue<TransitionType>("transition-type", out var transitionType);

                if (hasTransition)
                {
                    MessagingCenter.Send<object, TransitionType>(this, "PageTransition", transitionType);
                }
            }
            else
            {
                MessagingCenter.Send<object, TransitionType>(this, "PageTransition", TransitionType.Default);
            }

            return base.GoBackToRootInternal(parameters);
        }

        protected override Task<INavigationResult> NavigateInternal(Uri uri, INavigationParameters parameters,
            bool? useModalNavigation, bool animated)
        {
            if (parameters != null)
            {
                var hasTransition = parameters.TryGetValue<TransitionType>("transition-type", out var transitionType);

                if (hasTransition)
                {
                    MessagingCenter.Send<object, TransitionType>(this, "PageTransition", transitionType);
                }
            }
            else
            {
                MessagingCenter.Send<object, TransitionType>(this, "PageTransition", TransitionType.Default);
            }

            return base.NavigateInternal(uri, parameters, useModalNavigation, animated);
        }
    }
}