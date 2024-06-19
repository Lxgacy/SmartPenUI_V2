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

		private readonly Random _random = new();
		private readonly ObservableCollection<ObservableValue>? _valuesAccX = [];
		private readonly ObservableCollection<ObservableValue>? _valuesAccY = [];
		private readonly ObservableCollection<ObservableValue>? _valuesAccZ = [];

		private readonly ObservableCollection<ObservableValue>? _valuesGyroX = [];
		private readonly ObservableCollection<ObservableValue>? _valuesGyroY = [];
		private readonly ObservableCollection<ObservableValue>? _valuesGyroZ = [];

		private readonly ObservableCollection<ObservableValue>? _valuesFSR = [];

		public ObservableCollection<ISeries>? AccSeries { get; set; } = [];
		public ObservableCollection<ISeries>? GyroSeries { get; set; } = [];
		public ObservableCollection<ISeries>? PressureSeries { get; set; } = [];

		// snackbar properties
		private ControlAppearance _snackbarAppearance = ControlAppearance.Success;
		[ObservableProperty]
		private int _snackbarTimeout = 3;
		private ISnackbarService _snackbarService;

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
				_tcpClientService.DataReceived += TcpClientService_OnDataReceived;
			}

			// Initialize the dummy InfluxDB service
			_influxDBService = App.GetService<DummyInfluxDBService>();
			if (_influxDBService == null)
			{
				throw new InvalidOperationException(
					"The DummyTcpClientService is not registered in the service provider.");
			}

			_snackbarService = App.GetService<ISnackbarService>();
		}

		[RelayCommand]
		public void ConnectDisconnectTCP()
		{
			if(IsConnected)
			{
				_tcpClientService?.Disconnect();
				ClearData();

				if(IsLabeling)
					StartStopLabeling();
				OpenSnackbar(ControlAppearance.Caution, "Disconnected successfully.", "The connection to the TCP server has been successfully terminated.", SymbolRegular.ErrorCircle24);
			}
			else
			{
				_tcpClientService?.Connect();
				OpenSnackbar(ControlAppearance.Success, "Connected successfully!", "The connection to the TCP server has been successfully established.", SymbolRegular.Checkmark24);
			}

			IsConnected = _tcpClientService?.IsConnected ?? false;

			StartStopActive = IsConnected;
		}

		[RelayCommand]
		public void PreviousLabel()
		{
			_selectedLabelIndex = (_selectedLabelIndex - 1 + _labels.Count) % _labels.Count;
			SelectedLabel = _labels[_selectedLabelIndex];
		}

		[RelayCommand]
		public void StartStopLabeling()
		{
			if (IsConnected)
			{
				if (IsLabeling)
				{
					_tcpClientService?.StopLabeling();
					IsLabeling = false;
					StartStopActive = false;
					// check if there is data to be saved
					if (_valuesAccX?.Count > 0 && _valuesAccY?.Count > 0 && _valuesAccZ?.Count > 0 &&
						_valuesGyroX?.Count > 0 && _valuesGyroY?.Count > 0 && _valuesGyroZ?.Count > 0 &&
						_valuesFSR?.Count > 0)
					{
						HasData = true;
					}
				}
				else
				{
					_tcpClientService?.StartLabeling();
					IsLabeling = true;
				}
			}

			
		}

		[RelayCommand]
		public void SaveData()
		{
			// check if there is data to be saved
			if (_valuesAccX?.Count > 0 && _valuesAccY?.Count > 0 && _valuesAccZ?.Count > 0 &&
			   _valuesGyroX?.Count > 0 && _valuesGyroY?.Count > 0 && _valuesGyroZ?.Count >0 &&
			   _valuesFSR?.Count > 0)
			{
				// save the data (not here -> dummy)

				// clear the data (no need for them anymore)
				ClearData();

				StartStopActive = true;
				_selectedLabelIndex = (_selectedLabelIndex + 1) % _labels.Count;
				SelectedLabel = _labels[_selectedLabelIndex];
				OpenSnackbar(ControlAppearance.Success, "Data saved successfully!", "The data has been successfully saved.", SymbolRegular.Checkmark24);
			}
		}

		[RelayCommand]
		public void ScrapData()
		{
			ClearData();
			OpenSnackbar(ControlAppearance.Caution, "Data scrapped!", "The data has been scrapped.", SymbolRegular.Delete24);
			StartStopActive = true;
		}

		private void ClearData()
		{
			_valuesAccX?.Clear();
			_valuesAccY?.Clear();
			_valuesAccZ?.Clear();
			_valuesGyroX?.Clear();
			_valuesGyroY?.Clear();
			_valuesGyroZ?.Clear();
			_valuesFSR?.Clear();
			HasData = false;
		}

		public void OnNavigatedTo()
		{
			if (!_isInitialized)
			{
				InitializeViewModel();
			}
		}

		public void OnNavigatedFrom() {}

        private void InitializeViewModel()
        {
			_isInitialized = true;
        }

		private void TcpClientService_OnDataReceived(byte[] data)
		{
			_valuesAccX?.Add(new(data[0]));
			_valuesAccY?.Add(new(data[1]));
			_valuesAccZ?.Add(new(data[2]));
			_valuesGyroX?.Add(new(data[3]));
			_valuesGyroY?.Add(new(data[4]));
			_valuesGyroZ?.Add(new(data[5]));
			_valuesFSR?.Add(new(data[6]));
		}

		private void OpenSnackbar(ControlAppearance appearance, string title, string message, SymbolRegular icon)
		{
			_snackbarService.Show(
				title,
				message,
				appearance,
				new SymbolIcon(icon),
				TimeSpan.FromSeconds(SnackbarTimeout)
			);
		}
	}
}