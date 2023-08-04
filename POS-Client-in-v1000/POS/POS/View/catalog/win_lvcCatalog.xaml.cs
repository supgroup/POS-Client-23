using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using System.Resources;

namespace POS.View.catalog
{
    /// <summary>
    /// Interaction logic for win_lvcCatalog.xaml
    /// </summary>
    public partial class win_lvcCatalog : Window
    {
        int selectedChart = 1;
        IEnumerable<Category> categoriesQuery;
        IEnumerable<Item> itemsQuery;
        IEnumerable<Property> propertiesQuery;
        IEnumerable<StorageCost> storagecostQuery;
        IEnumerable<Unit> unitsQuery;
        IEnumerable<Warranty> warrantiesQuery;
        IEnumerable<Slice> slicesQuery;
        List<double> chartList;
        List<double> PiechartList;
        List<double> ColumnchartList;
        int catalog;
        string label;

        public SeriesCollection SeriesCollection { get; set; }

        public win_lvcCatalog(IEnumerable<Category> _categoriesQuery, int _catalog)
        {
            try
            {
                InitializeComponent();
                categoriesQuery = _categoriesQuery;
                catalog = _catalog;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        public win_lvcCatalog(IEnumerable<Item> _itemsQuery, int _catalog)
        {
            try
            {
                InitializeComponent();
                itemsQuery = _itemsQuery;
                catalog = _catalog;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        public win_lvcCatalog(IEnumerable<Property> _propertiesQuery, int _catalog)
        {
            try
            {
                InitializeComponent();
                propertiesQuery = _propertiesQuery;
                catalog = _catalog;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        public win_lvcCatalog(IEnumerable<StorageCost> _storagecostQuery, int _catalog)
        {
            try
            {
                InitializeComponent();
                storagecostQuery = _storagecostQuery;
                catalog = _catalog;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        public win_lvcCatalog(IEnumerable<Unit> _unitsQuery, int _catalog)
        {
            try
            {
                InitializeComponent();
                unitsQuery = _unitsQuery;
                catalog = _catalog;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        public win_lvcCatalog(IEnumerable<Warranty> _warrantiesQuery, int _warranty)
        {
            try
            {
                InitializeComponent();
                warrantiesQuery = _warrantiesQuery;
                catalog = _warranty;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        public win_lvcCatalog(IEnumerable<Slice> _slicesQuery, int _slice)
        {
            try
            {
                InitializeComponent();
                slicesQuery = _slicesQuery;
                catalog = _slice;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }

                translate();
                #endregion

                chartList = new List<double>();
                PiechartList = new List<double>();
                ColumnchartList = new List<double>();
                fillDates();
                fillSelectedChart();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trReports");
            rdoMonth.Content = MainWindow.resourcemanager.GetString("trMonth");
            rdoYear.Content = MainWindow.resourcemanager.GetString("trYear");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dpStrtDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dpEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            btn_print.Content = MainWindow.resourcemanager.GetString("trPrint");
        }

        public void fillDates()
        {
            dpEndDate.SelectedDate = DateTime.Now;
            dpStrtDate.SelectedDate = dpEndDate.SelectedDate.Value.AddYears(-1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception) { }
        }

        public void fillChart()
        {
            chartList.Clear();
            MyAxis.Labels = new List<string>();
            int startYear = dpStrtDate.SelectedDate.Value.Year;
            int endYear = dpEndDate.SelectedDate.Value.Year;
            int startMonth = dpStrtDate.SelectedDate.Value.Month;
            int endMonth = dpEndDate.SelectedDate.Value.Month;
            if (rdoMonth.IsChecked == true)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        //var firstOfThisMonth = new DateTime(year, month, dpStrtDate.SelectedDate.Value.Day);
                        DateTime firstOfThisMonth = DateTime.Now;
                        try
                        {
                            firstOfThisMonth = new DateTime(year, month, dpStrtDate.SelectedDate.Value.Day);
                        }
                        catch
                        {
                            try
                            {
                                firstOfThisMonth = new DateTime(year, month, 29);
                            }
                            catch
                            {
                                firstOfThisMonth = new DateTime(year, month, 28);
                            }
                        }
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);

                        if (catalog == 1)
                        {
                            var Draw = categoriesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            chartList.Add(Draw);
                            //label = "Categories count";
                            label = MainWindow.resourcemanager.GetString("trCategories");
                        }
                        else if(catalog == 2)
                        {
                            var Draw = itemsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            chartList.Add(Draw);
                            //label = "Items count";
                            label = MainWindow.resourcemanager.GetString("trItems");
                        }
                        else if (catalog == 3)
                        {
                            var Draw = propertiesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            chartList.Add(Draw);
                            //label = "Properties count";
                            label = MainWindow.resourcemanager.GetString("trProperties");
                        }
                        else if (catalog == 4)
                        {
                            var Draw = storagecostQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            chartList.Add(Draw);
                            //label = "Storage Costs count";
                            label = MainWindow.resourcemanager.GetString("trStorageCost");
                        }
                        else if (catalog == 5)
                        {
                            var Draw = unitsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            chartList.Add(Draw);
                            //label = "Units count";
                            label = MainWindow.resourcemanager.GetString("trUnits");
                        }
                        else if (catalog == 6)
                        {
                            var Draw = warrantiesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            chartList.Add(Draw);
                            //label = "Units count";
                            label = MainWindow.resourcemanager.GetString("trWarranties");
                        }
                        MyAxis.Separator.Step = 2;
                        MyAxis.Labels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + "/" + year);
                        if (year == dpEndDate.SelectedDate.Value.Year && month == dpEndDate.SelectedDate.Value.Month)
                        {
                            break;
                        }
                        if (month == 12)
                        {
                            startMonth = 1;
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    var firstOfThisYear = new DateTime(year, 1, dpStrtDate.SelectedDate.Value.Month);

                    var firstOfNextMYear = firstOfThisYear.AddYears(1);
                    if (catalog == 1)
                    {
                        var Draw = categoriesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);

                        //label = "Categories count";
                        label = MainWindow.resourcemanager.GetString("trCategories");
                    }

                    else if (catalog == 2)
                    {
                        var Draw = itemsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Items count";
                        label = MainWindow.resourcemanager.GetString("trItems");
                    }
                    else if (catalog == 3)
                    {
                        var Draw = propertiesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Properties count";
                        label = MainWindow.resourcemanager.GetString("trProperties");
                    }
                    else if (catalog == 4)
                    {
                        var Draw = storagecostQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Storage Costs count";
                        label = MainWindow.resourcemanager.GetString("trStorageCost");
                    }
                    else if (catalog == 5)
                    {
                        var Draw = unitsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Units count";
                        label = MainWindow.resourcemanager.GetString("trUnits");
                    }
                    else if (catalog == 6)
                    {
                        var Draw = warrantiesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Units count";
                        label = MainWindow.resourcemanager.GetString("trWarranties");
                    }
                    MyAxis.Separator.Step = 1;
                    MyAxis.Labels.Add(year.ToString());
                }
            }
            SeriesCollection = new SeriesCollection
           {
                 new LineSeries
               {
                        Title = label,
                   Values = chartList.AsChartValues()
               },
           };
            grid1.Children.Clear();
            grid1.Children.Add(charts);
            DataContext = this;

        }

        public void fillPieChart()
        {

            PiechartList.Clear();
            SeriesCollection piechartData = new SeriesCollection();
            List<string> titles = new List<string>();
            int startYear = dpStrtDate.SelectedDate.Value.Year;
            int endYear = dpEndDate.SelectedDate.Value.Year;
            int startMonth = dpStrtDate.SelectedDate.Value.Month;
            int endMonth = dpEndDate.SelectedDate.Value.Month;
            if (rdoMonth.IsChecked == true)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        //var firstOfThisMonth = new DateTime(year, month, dpStrtDate.SelectedDate.Value.Day);
                        DateTime firstOfThisMonth = DateTime.Now;
                        try
                        {
                            firstOfThisMonth = new DateTime(year, month, dpStrtDate.SelectedDate.Value.Day);
                        }
                        catch
                        {
                            try
                            {
                                firstOfThisMonth = new DateTime(year, month, 29);
                            }
                            catch
                            {
                                firstOfThisMonth = new DateTime(year, month, 28);
                            }
                        }
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                        if (catalog == 1)
                        {
                            var Draw = categoriesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Categories count";
                            label = MainWindow.resourcemanager.GetString("trCategories");
                        }
                        else if (catalog == 2)
                        {
                            var Draw = itemsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Items count";
                            label = MainWindow.resourcemanager.GetString("trItems");
                        }
                        else if (catalog == 3)
                        {
                            var Draw = propertiesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Properties count";
                            label = MainWindow.resourcemanager.GetString("trProperties");
                        }
                        else if (catalog == 4)
                        {
                            var Draw = storagecostQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Storage Costs count";
                            label = MainWindow.resourcemanager.GetString("trStorageCost");
                        }
                        else if (catalog == 5)
                        {
                            var Draw = unitsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Units count";
                            label = MainWindow.resourcemanager.GetString("trUnits");
                        }
                        else if (catalog == 6)
                        {
                            var Draw = warrantiesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Units count";
                            label = MainWindow.resourcemanager.GetString("trWarranties");
                        }
                        titles.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + "/" + year);
                        if (year == dpEndDate.SelectedDate.Value.Year && month == dpEndDate.SelectedDate.Value.Month)
                        {
                            break;
                        }
                        if (month == 12)
                        {
                            startMonth = 1;
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    var firstOfThisYear = new DateTime(year, 1, dpStrtDate.SelectedDate.Value.Month);
                    var firstOfNextMYear = firstOfThisYear.AddYears(1);
                    if (catalog == 1)
                    {
                        var Draw = categoriesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Categories count";
                        label = MainWindow.resourcemanager.GetString("trCategories");
                    }
                    else if (catalog == 2)
                    {
                        var Draw = itemsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Items count";
                        label = MainWindow.resourcemanager.GetString("trItems");
                    }
                    else if (catalog == 3)
                    {
                        var Draw = propertiesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Properties count";
                        label = MainWindow.resourcemanager.GetString("trProperties");
                    }
                    else if (catalog == 4)
                    {
                        var Draw = storagecostQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Storage Costs count";
                        label = MainWindow.resourcemanager.GetString("trStorageCost");
                    }
                    else if (catalog == 5)
                    {
                        var Draw = unitsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Units count";
                        label = MainWindow.resourcemanager.GetString("trUnits");
                    }
                    else if (catalog == 6)
                    {
                        var Draw = warrantiesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Units count";
                        label = MainWindow.resourcemanager.GetString("trWarranties");
                    }
                    titles.Add(year.ToString());
                }
            }
            for (int i = 0; i < PiechartList.Count(); i++)
            {
                List<double> final = new List<double>();
                List<string> lable = new List<string>();
                final.Add(PiechartList.ToList().Skip(i).FirstOrDefault());
                piechartData.Add(
                  new PieSeries
                  {
                      Values = final.AsChartValues(),
                      Title = titles.ToList().Skip(i).FirstOrDefault().ToString(),
                      DataLabels = true,
                  }
              );
            }
            pieChart.Series = piechartData;
        }

        public void fillColumnChart()
        {
            columnAxis.Labels = new List<string>();
            ColumnchartList.Clear();
            SeriesCollection columnchartData = new SeriesCollection();
            List<string> titles = new List<string>();
            int startYear = dpStrtDate.SelectedDate.Value.Year;
            int endYear = dpEndDate.SelectedDate.Value.Year;
            int startMonth = dpStrtDate.SelectedDate.Value.Month;
            int endMonth = dpEndDate.SelectedDate.Value.Month;
            if (rdoMonth.IsChecked == true)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        //var firstOfThisMonth = new DateTime(year, month, dpStrtDate.SelectedDate.Value.Day);
                        DateTime firstOfThisMonth = DateTime.Now;
                        try
                        {
                            firstOfThisMonth = new DateTime(year, month, dpStrtDate.SelectedDate.Value.Day);
                        }
                        catch
                        {
                            try
                            {
                                firstOfThisMonth = new DateTime(year, month, 29);
                            }
                            catch
                            {
                                firstOfThisMonth = new DateTime(year, month, 28);
                            }
                        }
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                        if (catalog == 1)
                        {
                            var Draw = categoriesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Categories count";
                            label = MainWindow.resourcemanager.GetString("trCategories");
                        }
                        else if (catalog == 2)
                        {
                            var Draw = itemsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Items count";
                            label = MainWindow.resourcemanager.GetString("trItems");
                        }
                        else if (catalog == 3)
                        {
                            var Draw = propertiesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Properties count";
                            label = MainWindow.resourcemanager.GetString("trProperties");
                        }
                        else if (catalog == 4)
                        {
                            var Draw = storagecostQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Storage Costs count";
                            label = MainWindow.resourcemanager.GetString("trStorageCost");
                        }
                        else if (catalog == 5)
                        {
                            var Draw = unitsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Units count";
                            label = MainWindow.resourcemanager.GetString("trUnits");
                        }
                        else if (catalog == 6)
                        {
                            var Draw = warrantiesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Units count";
                            label = MainWindow.resourcemanager.GetString("trWarranties");
                        }
                        columnAxis.Separator.Step = 2;
                        columnAxis.Labels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + "/" + year);
                        if (year == dpEndDate.SelectedDate.Value.Year && month == dpEndDate.SelectedDate.Value.Month)
                        {
                            break;
                        }
                        if (month == 12)
                        {
                            startMonth = 1;
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    var firstOfThisYear = new DateTime(year, 1, dpStrtDate.SelectedDate.Value.Month);
                    var firstOfNextMYear = firstOfThisYear.AddYears(1);
                    if (catalog == 1)
                    {
                        var Draw = categoriesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Categories count";
                        label = MainWindow.resourcemanager.GetString("trCategories");
                    }
                    else if (catalog == 2)
                    {
                        var Draw = itemsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Items count";
                        label = MainWindow.resourcemanager.GetString("trItems");
                    }
                    else if (catalog == 3)
                    {
                        var Draw = propertiesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Properties count";
                        label = MainWindow.resourcemanager.GetString("trProperties");
                    }
                    else if (catalog == 4)
                    {
                        var Draw = storagecostQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Storage Costs count";
                        label = MainWindow.resourcemanager.GetString("trstorageCost");
                    }
                    else if (catalog == 5)
                    {
                        var Draw = unitsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Units count";
                        label = MainWindow.resourcemanager.GetString("trUnits");
                    }
                    else if (catalog == 6)
                    {
                        var Draw = warrantiesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Units count";
                        label = MainWindow.resourcemanager.GetString("trWarranties");
                    }
                    columnAxis.Separator.Step = 1;
                    columnAxis.Labels.Add(year.ToString());
                }
            }
            columnchartData.Add(
                 new ColumnSeries
                 {
                     Title = label,
                     Values = ColumnchartList.AsChartValues(),

                 }
             );
            columnChart.Series = columnchartData;
        }

        private void dpStrtDate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dpEndDate.SelectedDate.Value.Year - dpStrtDate.SelectedDate.Value.Year > 1)
                {
                    rdoYear.IsChecked = true;
                    fillSelectedChart();
                }
                else
                {
                    fillSelectedChart();
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void dpEndDate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dpEndDate.SelectedDate.Value.Year - dpStrtDate.SelectedDate.Value.Year > 1)
                {
                    rdoYear.IsChecked = true;
                    fillSelectedChart();
                }
                else
                {
                    fillSelectedChart();
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                rdoMonth.IsChecked = true;
                fillDates();
                selectedChart = 1;
                fillSelectedChart();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void rdoYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dpEndDate.SelectedDate.Value.Year - dpStrtDate.SelectedDate.Value.Year > 1)
                {
                    rdoYear.IsChecked = true;
                    fillSelectedChart();
                }
                else
                {
                    fillSelectedChart();
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void rdoMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dpEndDate.SelectedDate.Value.Year - dpStrtDate.SelectedDate.Value.Year > 1)
                {
                    rdoYear.IsChecked = true;
                    fillSelectedChart();
                }
                else
                {
                    fillSelectedChart();
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void fillSelectedChart()
        {

            grid1.Visibility = Visibility.Hidden;
            grd_pieChart.Visibility = Visibility.Hidden;
            grd_columnChart.Visibility = Visibility.Hidden;

            icon_rowChar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E8E8E8"));
            icon_columnChar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E8E8E8"));
            icon_pieChar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E8E8E8"));

            if (selectedChart == 1)
            {
                grid1.Visibility = Visibility.Visible;
                icon_rowChar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                fillChart();
            }
            else if (selectedChart == 2)
            {
                grd_pieChart.Visibility = Visibility.Visible;
                icon_pieChar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                fillPieChart();
            }
            else if (selectedChart == 3)
            {
                grd_columnChart.Visibility = Visibility.Visible;
                icon_columnChar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                fillColumnChart();

            }

        }

        private void btn_rowChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                selectedChart = 1;
                fillSelectedChart();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                selectedChart = 2;
                fillSelectedChart();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void btn_columnChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                selectedChart = 3;
                fillSelectedChart();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedChart == 1)
                    PrintingChart.Print(charts, this);
                else if (selectedChart == 2)
                    PrintingChart.Print(pieChart, this);
                else if (selectedChart == 3)
                    PrintingChart.Print(columnChart, this);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
    }
}