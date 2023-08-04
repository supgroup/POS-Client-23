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

using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace POS.View.sectionData.Charts
{
    /// <summary>
    /// Interaction logic for lvcVendor.xaml
    /// </summary>
    public partial class win_lvc : Window
    {
        int selectedChart = 1;
        IEnumerable<Agent> agentsQuery;
        IEnumerable<Bank> banksQuery;
        IEnumerable<Bonds> bondsQuery;
        IEnumerable<User> usersQuery;
        IEnumerable<Branch> branchesQuery;
        IEnumerable<Pos> possQuery;
        IEnumerable<CashTransfer> cashQuery;
        IEnumerable<Card> cardsQuery;
        IEnumerable<Taxes> taxesQuery;
        IEnumerable<ShippingCompanies> shippingCompanyQuery;
        IEnumerable<Invoice> invoiceQuery;
        List<double> chartList;
        List<double> PiechartList;
        List<double> ColumnchartList;
        int sectionDate;
        bool isSameList;
        string label;
 
        public SeriesCollection SeriesCollection { get; set; }
        Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

        public win_lvc(IEnumerable<Agent> _agentsQuery, int _sectionDate, bool _isSameList)
        {
            try
            {
                InitializeComponent();
                agentsQuery = _agentsQuery;
                sectionDate = _sectionDate;
                isSameList = _isSameList;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public win_lvc(IEnumerable<Bank> _banksQuery, int _sectionDate)
        {
            try
            {
                InitializeComponent();
                banksQuery = _banksQuery;
                sectionDate = _sectionDate;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public win_lvc(IEnumerable<Bonds> _bondsQuery, int _sectionDate)
        {
            try
            {
                InitializeComponent();
                bondsQuery = _bondsQuery;
                sectionDate = _sectionDate;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public win_lvc(IEnumerable<CashTransfer> _cashQuery, int _sectionDate)
        {
            try
            {
                InitializeComponent();
                cashQuery = _cashQuery;
                sectionDate = _sectionDate;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public win_lvc(IEnumerable<User> _usersQuery, int _sectionDate)
        {
            try
            {
                InitializeComponent();
                usersQuery = _usersQuery;
                sectionDate = _sectionDate;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public win_lvc(IEnumerable<Branch> _branchesQuery, int _sectionDate, bool _isSameList)
        {
            try
            {
                InitializeComponent();
                branchesQuery = _branchesQuery;
                sectionDate = _sectionDate;
                isSameList = _isSameList;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public win_lvc(IEnumerable<Pos> _possQuery, int _sectionDate)
        {
            try
            {
                InitializeComponent();
                possQuery = _possQuery;
                sectionDate = _sectionDate;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public win_lvc(IEnumerable<Invoice> _invoiceQuery, int _sectionDate)
        {
            try
            {
                InitializeComponent();
                invoiceQuery = _invoiceQuery;
                sectionDate = _sectionDate;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public win_lvc(IEnumerable<Card> _cardsQuery, int _sectionDate)
        {
            try
            {
                InitializeComponent();
                cardsQuery = _cardsQuery;
                sectionDate = _sectionDate;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
       public win_lvc(IEnumerable<Taxes> _taxesQuery, int _sectionDate)
        {
            try
            {
                InitializeComponent();
                taxesQuery = _taxesQuery;
                sectionDate = _sectionDate;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public win_lvc(IEnumerable<ShippingCompanies> _shippingCompanyQuery, int _sectionDate)
        {
            try
            {
                InitializeComponent();
                shippingCompanyQuery = _shippingCompanyQuery;
                sectionDate = _sectionDate;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
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
            catch (Exception)
            { }
        }

        public void fillChart()
        {
            chartList.Clear();
            MyAxis.Labels = new List<string>();
            int startYear = dpStrtDate.SelectedDate.Value.Year;
            int endYear = dpEndDate.SelectedDate.Value.Year;
            int startMonth = dpStrtDate.SelectedDate.Value.Month;
            int endMonth = dpEndDate.SelectedDate.Value.Month;

            if (rdoMonth.IsChecked == true)//month
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        //var firstOfThisMonth = new DateTime(year, month, dpStrtDate.SelectedDate.Value.Day);
                        DateTime firstOfThisMonth = DateTime.Now ;
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

                            if (sectionDate == 1)
                            {
                                var Draw = agentsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                if (isSameList)
                                {
                                //label = "Customers count";
                                label = MainWindow.resourcemanager.GetString("trCustomers");
                                }
                                else //label = "Vendors count";
                                label = MainWindow.resourcemanager.GetString("trVendors");
                            }
                            else if (sectionDate == 2)
                            {
                                var Draw = banksQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                //label = "Banks count";
                                label = MainWindow.resourcemanager.GetString("trBanks");
                            }
                            else if (sectionDate == 3)
                            {
                                var Draw = usersQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                //label = "Users count";
                                label = MainWindow.resourcemanager.GetString("trUsers");
                            }
                            else if (sectionDate == 4)
                            {
                                var Draw = branchesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                if (isSameList)
                                {
                                    //label = "Branches count";
                                    label = MainWindow.resourcemanager.GetString("trBranches");
                                }
                                else //label = "Stores count";
                                label = MainWindow.resourcemanager.GetString("trStores");
                            }
                            else if (sectionDate == 5)
                            {
                                var Draw = possQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                //label = "Pos count";
                                label = MainWindow.resourcemanager.GetString("trPOSs");
                            }
                            else if (sectionDate == 6)
                            {
                                var Draw = bondsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                //label = "Bondes count";
                                label = MainWindow.resourcemanager.GetString("trBondesCount");
                            }
                            else if (sectionDate == 7)
                            {
                                var Draw = invoiceQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                //label = "Invoice count";
                                label = MainWindow.resourcemanager.GetString("trOrdersCount");
                            }
                            else if (sectionDate == 8)
                            {
                                var Draw = cashQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                //label = "CashTransfer count";
                                label = MainWindow.resourcemanager.GetString("trCashtransferCount");
                            }
                            else if (sectionDate == 9)
                            {
                                var Draw = cardsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                //label = "Cards count";
                                label = MainWindow.resourcemanager.GetString("trCardsCount");
                            }
                            else if (sectionDate == 10)
                            {
                                var Draw = shippingCompanyQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                                chartList.Add(Draw);
                                //label = "Shipping Companies count";
                                label = MainWindow.resourcemanager.GetString("trShippingCompaniesCount");
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
            else //year
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    var firstOfThisYear = new DateTime(year, 1, dpStrtDate.SelectedDate.Value.Month);
                    var firstOfNextMYear = firstOfThisYear.AddYears(1);
                    if (sectionDate == 1)
                    {
                        var Draw = agentsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        if (isSameList)
                        {
                            //label = "Customers count";
                            label = MainWindow.resourcemanager.GetString("trCustomersCount");
                        }
                        else //label = "Vendors count";
                            label = MainWindow.resourcemanager.GetString("trVendorsCount");
                    }
                    else if (sectionDate == 2)
                    {
                        var Draw = banksQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Banks count";
                        label = MainWindow.resourcemanager.GetString("trBanksCount");
                    }
                    else if (sectionDate == 3)
                    {
                        var Draw = usersQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Users count";
                        label = MainWindow.resourcemanager.GetString("trUsersCount");
                    }
                    else if (sectionDate == 4)
                    {
                        var Draw = branchesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        if (isSameList)
                        {
                            //label = "Branches count";
                            label = MainWindow.resourcemanager.GetString("trBranchesCount");
                        }
                        else //label = "Stores count";
                            label = MainWindow.resourcemanager.GetString("trStoresCount");
                    }
                    else if (sectionDate == 5)
                    {
                        var Draw = possQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Pos count";
                        label = MainWindow.resourcemanager.GetString("trPosCount");
                    }
                    else if (sectionDate == 6)
                    {
                        var Draw = bondsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Bondes count";
                        label = MainWindow.resourcemanager.GetString("trBonds");
                    }
                    else if (sectionDate == 7)
                    {
                        var Draw = invoiceQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Bondes count";
                        label = MainWindow.resourcemanager.GetString("trOrders");
                    }
                    else if (sectionDate == 8)
                    {
                        var Draw = cashQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Cashtransfer count";
                        label = MainWindow.resourcemanager.GetString("trCashtransferCount");
                    }
                    else if (sectionDate == 9)
                    {
                        var Draw = cardsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Cards count";
                        label = MainWindow.resourcemanager.GetString("trCardsCount");
                    }
                    else if (sectionDate == 10)
                    {
                        var Draw = shippingCompanyQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        chartList.Add(Draw);
                        //label = "Shipping Companies count";
                        label = MainWindow.resourcemanager.GetString("trShippingCompaniesCount");
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

            if (rdoMonth.IsChecked == true)//month
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
                        if (sectionDate == 1)
                        {
                            var Draw = agentsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            if (isSameList)
                            {
                                //label = "Customers count";
                                label = MainWindow.resourcemanager.GetString("trCustomersCount");
                            }
                            else //label = "Vendors count";
                                label = MainWindow.resourcemanager.GetString("trVendorsCount");
                        }
                        else if (sectionDate == 2)
                        {
                            var Draw = banksQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Banks count";
                            label = MainWindow.resourcemanager.GetString("trBanksCount");
                        }
                        else if (sectionDate == 3)
                        {
                            var Draw = usersQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Users count";
                            label = MainWindow.resourcemanager.GetString("trUsersCount");
                        }
                        else if (sectionDate == 4)
                        {
                            var Draw = branchesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            if (isSameList)
                            {
                                //label = "Branches count";
                                label = MainWindow.resourcemanager.GetString("trBranchesCount");
                            }
                            else //label = "Stores count";
                                label = MainWindow.resourcemanager.GetString("trStoresCount");
                        }
                        else if (sectionDate == 5)
                        {
                            var Draw = possQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Pos count";
                            label = MainWindow.resourcemanager.GetString("trPosCount");
                        }
                        else if (sectionDate == 6)
                        {
                            var Draw = bondsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Bonde count";
                            label = MainWindow.resourcemanager.GetString("trBondesCount");
                        }
                        else if (sectionDate == 7)
                        {
                            var Draw = invoiceQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Invoice count";
                            label = MainWindow.resourcemanager.GetString("trOredersCount");
                        }
                        else if (sectionDate == 8)
                        {
                            var Draw = cashQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Cashtransfer count";
                            label = MainWindow.resourcemanager.GetString("trCashtransfers");
                        }
                        else if (sectionDate == 9)
                        {
                            var Draw = cardsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Cards count";
                            label = MainWindow.resourcemanager.GetString("trCardsCount");
                        }
                        else if (sectionDate == 10)
                        {
                            var Draw = shippingCompanyQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            PiechartList.Add(Draw);
                            //label = "Shipping Companies count";
                            label = MainWindow.resourcemanager.GetString("trShippingCompaniesCount");
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
            else //year
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    var firstOfThisYear = new DateTime(year, 1, dpStrtDate.SelectedDate.Value.Month);
                    var firstOfNextMYear = firstOfThisYear.AddYears(1);
                    if (sectionDate == 1)
                    {
                        var Draw = agentsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        if (isSameList)
                        {
                            //label = "Customers count";
                            label = MainWindow.resourcemanager.GetString("trCustomersCount");
                        }
                        else //label = "Vendors count";
                            label = MainWindow.resourcemanager.GetString("trVendorsCount");
                    }
                    else if (sectionDate == 2)
                    {
                        var Draw = banksQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Banks count";
                        label = MainWindow.resourcemanager.GetString("trBanksCount");
                    }
                    else if (sectionDate == 3)
                    {
                        var Draw = usersQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Users count";
                        label = MainWindow.resourcemanager.GetString("trUsersCount");
                    }
                    else if (sectionDate == 4)
                    {
                        var Draw = branchesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        if (isSameList)
                        {
                            //label = "Branches count";
                            label = MainWindow.resourcemanager.GetString("trBranchesCount");
                        }
                        else //label = "Stores count";
                            label = MainWindow.resourcemanager.GetString("trStoresCount");
                    }
                    else if (sectionDate == 5)
                    {
                        var Draw = possQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Pos count";
                        label = MainWindow.resourcemanager.GetString("trPosCount");
                    }
                    else if (sectionDate == 6)
                    {
                        var Draw = bondsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Bonde count";
                        label = MainWindow.resourcemanager.GetString("trBondesCount");
                    }
                    else if (sectionDate == 7)
                    {
                        var Draw = invoiceQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Invoice count";
                        label = MainWindow.resourcemanager.GetString("trOrdersCount");
                    }
                    else if (sectionDate == 8)
                    {
                        var Draw = cashQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Cashtrasfer count";
                        label = MainWindow.resourcemanager.GetString("trCashtransferCount");
                    }
                    else if (sectionDate == 9)
                    {
                        var Draw = cardsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Cards count";
                        label = MainWindow.resourcemanager.GetString("trCards");
                    }
                     else if (sectionDate == 10)
                    {
                        var Draw = shippingCompanyQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        PiechartList.Add(Draw);
                        //label = "Shipping Companies count";
                        label = MainWindow.resourcemanager.GetString("trShippingCompanies");
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
                        if (sectionDate == 1)
                        {
                            var Draw = agentsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            if (isSameList)
                            {
                                //label = "Customers count";
                                label = MainWindow.resourcemanager.GetString("trCustomers");
                            }
                            else //label = "Vendors count";
                                label = MainWindow.resourcemanager.GetString("trVendors");


                        }
                        else if (sectionDate == 2)
                        {
                            var Draw = banksQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Banks count";
                            label = MainWindow.resourcemanager.GetString("trBanks");
                        }
                        else if (sectionDate == 3)
                        {
                            var Draw = usersQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Users count";
                            label = MainWindow.resourcemanager.GetString("trUsers");
                        }
                        else if (sectionDate == 4)
                        {
                            var Draw = branchesQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            if (isSameList)
                            {
                                //label = "Branches count";
                                label = MainWindow.resourcemanager.GetString("trBranches");
                            }
                            else //label = "Stores count";
                                label = MainWindow.resourcemanager.GetString("trStores");
                        }
                        else if (sectionDate == 5)
                        {
                            var Draw = possQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Pos count";
                            label = MainWindow.resourcemanager.GetString("trPOSs");
                        }
                        else if (sectionDate == 6)
                        {
                            var Draw = bondsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Bonde count";
                            label = MainWindow.resourcemanager.GetString("trBonds");
                        }
                        else if (sectionDate == 7)
                        {
                            var Draw = invoiceQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Invoice count";
                            label = MainWindow.resourcemanager.GetString("trOrders");
                        }
                        else if (sectionDate == 8)
                        {
                            var Draw = cashQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Cashtrasfer count";
                            label = MainWindow.resourcemanager.GetString("trCashtransfers");
                        }
                        else if (sectionDate == 9)
                        {
                            var Draw = cardsQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Cards count";
                            label = MainWindow.resourcemanager.GetString("trCards");
                        }
                        else if (sectionDate == 10)
                        {
                            var Draw = shippingCompanyQuery.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Count();
                            ColumnchartList.Add(Draw);
                            //label = "Shipping Companies count";
                            label = MainWindow.resourcemanager.GetString("trShippingCompanies");
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
                    if (sectionDate == 1)
                    {
                        var Draw = agentsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        if (isSameList)
                        {
                            //label = "Customers count";
                            label = MainWindow.resourcemanager.GetString("trCustomers");
                        }
                        else //label = "Vendors count";
                            label = MainWindow.resourcemanager.GetString("trVendors");
                    }
                    else if (sectionDate == 2)
                    {
                        var Draw = banksQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Banks count";
                        label = MainWindow.resourcemanager.GetString("trBanks");
                    }
                    else if (sectionDate == 3)
                    {
                        var Draw = usersQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Users count";
                        label = MainWindow.resourcemanager.GetString("trUsers");
                    }
                    else if (sectionDate == 4)
                    {
                        var Draw = branchesQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        if (isSameList)
                        {
                            //label = "Branches count";
                            label = MainWindow.resourcemanager.GetString("trBranches");
                        }
                        else //label = "Stores count";
                            label = MainWindow.resourcemanager.GetString("trStores");
                    }
                    else if (sectionDate == 5)
                    {
                        var Draw = possQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Pos count";
                        label = MainWindow.resourcemanager.GetString("trPOSs");
                    }
                    else if (sectionDate == 6)
                    {
                        var Draw = bondsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Bonde count";
                        label = MainWindow.resourcemanager.GetString("trBonds");
                    }
                    else if (sectionDate == 7)
                    {
                        var Draw = invoiceQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Invoice count";
                        label = MainWindow.resourcemanager.GetString("trOrders");
                    }
                    else if (sectionDate == 8)
                    {
                        var Draw = cashQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Cashtrasfer count";
                        label = MainWindow.resourcemanager.GetString("trCashtransfers");
                    }
                    else if (sectionDate == 9)
                    {
                        var Draw = cardsQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Cards count";
                        label = MainWindow.resourcemanager.GetString("trCards");
                    }
                    else if (sectionDate == 10)
                    {
                        var Draw = shippingCompanyQuery.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Count();
                        ColumnchartList.Add(Draw);
                        //label = "Shipping Companies count";
                        label = MainWindow.resourcemanager.GetString("trCards");
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
                selectedChart = 1;
                rdoMonth.IsChecked = true;
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
