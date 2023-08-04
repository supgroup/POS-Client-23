using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class replyTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int msgMainId = 0;
                if (values[0] != null)
                    msgMainId = (int)values[0];
                string title = (string)values[1];

                if(msgMainId != 0)
                {
                    title = MainWindow.resourcemanager.GetString("reply") + ": " + title;
                }
                return title;
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
