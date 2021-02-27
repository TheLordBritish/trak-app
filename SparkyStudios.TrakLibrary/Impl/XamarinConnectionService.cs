using SparkyStudios.TrakLibrary.Service;
using Xamarin.Essentials;

namespace SparkyStudios.TrakLibrary.Impl
{
    public class XamarinConnectionService : IConnectionService
    {
        private bool _isConnected;
        
        public XamarinConnectionService()
        {
            _isConnected = Connectivity.NetworkAccess == NetworkAccess.Internet;
            Connectivity.ConnectivityChanged += (sender, args) =>
            {
                _isConnected = args.NetworkAccess == NetworkAccess.Internet;
            };
        }

        public bool IsConnected()
        {
            return _isConnected;
        }
    }
}