using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Data;

namespace ManutdNews.Services
{
    public class StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
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

            fixedString = Regex.Replace(fixedString, "&#8220;", "“");
            fixedString = Regex.Replace(fixedString, "&#8221;", "”");
            return fixedString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
