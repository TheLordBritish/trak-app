using System;
using SparkyStudios.TrakLibrary.Service;
using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary.Impl
{
    public class XamarinFormsDevice : IFormsDevice
    {
        public void BeginInvokeOnMainThread(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }
    }
}