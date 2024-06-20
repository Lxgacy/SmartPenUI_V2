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
using Wpf.Ui;
using Microsoft.Extensions.Primitives;
using System.Runtime.InteropServices;

namespace SmartPenUI_V2.ViewModels
{
    public partial class DataLabelingViewModel : ObservableObject, INavigationAware
    {
		// Dummy services
        private DummyTcpClientService? _tcpClientService;
		private readonly DummyInfluxDBService? _influxDBService;

		// Properties for the list of projects and currently selected project
		public ObservableCollection<Project> Projects => _influxDBService?.GetProjects() ?? new ObservableCollection<Project>();
		private Project _selectedProject;
		public Project SelectedProject
		{
			get => _selectedProject;
			set
			{
				SetProperty(ref _selectedProject, value);
				OnPropertyChanged(nameof(SelectedProject.Measurements));
			}
		}


		// Boolean properties for enabling and disabling buttons
		private bool _isInitialized = false;

		private bool _isConnected = false;
		[ObservableProperty]
		public bool isConnected;

		private bool _isLabeling = false;
		[ObservableProperty]
		public bool isLabeling;

		private bool _hasData = false;
		[ObservableProperty]
		public bool hasData;

		private bool _startStopActive = false;
		[ObservableProperty]
		public bool startStopActive;


		// list of labels (A-Z and 
		//private ObservableCollection<string> _labels;
		private List<string> _labels;

		// property for selected label
		private string _selectedLabel = string.Empty;
		[ObservableProperty]
		public string selectedLabel;
		private int _selectedLabelIndex;


		// collections of data for the chart series
		private readonly ObservableCollection<ObservableValue>? _valuesAccX = [];
		private readonly ObservableCollection<ObservableValue>? _valuesAccY = [];
		private readonly ObservableCollection<ObservableValue>? _valuesAccZ = [];

		private readonly ObservableCollection<ObservableValue>? _valuesGyroX = [];
		private readonly ObservableCollection<ObservableValue>? _valuesGyroY = [];
		private readonly ObservableCollection<ObservableValue>? _valuesGyroZ = [];

		private readonly ObservableCollection<ObservableValue>? _valuesFSR = [];

		// properties for the chart series
		public ObservableCollection<ISeries>? AccSeries { get; set; } = [];
		public ObservableCollection<ISeries>? GyroSeries { get; set; } = [];
		public ObservableCollection<ISeries>? PressureSeries { get; set; } = [];


		// Snackbar properties
		private ISnackbarService? _snackbarService;

		/// <summary>
		/// Constructor for the DataLabelingViewModel
		/// Initializes the LineSeries for Acceleration X, Y, Z and Gyro X, Y, Z
		/// aswell as the list of labels and the dummy services
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public DataLabelingViewModel()
		{
			// Initialize the LineSeries for Acceleration X, Y, Z
			AccSeries.Add(new LineSeries<ObservableValue> { Values = _valuesAccX, Fill = null, Name = "X" });
			AccSeries.Add(new LineSeries<ObservableValue> { Values = _valuesAccY, Fill = null, Name = "Y" });
			AccSeries.Add(new LineSeries<ObservableValue> { Values = _valuesAccZ, Fill = null, Name = "Z" });

			// Initialize the LineSeries for Gyro (Angular Velocity) X, Y, Z
			GyroSeries.Add(new LineSeries<ObservableValue> { Values = _valuesGyroX, Fill = null, Name = "X" });
			GyroSeries.Add(new LineSeries<ObservableValue> { Values = _valuesGyroY, Fill = null, Name = "Y" });
			GyroSeries.Add(new LineSeries<ObservableValue> { Values = _valuesGyroZ, Fill = null, Name = "Z" });

			PressureSeries.Add(new LineSeries<ObservableValue> { Values = _valuesFSR, Fill = null, Name = "FSR" });

			// Initialize the list of labels
			_labels = [ "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P",
						"Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "NaN", "Silent" ];

			_selectedLabelIndex = 0;
			SelectedLabel = _labels[_selectedLabelIndex];

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
				_tcpClientService.DataReceived += TcpClientService_OnDataReceived;
			}

			// Initialize the dummy InfluxDB service
			_influxDBService = App.GetService<DummyInfluxDBService>();
			if (_influxDBService == null)
			{
				throw new InvalidOperationException(
					"The DummyTcpClientService is not registered in the service provider.");
			}

			// retrieve the snackbar service
			_snackbarService = App.GetService<ISnackbarService>();
		}

		/// <summary>
		/// Command for connecting to and disconnecting from the TCP client
		/// </summary>
		[RelayCommand]
		public async void ConnectDisconnectTCP()
		{
			// check for the connection status
			// if connected -> disconnect
			// if disconnected -> connect
			if (IsConnected)
			{
				// check if the labeling has been started
				if (IsLabeling)
					// if so, stop
					StartStopLabeling();

				// disconnect from the server
				_tcpClientService?.Disconnect();
				// clear the data as it is probably corrupted or not complete
				ClearData();

				// show a snackbar message
				OpenSnackbar(ControlAppearance.Caution, "Disconnected successfully.", "The connection to the TCP server has been successfully terminated.", SymbolRegular.ErrorCircle24);
			}
			else
			{
				// connect to the tcp server
				// check if the connection was successful
				if (await _tcpClientService?.Connect())
				{
					// show a snackbar message
					OpenSnackbar(ControlAppearance.Success, "Connected successfully!", "The connection to the TCP server has been successfully established.", SymbolRegular.Checkmark24);
				}
				else
				{
					// show a snackbar message
					OpenSnackbar(ControlAppearance.Danger, "Connection failed!", "The connection to the TCP server could not be established.", SymbolRegular.ErrorCircle24);
				}
			}

			// update the connection status from the viewmodel that is bound to the view
			IsConnected = _tcpClientService?.IsConnected ?? false;

			// enable or disable the start/stop button
			// should be disabled when there is no connection
			// should be enabled when there is a connection
			StartStopActive = IsConnected;
		}

