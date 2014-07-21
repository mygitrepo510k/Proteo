using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Cirrious.CrossCore.Converters;
using System.Globalization;

namespace MWF.Mobile.Core.ValueConverters
{
    public class StringHasLengthConverter : MvxValueConverter<string, bool>
    {
        protected override bool Convert(string value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return (value.Length > 0);
        }
    }
}