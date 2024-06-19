using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using SmartPenUI_V2.Models;
using SmartPenUI_V2.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartPenUI_V2.ViewModels
{
    public partial class PredictionViewModel : ObservableObject, INavigationAware
    {
		// Dummy services
		private DummyTcpClientService? _tcpClientService;

		// Boolean properties for enabling and disabling buttons
		private bool _isInitialized = false;

		private bool _isConnected;
		[ObservableProperty]
		public bool isConnected;

		private bool _isPredicting = false;
		[ObservableProperty]
		public bool isPredicting;

		private bool _hasData = false;
		[ObservableProperty]
		public bool hasData;

		private bool _startStopActive = false;
		[ObservableProperty]
		public bool startStopActive;

		[ObservableProperty]
		private ObservableCollection<CharacterViewModel> characters = [];


		// Snackbar properties
		private ISnackbarService? _snackbarService;

		/// <summary>
		/// Constructor for the DataLabelingViewModel
		/// Initializes the LineSeries for Acceleration X, Y, Z and Gyro X, Y, Z
		/// aswell as the list of labels and the dummy services
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
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
				// subscribe to the DataReceived event for updating the chart series
				_tcpClientService.PredictionReceived += TcpClientService_OnPredictionReceived;
			}

			// retrieve the snackbar service
			_snackbarService = App.GetService<ISnackbarService>();
		}

		/// <summary>
		/// Command for connecting to and disconnecting from the TCP client
		/// </summary>
		[RelayCommand]
		public void ConnectDisconnectTCP()
		{
			// check for the connection status
			// if connected -> disconnect
			// if disconnected -> connect
			if (IsConnected)
			{
				// check if the labeling has been started
				if (IsPredicting)
					// if so, stop
					StartStopPredicting();

				// disconnect from the server
				_tcpClientService?.Disconnect();
				// clear the data as it is probably corrupted or not complete

				// show a snackbar message
				OpenSnackbar(ControlAppearance.Caution, "Disconnected successfully.", "The connection to the TCP server has been successfully terminated.", SymbolRegular.ErrorCircle24);
			}
			else
			{
				// connect to the tcp server
				_tcpClientService?.Connect();

				// show a snackbar message
				OpenSnackbar(ControlAppearance.Success, "Connected successfully!", "The connection to the TCP server has been successfully established.", SymbolRegular.Checkmark24);
			}

			// update the connection status from the viewmodel that is bound to the view
			IsConnected = _tcpClientService?.IsConnected ?? false;

			// enable or disable the start/stop button
			// should be disabled when there is no connection
			// should be enabled when there is a connection
			StartStopActive = IsConnected;
		}

		/// <summary>
		/// Command for starting and stopping the data labeling.
		/// </summary>
		[RelayCommand]
		public void StartStopPredicting()
		{
			// check if there is a connection to the server
			// if not, do nothing
			// if connected, start or stop the labeling
			if (IsConnected)
			{
				// check if the labeling has been started
				if (IsPredicting)
				{
					// stop the labeling
					_tcpClientService?.StopPredicting();

					// update the bool properties for the buttons
					IsPredicting = false;
					// disable the start/stop button after finishing the labeling
					StartStopActive = true;
					// enable the save and scrap buttons
					HasData = true;
				}
				else
				{
					// labeling has not been started
					// start the labeling
					_tcpClientService?.StartPredicting();
					// update the bool property for the labeling status
					IsPredicting = true;
				}
			}
		}

		/// <summary>
		/// Command for scrapping the data.
		/// </summary>
		[RelayCommand]
		public void ScrapHistory()
		{
			// clear the data
			Characters.Clear();

			// show a snackbar message
			OpenSnackbar(ControlAppearance.Caution, "History scrapped!", "The history has been scrapped.", SymbolRegular.Delete24);

			// re-enable the start/stop button
			StartStopActive = true;
		}

		/// <summary>
		/// Function for the WPF UI app
		/// Needs to be implemented in order for the Dependency Injection to work (INavigationAware)
		/// </summary>
		public void OnNavigatedTo()
		{
			// check if the viewmodel has been initialized
			// if not, initialize it
			if (!_isInitialized)
			{
				InitializeViewModel();
			}
			IsConnected = _tcpClientService?.IsConnected ?? false;

			// enable or disable the start/stop button
			// should be disabled when there is no connection
			// should be enabled when there is a connection
			StartStopActive = IsConnected;
		}

		/// <summary>
		/// Function for the WPF UI app
		/// Needs to be implemented in order for the Dependency Injection to work (INavigationAware)
		/// </summary>
		/// @note: not used in this viewmodel
		public void OnNavigatedFrom() { }

		/// <summary>
		/// Initialize the viewmodel.
		/// </summary>
		private void InitializeViewModel()
		{
			_isInitialized = true;
		}

		private void TcpClientService_OnPredictionReceived(string pred)
		{
			// add character to the list of characters
			AddCharacter(pred[0]);
			//Application.Current.Dispatcher.Invoke(() => Characters.Add(pred[0]));
		}

		private void AddCharacter(char character)
		{
			Application.Current.Dispatcher.Invoke(() => Characters.Add(new CharacterViewModel { Character = character.ToString() }));
			UpdateScales();
		}

		private void UpdateScales()
		{
			int count = Characters.Count;
			for (int i = 0; i < count; i++)
			{
				int reverseIndex = count - i - 1;
				double scale = 1 - (reverseIndex * 0.1);
				Characters[i].Scale = scale > 0.5 ? scale : 0.5; // Ensure scale does not go below 0.5, adjust as needed.
			}
		}

		/// <summary>
		/// Function for showing a snackbar message.
		/// </summary>
		/// <param name="appearance"></param>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="icon"></param>
		private void OpenSnackbar(ControlAppearance appearance, string title, string message, SymbolRegular icon)
		{
			// call the snackbar service to show a snackbar message
			_snackbarService.Show(
				title,
				message,
				appearance,
				new SymbolIcon(icon),
				TimeSpan.FromSeconds(2) // hard coded duration for the snackbar message
			);
		}
	}
}

public partial class CharacterViewModel : ObservableObject
{
	[ObservableProperty]
	private string character;

	[ObservableProperty]
	private double scale = 1.0;
}
