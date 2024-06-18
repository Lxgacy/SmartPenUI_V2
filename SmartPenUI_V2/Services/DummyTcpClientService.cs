using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Policy;

namespace SmartPenUI_V2.Services
{
	public class DummyTcpClientService
	{
		public delegate void DataReceivedEventHandler(byte[] data);
		public event DataReceivedEventHandler? DataReceived;

		private Timer? _timer;
		private bool _isConnected = false;

		public bool IsConnected => _isConnected;

		public DummyTcpClientService()
		{
			_timer = new Timer(SimulateDataReception, null, Timeout.Infinite, 2000);
		}

		public void Connect()
		{
			// Simulate connecting to a server
			_isConnected = true;
			// Start simulating data reception every 2 seconds after connecting
			_timer?.Change(TimeSpan.Zero, TimeSpan.FromSeconds(2));

		}

		public void Disconnect()
		{
			// Simulate disconnecting from a server
			_isConnected = false;
			// Stop the timer when disconnecting
			_timer?.Change(Timeout.Infinite, 0);
			//_timer?.Dispose();
		}

		private void SimulateDataReception(object? state)
		{
			if (!_isConnected) return;

			// Simulate receiving random data
			Random random = new Random();
			byte[] dummyData = new byte[1];
			random.NextBytes(dummyData);

			OnDataReceived(dummyData);
		}

		protected virtual void OnDataReceived(byte[] data)
		{
			DataReceived?.Invoke(data);
		}
	}

}
