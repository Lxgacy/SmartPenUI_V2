using System;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using System.Windows.Media;

namespace SmartPenUI_V2.Helpers
{
	internal class LabelingStatusToIconConverter : IMultiValueConverter
	{
		public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length >= 2 && values[0] is bool isLabeling && values[1] is bool active)
			{
				var icon = isLabeling ? new SymbolIcon(SymbolRegular.Pause20) : new SymbolIcon(SymbolRegular.Play20);

				// change the color of the icon
				
				if (active)
				{
					icon.Foreground = isLabeling ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.LightGreen);
				}
				else
				{
					// Default or another color for different combinations
					icon.Foreground = new SolidColorBrush(Colors.Gray);
				}

				return icon;
			}
			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
