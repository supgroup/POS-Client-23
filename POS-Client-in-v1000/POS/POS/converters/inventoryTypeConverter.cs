using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class inventoryTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string s = value as string;
                switch (value)
                {
                    case "a": return MainWindow.resourcemanager.GetString("trArchived");
                    //break;
                    case "n": return MainWindow.resourcemanager.GetString("trSaved");
                    //break;
                    case "d": return MainWindow.resourcemanager.GetString("trDraft");
                    //break;
                    default: return value;
                        //break;
                }
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
