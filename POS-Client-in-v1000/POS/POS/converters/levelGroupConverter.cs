using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    public class levelGroupConverter : IValueConverter
    {
        //ItemUnit itemUnit = new ItemUnit();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                #region
                //new { Text = MainWindow.resourcemanager.GetString("trPercentageDiscount"), Value = "2" },

                var levelsList = new[] {
            new { Text = "المستوى الاول", Value = "1" },
            new { Text = "المستوى الثاني", Value = "2" },
            new { Text = "المستوى الثالث", Value = "3" },
             };
                //cb_level.DisplayMemberPath = "Text";
                //cb_level.SelectedValuePath = "Value";
                //cb_level.ItemsSource = levelsList;
                #endregion
                //return Task.Run(() => itemUnit.GetItemUnits(int.Parse(value.ToString()))).Result;
                return levelsList;
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
