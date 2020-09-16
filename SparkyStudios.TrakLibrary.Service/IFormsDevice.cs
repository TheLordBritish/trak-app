using System;

namespace SparkyStudios.TrakLibrary.Service
{
    public interface IFormsDevice
    {
        void BeginInvokeOnMainThread(Action action);
    }
}