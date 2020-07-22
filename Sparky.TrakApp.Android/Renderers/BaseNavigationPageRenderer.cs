using System.ComponentModel;
using Android.Content;
using Android.Support.V4.App;
using Sparky.TrakApp;
using Sparky.TrakApp.Common;
using Sparky.TrakApp.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(BaseNavigationPage), typeof(BaseNavigationPageRenderer))]

namespace Sparky.TrakApp.Droid.Renderers
{
    public class BaseNavigationPageRenderer : NavigationPageRenderer
    {
        private TransitionType _transitionType = TransitionType.Default;

        public BaseNavigationPageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == BaseNavigationPage.TransitionTypeProperty.PropertyName)
            {
                UpdateTransitionType();
            }
        }

        protected override void SetupPageTransition(FragmentTransaction transaction, bool isPush)
        {
            switch (_transitionType)
            {
                case TransitionType.None:
                    return;
                case TransitionType.Default:
                    return;
                case TransitionType.SlideFromLeft:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_left, Resource.Animation.exit_right,
                            Resource.Animation.enter_right, Resource.Animation.exit_left);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_right, Resource.Animation.exit_left,
                            Resource.Animation.enter_left, Resource.Animation.exit_right);
                    }

                    break;
                case TransitionType.SlideFromRight:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_right, Resource.Animation.exit_left,
                            Resource.Animation.enter_left, Resource.Animation.exit_right);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_left, Resource.Animation.exit_right,
                            Resource.Animation.enter_right, Resource.Animation.exit_left);
                    }

                    break;
                case TransitionType.SlideFromTop:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_top, Resource.Animation.exit_bottom,
                            Resource.Animation.enter_bottom, Resource.Animation.exit_top);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_bottom, Resource.Animation.exit_top,
                            Resource.Animation.enter_top, Resource.Animation.exit_bottom);
                    }

                    break;
                case TransitionType.SlideFromBottom:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_bottom, Resource.Animation.exit_top,
                            Resource.Animation.enter_top, Resource.Animation.exit_bottom);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_top, Resource.Animation.exit_bottom,
                            Resource.Animation.enter_bottom, Resource.Animation.exit_top);
                    }

                    break;
                default:
                    return;
            }
        }

        private void UpdateTransitionType()
        {
            var transitionNavigationPage = (BaseNavigationPage) Element;
            _transitionType = transitionNavigationPage.TransitionType;
        }
    }
}