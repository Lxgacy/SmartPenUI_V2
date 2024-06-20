using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SmartPenUI_V2.Helpers
{
    class BoolToThicknessConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// If the value is true, return a Thickness that represents a visible border
			if (value is bool && (bool)value)
			{
				return new Thickness(2); // Assuming a thickness of 2 for the visible border
			}
			else
			{
				return new Thickness(0); // No border
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
