using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace SmartPenUI_V2.Helpers;
public class ConnectionStatusToIconConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool isConnected)
		{
			var icon = isConnected ? new SymbolIcon(SymbolRegular.PlugDisconnected20) : new SymbolIcon(SymbolRegular.PlugConnected20);

			// change the color of the icon
			icon.Foreground = isConnected ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.LightGreen);
			return icon;
		}
		return null;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
