using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Policy;
using System.Windows.Media.Animation;

namespace SmartPenUI_V2.Services
{
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
		private bool _isConnected = false;

		public bool IsConnected => _isConnected;

		public DummyTcpClientService()
		{
			_timer = new Timer(SimulateDataReception, null, Timeout.Infinite, 2000);
			_timerPred = new Timer(SimulatePredictionReception, null, Timeout.Infinite, 5);
		}

		public void Connect()
		{
			// Simulate connecting to a server
			_isConnected = true;
		}

		public void Disconnect()
		{
			// Simulate disconnecting from a server
			_isConnected = false;
		}

		public void StartLabeling()
		{
			_timer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
		}

		public void StopLabeling()
		{
			_timer?.Change(Timeout.Infinite, 0);
		}

		public void StartPredicting()
		{
			// doesnt do anything here since this is a dummy service
		}

		public void StopPredicting()
		{
			// make a dummy prediction after 1 second
			_timerPred?.Change(1500, Timeout.Infinite);
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
