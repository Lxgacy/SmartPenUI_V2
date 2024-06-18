using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;
using System.Windows.Media;
using SmartPenUI_V2.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SmartPenUI_V2.Services;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartPenUI_V2.ViewModels
{
    public partial class DataLabelingViewModel : ObservableObject, INavigationAware
    {
        private DummyTcpClientService? _tcpClientService;
		private bool _isInitialized = false;

		private readonly Random _random = new();
		private readonly ObservableCollection<ObservableValue>? _observableValues;
		public ObservableCollection<ISeries>? Series { get; set; }

		public DataLabelingViewModel()
		{
			_observableValues = new ObservableCollection<ObservableValue>
			{
				// Use the ObservableValue or ObservablePoint types to let the chart listen for property changes 
				// or use any INotifyPropertyChanged implementation 
				new(2),
				new(5), // the ObservableValue type is redundant and inferred by the compiler (C# 9 and above)
				new(4),
				new(5),
				new(2),
				new(6),
				new(6),
				new(6),
				new(4),
				new(2),
				new(3),
				new(4),
				new(3)
			};

			Series = new ObservableCollection<ISeries>
			{
				new LineSeries<ObservableValue>
				{
					Values = _observableValues,
					Fill = null
				}
			};

			//Initialize the dummy TCP client service

			_tcpClientService = App.GetService<DummyTcpClientService>();
			if (_tcpClientService == null)
			{
				throw new InvalidOperationException(
					"The DummyTcpClientService is not registered in the service provider.");
			}
			else
			{
				_tcpClientService.DataReceived += TcpClientService_OnDataReceived;
			}
		}

		public void OnNavigatedTo()
        {
			_tcpClientService?.Connect();
			if (!_isInitialized)
            {
				InitializeViewModel();
            }
		}

        public void OnNavigatedFrom()
        {
			_tcpClientService?.Disconnect();
		}

        private void InitializeViewModel()
        {
			_isInitialized = true;
        }

		private void TcpClientService_OnDataReceived(byte[] data)
		{
			var randomValue = data[0];
			_observableValues?.Add(new(randomValue));
		}
	}
}