		/// <summary>
		/// Command for reverting to the last label.
		/// Can be used if the data is not sufficient or the wrong label has been selected.
		/// </summary>
		[RelayCommand]
		public void PreviousLabel()
		{
			// decrement the selected label index (modulo to not exceed the bounds)
			_selectedLabelIndex = (_selectedLabelIndex - 1 + _labels.Count) % _labels.Count;
			// update the selected label
			SelectedLabel = _labels[_selectedLabelIndex];
		}

		/// <summary>
		/// Command for starting and stopping the data labeling.
		/// </summary>
		[RelayCommand]
		public void StartStopLabeling()
		{
			// check if there is a connection to the server
			// if not, do nothing
			// if connected, start or stop the labeling
			if (IsConnected)
			{
				// check if the labeling has been started
				if (IsLabeling)
				{
					// stop the labeling
					_tcpClientService?.StopLabeling();
					
					// update the bool properties for the buttons
					IsLabeling = false;
					// disable the start/stop button after finishing the labeling
					StartStopActive = false;

					// check if there is data to be saved
					if (_valuesAccX?.Count > 0 && _valuesAccY?.Count > 0 && _valuesAccZ?.Count > 0 &&
						_valuesGyroX?.Count > 0 && _valuesGyroY?.Count > 0 && _valuesGyroZ?.Count > 0 &&
						_valuesFSR?.Count > 0)
					{
						// enable the save and scrap buttons
						HasData = true;
					}
					else
						StartStopActive = true;
				}
				else
				{
					// labeling has not been started
					// start the labeling
					_tcpClientService?.StartLabeling();
					// update the bool property for the labeling status
					IsLabeling = true;
				}
			}
		}

		/// <summary>
		/// Command for saving the data to the InfluxDB.
		/// </summary>
		[RelayCommand]
		public void SaveData()
		{
			// check if there is data to be saved
			if (_valuesAccX?.Count > 0 && _valuesAccY?.Count > 0 && _valuesAccZ?.Count > 0 &&
			   _valuesGyroX?.Count > 0 && _valuesGyroY?.Count > 0 && _valuesGyroZ?.Count >0 &&
			   _valuesFSR?.Count > 0)
			{
				// save the data (not here -> dummy)
				// add the function(s) to save the data to the InfluxDB
				_influxDBService?.SaveData(_valuesAccX.ToArray(), _valuesAccY.ToArray(), _valuesAccZ.ToArray(),
										   _valuesGyroX.ToArray(), _valuesGyroY.ToArray(), _valuesGyroZ.ToArray(), 
										   _valuesFSR.ToArray(), SelectedLabel, SelectedProject);

				// clear the data (no need for them anymore as they have been saved)
				ClearData();

				// enable the start/stop button
				StartStopActive = true;

				// increment the selected label and index
				_selectedLabelIndex = (_selectedLabelIndex + 1) % _labels.Count;
				SelectedLabel = _labels[_selectedLabelIndex];

				// show a snackbar message
				OpenSnackbar(ControlAppearance.Success, "Data saved successfully!", "The data has been successfully saved.", SymbolRegular.Checkmark24);
			}
		}

		/// <summary>
		/// Command for scrapping the data.
		/// </summary>
		[RelayCommand]
		public void ScrapData()
		{
			// clear the data
			ClearData();

			// show a snackbar message
			OpenSnackbar(ControlAppearance.Caution, "Data scrapped!", "The data has been scrapped.", SymbolRegular.Delete24);

			// re-enable the start/stop button
			StartStopActive = true;
		}

		/// <summary>
		/// Internal function for clearing the data collections.
		/// </summary>
		private void ClearData()
		{
			_valuesAccX?.Clear();
			_valuesAccY?.Clear();
			_valuesAccZ?.Clear();
			_valuesGyroX?.Clear();
			_valuesGyroY?.Clear();
			_valuesGyroZ?.Clear();
			_valuesFSR?.Clear();

			// disable the save and scrap buttons
			HasData = false;
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
		public void OnNavigatedFrom() {}

		/// <summary>
		/// Initialize the viewmodel.
		/// </summary>
		private void InitializeViewModel()
        {
			_isInitialized = true;
        }

		/// <summary>
		/// Event handler for the DataReceived event from the TCP client service for data labeling.
		/// </summary>
		/// <param name="data"></param>
		private void TcpClientService_OnDataReceived(byte[] data)
		{
			// add the received data to the collections
			_valuesAccX?.Add(new(data[1]));
			_valuesAccY?.Add(new(data[2]));
			_valuesAccZ?.Add(new(data[3]));
			_valuesGyroX?.Add(new(data[4]));
			_valuesGyroY?.Add(new(data[5]));
			_valuesGyroZ?.Add(new(data[6]));
			_valuesFSR?.Add(new(data[7]));
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