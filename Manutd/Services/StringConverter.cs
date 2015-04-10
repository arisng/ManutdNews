using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Text.RegularExpressions;

namespace Manutd.Services
{
    public class StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string fixedString = "";

            // convert &quot; -> "
            fixedString = Regex.Replace(value.ToString(), "&quot;", "\"");

            // convert &amp; -> &
            fixedString = Regex.Replace(fixedString, "&amp;", "&");

            // convert &rdquo; -> "
            fixedString = Regex.Replace(fixedString, "&rdquo;", "\"");

            // convert &rdquo; -> ”
            fixedString = Regex.Replace(fixedString, "&ldquo;", "\"");

            // convert &rsquo; -> ’
            fixedString = Regex.Replace(fixedString, "&rsquo;", "'");

            // convert &rdquo; -> -
            fixedString = Regex.Replace(fixedString, "&ndash;", "");

            // convert &euro -> €
            fixedString = Regex.Replace(fixedString, "&euro;", "€");

            // convert &euro -> ""
            fixedString = Regex.Replace(fixedString, "&nbsp;", "");

            return fixedString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
