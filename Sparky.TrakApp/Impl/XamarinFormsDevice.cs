using System;
using Sparky.TrakApp.Service;
using Xamarin.Forms;

namespace Sparky.TrakApp.Impl
{
    public class XamarinFormsDevice : IFormsDevice
    {
        public void BeginInvokeOnMainThread(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }
    }
}