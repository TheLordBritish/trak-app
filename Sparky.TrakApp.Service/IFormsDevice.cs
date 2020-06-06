using System;

namespace Sparky.TrakApp.Service
{
    public interface IFormsDevice
    {
        void BeginInvokeOnMainThread(Action action);
    }
}