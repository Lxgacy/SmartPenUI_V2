using LiveChartsCore.Defaults;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.SkiaSharpView;

namespace SmartPenUI_V2.Models
{
	public partial class DataLabelingModel : ObservableObject
	{
		public readonly ObservableCollection<ObservableValue>? valuesAccX = [];
		public readonly ObservableCollection<ObservableValue>? valuesAccY = [];
		public readonly ObservableCollection<ObservableValue>? valuesAccZ = [];

		public readonly ObservableCollection<ObservableValue>? valuesGyroX = [];
		public readonly ObservableCollection<ObservableValue>? valuesGyroY = [];
		public readonly ObservableCollection<ObservableValue>? valuesGyroZ = [];

		public readonly ObservableCollection<ObservableValue>? valuesFSR = [];

		// properties for the chart series
		public ObservableCollection<ISeries>? AccSeries { get; set; } = [];
		public ObservableCollection<ISeries>? GyroSeries { get; set; } = [];
		public ObservableCollection<ISeries>? PressureSeries { get; set; } = [];

		public DataLabelingModel()
		{
			// Initialize the LineSeries for Acceleration X, Y, Z
			AccSeries.Add(new LineSeries<ObservableValue> { Values = valuesAccX, Fill = null, Name = "X" });
			AccSeries.Add(new LineSeries<ObservableValue> { Values = valuesAccY, Fill = null, Name = "Y" });
			AccSeries.Add(new LineSeries<ObservableValue> { Values = valuesAccZ, Fill = null, Name = "Z" });

			// Initialize the LineSeries for Gyro (Angular Velocity) X, Y, Z
			GyroSeries.Add(new LineSeries<ObservableValue> { Values = valuesGyroX, Fill = null, Name = "X" });
			GyroSeries.Add(new LineSeries<ObservableValue> { Values = valuesGyroY, Fill = null, Name = "Y" });
			GyroSeries.Add(new LineSeries<ObservableValue> { Values = valuesGyroZ, Fill = null, Name = "Z" });

			PressureSeries.Add(new LineSeries<ObservableValue> { Values = valuesFSR, Fill = null, Name = "FSR" });
		}
	}
}
