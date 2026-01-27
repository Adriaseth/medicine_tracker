using System.Globalization;

namespace medicine_tracker.Converters;

public sealed class BooleanToTakenColorsConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var kind = parameter?.ToString();
		if (value is true)
		{
			return kind switch
			{
				"Border" => Color.FromArgb("#16A34A"),
				"Background" => Color.FromArgb("#DCFCE7"),
				"Text" => Color.FromArgb("#166534"),
				_ => Color.FromArgb("#DCFCE7")
			};
		}

		return Colors.Transparent;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
