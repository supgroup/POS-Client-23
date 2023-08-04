using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
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
using Microsoft.Win32;
using Microsoft.Reporting.WinForms;
using System.IO;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_serialNumData.xaml
    /// </summary>
    public partial class wd_serialNumData : Window
    {
        public wd_serialNumData()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region variables
        string basicsPermission = "serial_basics";
        string soldPermission = "serial_sold";
        Serial serial = new Serial();
        public IEnumerable<Serial> serials;
        List<Serial> serialsQuery = new List<Serial>();
        List<Serial> serialsList;
        public int itemunitId = 0;
        public long count = 0;
        public long quantity = 0;
        public string item = "";
        public string unit = "";
        public bool isOk = false;
        string searchText = "";

        #endregion

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
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

                txt_item.Text = item;
                txt_unit.Text = unit;
                await Search();

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region methods
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trSerial");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_serialNum, MainWindow.resourcemanager.GetString("trSerialNumHint"));

            dg_serials.Columns[1].Header = MainWindow.resourcemanager.GetString("trItem");

            txt_itemDetails.Text = MainWindow.resourcemanager.GetString("itemDetails");
            txt_serials.Text = MainWindow.resourcemanager.GetString("trSerials");

            txt_itemTitle.Text = MainWindow.resourcemanager.GetString("trItem");
            txt_itemUnitTitle.Text = MainWindow.resourcemanager.GetString("trUnit");
            txt_quantityTitle.Text = MainWindow.resourcemanager.GetString("trQuantity");
            txt_serialsCountTitle.Text = MainWindow.resourcemanager.GetString("trCount");

            btn_add.Content = MainWindow.resourcemanager.GetString("trAdd");
            btn_update.Content = MainWindow.resourcemanager.GetString("trUpdate");
            btn_delete.Content = MainWindow.resourcemanager.GetString("trDelete");

            tt_sold.Content = MainWindow.resourcemanager.GetString("sold_");
            tt_attach.Content = MainWindow.resourcemanager.GetString("trImport");

            txt_from.Text = MainWindow.resourcemanager.GetString("trFrom");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_close.Content = MainWindow.resourcemanager.GetString("trClose");

        }
        async Task Search()
        {
            if(serialsList is null)
            await RefreshSerialsList();

            searchText = txb_search.Text.ToLower();
            serialsQuery = serialsList.Where(s => s.serialNum.ToLower().Contains(searchText)).ToList();
            RefreshSerialView();
        }
        async Task<IEnumerable<Serial>> RefreshSerialsList()
        {
            serialsList = await serial.GetSerialsAvailable(MainWindow.branchID.Value, itemunitId);
            foreach (var s in serialsList)
            {
                s.isSelected = true;
            }
            return serialsList;
        }
        void RefreshSerialView()
        {
            dg_serials.ItemsSource = serialsQuery;
            txt_sum2.Text = dg_serials.Items.Count.ToString();
            txt_sum.Text = dg_serials.Items.Count.ToString();
            txt_serialsCount.Text = dg_serials.Items.Count.ToString();
            txt_quantity.Text = quantity.ToString();

        }
        private async Task<bool> chkDuplicate()
        {
            if (serialsQuery.Any(s => s.serialNum == tb_serialNum.Text && s.serialId != serial.serialId))
                return true;
            else
                return false;
        }
        private void Clear()
        {
            this.DataContext = new Serial();

            SectionData.clearValidate(tb_serialNum, p_errorSerialNum);
        }
        #endregion

        #region events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {//closing

            try
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
                this.isOk = true;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {//unload
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {//close
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void preview_TextBox(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == ",")
                    e.Handled = true;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void space_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            SectionData.InputJustNumber(ref textBox);
            e.Handled = e.Key == Key.Space;
        }
        private void Chb_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsFocused)
            {
                var serialsLst = new List<Serial>();

                if (dg_serials.ItemsSource != null)
                    serialsLst = (List<Serial>)dg_serials.ItemsSource;

                txt_sum.Text = serialsLst.Where(s => s.isSelected).Count().ToString();

                dg_serials.ItemsSource = null;
                dg_serials.ItemsSource = serialsLst;
            }
        }
        private void Chb_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsFocused)
            {
                var serialsLst = new List<Serial>();

                if (dg_serials.ItemsSource != null)
                    serialsLst = (List<Serial>)dg_serials.ItemsSource;

                txt_sum.Text = serialsLst.Where(s => s.isSelected).Count().ToString();

                dg_serials.ItemsSource = null;
                dg_serials.ItemsSource = serialsLst;
            }
        }
        private void Dg_serials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dg_serials.SelectedIndex != -1)
                {
                    serial = dg_serials.SelectedItem as Serial;
                    this.DataContext = serial;
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

        #endregion

        #region add - update - delete - sold - attach
        private async void Btn_sold_Click(object sender, RoutedEventArgs e)
        {//sold
            try
            {
                if (serialsQuery.Count > 0)
                {
                    //SectionData.StartAwait(grid_main);

                    if (MainWindow.groupObject.HasPermissionAction(soldPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                    {
                        #region old
                        //#region validate
                        ////chk empty serialNum
                        //SectionData.validateEmptyTextBox(tb_serialNum, p_errorSerialNum, tt_errorSerialNum, "trIsRequired");
                        //#endregion

                        //if (!tb_serialNum.Text.Equals(""))
                        //{
                        //    serial.isSold = true;

                        //    int s = (int)await serial.Save(serial);

                        //    if (s > 0)
                        //    {
                        //        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                        //        Clear();
                        //    }
                        //    else
                        //        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        //    await Search();
                        //}
                        #endregion
                        List<Serial> serialsQueryForSale = new List<Serial>();
                        //serialsQueryForSale = serialsQuery.Where(s => s.isSelected == true).ToList();
                        foreach (var s in serialsQuery)
                        {
                            if (s.isSelected)
                            {
                                s.isSold = true;
                                serialsQueryForSale.Add(s);
                            }
                        }
                        if (serialsQueryForSale.Count > 0)
                        {
                            int res = (int)await serial.UpdateSerialsList(serialsQueryForSale);
                            if (res > 0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSold"), animation: ToasterAnimation.FadeIn);
                                Clear();
                                await RefreshSerialsList();
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            await Search();
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("noSelectedSerials"), animation: ToasterAnimation.FadeIn);
                }
                    else
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                    //SectionData.EndAwait(grid_main);
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    serial.serialId = 0;

                    #region validate
                    //chk empty serialNum
                    SectionData.validateEmptyTextBox(tb_serialNum, p_errorSerialNum, tt_errorSerialNum, "trIsRequired");
                    #endregion

                    if (!tb_serialNum.Text.Equals(""))
                    {
                   
                        serial.serialNum = tb_serialNum.Text;
                        serial.itemUnitId = itemunitId;
                        serial.isActive = 1;
                        serial.createUserId = MainWindow.userID;
                        serial.updateUserId = MainWindow.userID;
                        serial.isSold = false;
                        serial.branchId = MainWindow.branchID;
                        serial.isManual = true;/////????????????

                        double s = (double)await serial.Save(serial);

                        if (s == -2.2)
                        {
                            SectionData.showTextBoxValidate(tb_serialNum, p_errorSerialNum, tt_errorSerialNum, "trDuplicateCodeToolTip");
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trDuplicateCodeToolTip"), animation: ToasterAnimation.FadeIn);
                        }
                        else if (s == -3.3)
                        {
                            SectionData.showTextBoxValidate(tb_serialNum, p_errorSerialNum, tt_errorSerialNum, "overQuantity");
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("overQuantity"), animation: ToasterAnimation.FadeIn);
                        }
                        else if (s > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            Clear();
                            await RefreshSerialsList();
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        await Search();

                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {//update
            try
            {
                if (serial.serialId > 0)
                {
                    SectionData.StartAwait(grid_main);
               
                    if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                    {
                        #region validate
                        //chk empty serialNum
                        SectionData.validateEmptyTextBox(tb_serialNum, p_errorSerialNum, tt_errorSerialNum, "trIsRequired");

                        #endregion

                        if (!tb_serialNum.Text.Equals(""))
                        {

                            serial.serialNum = tb_serialNum.Text;
                            serial.isActive = 1;
                            serial.createUserId = MainWindow.userID;
                            serial.updateUserId = MainWindow.userID;
                            serial.isSold = false;
                            serial.branchId = MainWindow.branchID;
                            serial.isManual = true;/////????????????

                            double s = (double)await serial.Save(serial);

                            if (s == -2.2)
                            {
                                SectionData.showTextBoxValidate(tb_serialNum, p_errorSerialNum, tt_errorSerialNum, "trDuplicateCodeToolTip");
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trDuplicateCodeToolTip"), animation: ToasterAnimation.FadeIn);
                            }
                            //else if (s == -3)
                            //{
                            //    //SectionData.showTextBoxValidate(tb_serialNum, p_errorSerialNum, tt_errorSerialNum, "overQuantity");
                            //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("overQuantity"), animation: ToasterAnimation.FadeIn);
                            //}
                            else if (s > 0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                                Clear();
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            await RefreshSerialsList();
                            await Search();
                        }
                    }
                    else
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                
                SectionData.EndAwait(grid_main);
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {//delete
            try
            {
                if (serial.serialId > 0)
                {
                    SectionData.StartAwait(grid_main);

                    if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                    {
                        if (serial.serialId != 0)
                        {
                            #region
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion

                            if (w.isOk)
                            {
                                string popupContent = "";
                                popupContent = MainWindow.resourcemanager.GetString("trPopDelete");

                                int b = (int)await serial.Delete(serial.serialId, MainWindow.userID.Value, true);

                                if (b > 0)
                                {
                                    serial.serialId = 0;
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: popupContent, animation: ToasterAnimation.FadeIn);
                                }
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }

                            Clear();

                            await RefreshSerialsList();
                            await Search();
                        }

                    }
                    else
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);


                    SectionData.EndAwait(grid_main);
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        OpenFileDialog openFileDialog = new OpenFileDialog();
        private async void Btn_attach_Click(object sender, RoutedEventArgs e)
        {//attach
            try
            {
                SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(soldPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    int iuId = itemunitId;
                    int userId = MainWindow.userLogin.userId;
                    int branchId = MainWindow.loginBranch.branchId;
                    //show dialog
                    // restore
                    string filepath = "";
                    ExcelCls exlclass = new ExcelCls();
                    List<string> stringserial = new List<string>();
                    int message = 0;
                    int serialCount = 0;
                    openFileDialog.Filter = "EXCEL|*.xls;*.xlsx;*.csv; ";
                    //  BackupCls back = new BackupCls();
                    if (openFileDialog.ShowDialog() == true)
                    {
                        #region
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                        w.contentText = MainWindow.resourcemanager.GetString("trContinueProcess?");
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                        #endregion
                        if (w.isOk)
                        {
                            // here start restore if user click yes button Mr. Yasin //////////////////////////////////////////////////////
                            filepath = openFileDialog.FileName;
                            stringserial = exlclass.readFile(filepath);
                            //check duplicat
                            List<string> serialnoduplicate = stringserial.GroupBy(x => x).Select(x => x.Key).ToList();
                            serialCount = serialnoduplicate.Count;
                            if (serialCount > 0)
                            {
                                // save serials
                                Serial serialobj = new Serial();
                                serialobj.serialId = 0;
                                serialobj.itemUnitId = iuId;
                                serialobj.createUserId = userId;
                                serialobj.updateUserId = userId;
                                serialobj.branchId = branchId;
                                serialobj.isActive = 1;
                                serialobj.isSold = false;
                                //send list
                                message = await serialobj.SaveSerialsList(serialnoduplicate, serialobj);

                            }
                            //   .Where(g => g.Count() > 1)

                            //  string message = "";
                            if (message > 0 && message == serialCount)
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCompleted"), animation: ToasterAnimation.FadeIn);
                            else
                            {
                                string resmsg = MainWindow.resourcemanager.GetString("saved") + " " + message.ToString() + " " + MainWindow.resourcemanager.GetString("of") + " " + serialCount.ToString();
                                Toaster.ShowWarning(Window.GetWindow(this), message: resmsg, animation: ToasterAnimation.FadeIn);

                            }


                        }
                        else
                        {
                            // here if user click no button

                        }
                    }
                    //end show
                    //

                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        #endregion

        #region report
     
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        private void pdf()
        {
            BuildReport();


            saveFileDialog.Filter = "PDF|*.pdf;";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filepath = saveFileDialog.FileName;
                LocalReportExtensions.ExportToPDF(rep, filepath);
            }

        }

        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Store\Ar\ArSerial.rdlc";
            }
            else
            {
                addpath = @"\Reports\Store\En\Serial.rdlc";
            }
          
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            //    IEnumerable<Slice> slicesQuery;
            clsReports.SerialReport(serialsQuery, rep, reppath, paramarr);
            paramarr.Add(new ReportParameter("Quantity", txt_quantity.Text));
            paramarr.Add(new ReportParameter("sCount",txt_serialsCount.Text));
            paramarr.Add(new ReportParameter("Item", txt_item.Text));
            paramarr.Add(new ReportParameter("Unit", txt_unit.Text));

            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    pdf();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    /////////////////////////////////////
                    //Thread t1 = new Thread(() =>
                    //{
                    print();
                    //});
                    //t1.Start();
                    //////////////////////////////////////
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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
        {//pie

        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    excel();
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    Window.GetWindow(this).Opacity = 0.2;
                    /////////////////////
                    string pdfpath = "";
                    pdfpath = @"\Thumb\report\temp.pdf";
                    pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                    BuildReport();
                    LocalReportExtensions.ExportToPDF(rep, pdfpath);
                    ///////////////////
                    wd_previewPdf w = new wd_previewPdf();
                    w.pdfPath = pdfpath;
                    if (!string.IsNullOrEmpty(w.pdfPath))
                    {
                        w.ShowDialog();
                        w.wb_pdfWebViewer.Dispose();
                    }
                    Window.GetWindow(this).Opacity = 1;
                    //////////////////////////////////////
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void print()
        {
            BuildReport();


            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

        }
        private void excel()
        {
            BuildReport();

            saveFileDialog.Filter = "EXCEL|*.xls;";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filepath = saveFileDialog.FileName;
                LocalReportExtensions.ExportToExcel(rep, filepath);
            }



        }



    

        //OpenFileDialog openFileDialog = new OpenFileDialog();
        //private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        //{
        //    if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
        //    {

        //    }
        //    else
        //        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

        //}
        //private void Btn_print_Click(object sender, RoutedEventArgs e)
        //{
        //    if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
        //    {

        //    }
        //    else
        //        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

        //}
        //private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        //{
        //    if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
        //    {

        //    }
        //    else
        //        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

        //}
        //private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        //{
        //    if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
        //    {

        //    }
        //    else
        //        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

        //}
        //private void Btn_preview_Click(object sender, RoutedEventArgs e)
        //{
        //    if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
        //    {

        //    }
        //    else
        //        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

        //}

        #endregion

        private async void Txb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
               await Search();
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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch //(Exception ex)
            {
                //SectionData.ExceptionMessage(ex, this);
            }
        }
    }
}
