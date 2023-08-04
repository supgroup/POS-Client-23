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
    class itemTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string s = "";
                switch (value)
                {
                    case "n": s = MainWindow.resourcemanager.GetString("trNormal"); break;
                    case "d": s = MainWindow.resourcemanager.GetString("trHaveExpirationDate"); break;
                    case "sn": s = MainWindow.resourcemanager.GetString("trHaveSerialNumber"); break;
                    case "sr": s = MainWindow.resourcemanager.GetString("trService"); break;
                    case "p": s = MainWindow.resourcemanager.GetString("trPackage"); break;
                }

                return s;
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
