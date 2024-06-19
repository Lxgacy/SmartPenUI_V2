using LiveChartsCore.Defaults;
using SmartPenUI_V2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace SmartPenUI_V2.ViewModels
{
    public partial class PredictionViewModel : ObservableObject, INavigationAware
    {
		private DummyTcpClientService? _tcpClientService;
		private bool _isInitialized = false;

		private bool _isConnected;

		[ObservableProperty]
		public bool isConnected;

        public PredictionViewModel()
        {
			//Initialize the dummy TCP client service
			_tcpClientService = App.GetService<DummyTcpClientService>();
			if (_tcpClientService == null)
			{
				throw new InvalidOperationException(
					"The DummyTcpClientService is not registered in the service provider.");
			}
			else
			{
				_tcpClientService.PredictionReceived += TcpClientService_OnPredictionReceived;
			}
		}

		public void OnNavigatedTo()
        {
            if (!_isInitialized)
            {
                InitializeViewModel();
            }
        }

        public void OnNavigatedFrom() { }

        [RelayCommand]
        public void ConnectDisconnectTCP()
        {
			if (IsConnected)
			{
				_tcpClientService?.Disconnect();
			}
			else
			{
				_tcpClientService?.Connect();
			}

			IsConnected = _tcpClientService?.IsConnected ?? false;
		}

        private void InitializeViewModel()
        {
            // do some stuff

            _isInitialized = true;
        }

		private void TcpClientService_OnPredictionReceived(string pred)
		{
			
		}
	}
}
