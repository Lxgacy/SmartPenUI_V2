using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Policy;
using System.Windows.Media.Animation;
using System.Net.Sockets;
using SmartPenUI_V2.Services;
using SmartPenUI_V2.ViewModels;

namespace SmartPenUI_V2.Services
{
	public enum CommandList : byte
	{
		StartSampling = 0x01,
		StopSampling = 0x02,
		StartPrediction = 0x03,
		StopPrediction = 0x04,
		Init = 0x05,
		InitSuccess = 0x06,
		Reset = 0x07,
		Label = 0x08,
		SensorData = 0x09,
		FSRData = 0x0A,
		LogMessage = 0x0B // added for logging
	}

	public class DummyTcpClientService
	{
		// Delegate and event for byte data
		public delegate void DataReceivedEventHandler(byte[] data);
		public event DataReceivedEventHandler? DataReceived;

		// Delegate and event for string data
		public delegate void PredictionReceivedEventHandler(string pred);
		public event PredictionReceivedEventHandler? PredictionReceived;

		private Timer? _timer;
		private Timer? _timerPred;

		// properties for test tcp connection --------------------------------
		private TcpClient? _tcpClient;
		private NetworkStream? _networkStream;

		private SettingsViewModel? _settingsViewModel;

		// -------------------------------------------------------------------


		private bool _isConnected = false;
		public bool IsConnected => _isConnected;

		public DummyTcpClientService()
		{
			_timer = new Timer(SimulateDataReception, null, Timeout.Infinite, 2000);
			_timerPred = new Timer(SimulatePredictionReception, null, Timeout.Infinite, 5);

			// load settings view model in order to get the tcp connection settings
			//Initialize the dummy TCP client service
			_settingsViewModel = App.GetService<SettingsViewModel>();
			if (_settingsViewModel == null)
			{
				throw new InvalidOperationException(
					"The DummyTcpClientService is not registered in the service provider.");
			}
			
		}

		public async Task<bool> Connect()
		{
			// Simulate connecting to a server
			if (!_isConnected)
			{
				_tcpClient = new TcpClient();
				if (_settingsViewModel?.TcpPort != null && int.TryParse(_settingsViewModel.TcpPort, out int tcpPort))
				{
					try
					{
						await _tcpClient.ConnectAsync(_settingsViewModel.MdnsName, tcpPort);
					}
					catch (Exception ex)
					{
						return false;
					}
				}

				_networkStream = _tcpClient.GetStream();
				_isConnected = true;

				// Start listening for data asynchronously
				StartListening();
				return true;
			}
			return false;
		}

		public void Disconnect()
		{
			// Simulate disconnecting from a server
			if (_isConnected)
			{
				_networkStream?.Close();
				_tcpClient?.Close();
				_isConnected = false;
			}
		}

		private async void StartListening()
		{
			var buffer = new byte[8];
			while (_isConnected)
			{
				try
				{
					int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
					if (bytesRead > 0)
					{
						byte[] receivedData = new byte[bytesRead];
						Array.Copy(buffer, receivedData, bytesRead);

						if (receivedData[0] == (byte)CommandList.SensorData)
							OnDataReceived(receivedData);
						else if (receivedData[0] == (byte)CommandList.Label)
							OnPredictionReceived(Encoding.ASCII.GetString(receivedData, 1, receivedData.Length - 1));
					}
				}
				catch
				{
					Disconnect(); // Disconnect on error
					break;
				}
			}
		}

		public void StartLabeling()
		{
			//_timer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
			SendCommand(CommandList.StartSampling);
		}

		public void StopLabeling()
		{
			//_timer?.Change(Timeout.Infinite, 0);
			SendCommand(CommandList.StopSampling);
		}

		private async void SendCommand(CommandList cmd)
		{
			if (!_isConnected || _networkStream == null) return;

			// Prepare the command to be sent
			byte[] command = [(byte)cmd];

			try
			{
				// Send the command to the server
				await _networkStream.WriteAsync(command, 0, command.Length);
			}
			catch (Exception ex)
			{
				// Handle any errors that occur during send
				Console.WriteLine($"Error sending command to server: {ex.Message}");
				Disconnect(); // Consider disconnecting or handling the error differently
			}
		}

		public void StartPredicting()
		{
			// doesnt do anything here since this is a dummy service
			SendCommand(CommandList.StartPrediction);
		}

		public void StopPredicting()
		{
			// make a dummy prediction after 1 second
			//_timerPred?.Change(1500, Timeout.Infinite);
			SendCommand(CommandList.StopPrediction);
		}

		private void SimulateDataReception(object? state)
		{
			if (!_isConnected) return;

			// Simulate receiving random data
			Random random = new Random();
			byte[] dummyData = new byte[7];
			random.NextBytes(dummyData);

			OnDataReceived(dummyData);
		}

		private void SimulatePredictionReception(object? state)
		{
			if (!_isConnected) return;

			// Generate a random capital letter
			Random random = new Random();
			char randomLetter = (char)random.Next('A', 'Z' + 1); // 'Z' + 1 because the upper bound is exclusive

			string dummyPrediction = randomLetter.ToString();

			OnPredictionReceived(dummyPrediction);
		}

		protected virtual void OnDataReceived(byte[] data)
		{
			DataReceived?.Invoke(data);
		}

		protected virtual void OnPredictionReceived(string pred)
		{
			PredictionReceived?.Invoke(pred);
		}
	}

}
