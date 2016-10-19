using System;
using System.Collections.Generic;

using CsvHelper.TypeConversion;

namespace UserGroupsCsvToJson
{
    public class SubsidiaryConverter : ITypeConverter
    {
        public string ConvertToString(TypeConverterOptions options, object value)
        {
            var intList = (IEnumerable<int>)value;
            return string.Join("|", intList);
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            throw new NotImplementedException();
        }

        public bool CanConvertTo(Type type)
        {
            return true;
        }

        public bool CanConvertFrom(Type type)
        {
            throw new NotImplementedException();
        }
    }
}