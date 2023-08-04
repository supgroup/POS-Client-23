using POS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
     class showDateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //<Binding Path = "isReaded" />
                //<Binding Path = "updateDate" />

                bool isReaded = false;
                bool.TryParse(values[0].ToString(),out isReaded);


                if (isReaded)
                    return SectionData.dateFrameConverter((DateTime) values[1]);
                else
                    return "-";
            }
            catch
            {
                return "";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
