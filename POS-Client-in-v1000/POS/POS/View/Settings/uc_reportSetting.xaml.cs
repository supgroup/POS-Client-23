using netoaster;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS.View.Settings
{
    /// <summary>
    /// Interaction logic for uc_reportSetting.xaml
    /// </summary>
    public partial class uc_reportSetting : UserControl
    {
        private static uc_reportSetting _instance;
        public static uc_reportSetting Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_reportSetting();
                return _instance;
            }
        }
        public uc_reportSetting()
        {
            InitializeComponent();
        }
        // preint parameters

        SetValues setvalueModel = new SetValues();
        List<SetValues> replangList = new List<SetValues>();
        SetValues replangrow = new SetValues();
        static SettingCls setModel = new SettingCls();
        static SettingCls set = new SettingCls();
        static SetValues valueModel = new SetValues();
        static SetValues printCount = new SetValues();
        static int printCountId = 0;
        List<SetValues> printList = new List<SetValues>();

        SetValues show_header_row = new SetValues();
        string show_header;
        SetValues itemtax_note_row = new SetValues();
        string itemtax_note;
        SetValues sales_invoice_note_row = new SetValues();
        string sales_invoice_note;
        
             SetValues invoice_langrow = new SetValues();

        public class Replang
        {
            public int langId { get; set; }
            public string lang { get; set; }
            public string trlang { get; set; }
            public Nullable<int> isDefault { get; set; }

        }
        List<Replang> langcomboList = new List<Replang>();
        List<Replang> langinvcomboList = new List<Replang>();
        async Task fillRepLang()
        {
            langcomboList = new List<Replang>();

           // replangList = await setvalueModel.GetBySetName("report_lang");
            SettingCls setm = FillCombo.settingsCls.Where(s => s.name == "report_lang").FirstOrDefault();
            replangList = FillCombo.settingsValues.Where(i => i.settingId == setm.settingId).ToList();

            foreach (var reprow in replangList)
            {
                //  trEnglish resourcemanager.GetString("trMenu");
                //trArabic
                Replang comborow = new Replang();
                comborow.langId = reprow.valId;
                comborow.lang = reprow.value;

                if (reprow.value == "ar")
                {
                    comborow.trlang = MainWindow.resourcemanager.GetString("trArabic");
                }
                else if (reprow.value == "en")
                {
                    comborow.trlang = MainWindow.resourcemanager.GetString("trEnglish");
                }
                else
                {
                    comborow.trlang = "";
                }

                langcomboList.Add(comborow);
            }
            cb_reportlang.ItemsSource = langcomboList;
            cb_reportlang.DisplayMemberPath = "trlang";
            cb_reportlang.SelectedValuePath = "langId";
            replangrow = replangList.Where(r => r.isDefault == 1).FirstOrDefault();
            cb_reportlang.SelectedValue = replangrow.valId;
        }
        async Task fillInvoiceLang()
        {
            langinvcomboList = new List<Replang>();
            //    cb_invlang
            // replangList = await setvalueModel.GetBySetName("report_lang");

            langinvcomboList.Add(new Replang { lang = "en", trlang = MainWindow.resourcemanager.GetString("trEnglish") });
            langinvcomboList.Add(new Replang { lang = "ar", trlang = MainWindow.resourcemanager.GetString("trArabic") });
            langinvcomboList.Add(new Replang { lang = "both", trlang = MainWindow.resourcemanager.GetString("trEnglish") + "-"+ MainWindow.resourcemanager.GetString("trArabic") });
 
            SettingCls setm = FillCombo.settingsCls.Where(s => s.name == "invoice_lang").FirstOrDefault();
            invoice_langrow = FillCombo.settingsValues.Where(i => i.settingId == setm.settingId).FirstOrDefault();

       
            cb_invlang.ItemsSource = langinvcomboList;
            cb_invlang.DisplayMemberPath = "trlang";
            cb_invlang.SelectedValuePath = "lang";

            cb_invlang.SelectedValue = invoice_langrow.value;
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);


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
                if (FillCombo.settingsCls == null)
                    await FillCombo.RefreshSettings();

                if (FillCombo.settingsValues == null)
                    await FillCombo.RefreshSettingsValues();

                ///naji code
                ///
                await FillprintList();
                fillPrintHeader();
                await fillRepLang();
                await fillInvoiceLang();
                #region get default print count
                await getDefaultPrintCount();
                if (printCount != null)
                    tb_printCount.Text = printCount.value;
                #endregion

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
        public async Task FillprintList()
        {
            printList = FillCombo.settingsValues.Where(i => i.notes == "print").ToList();

        //    printList = await setvalueModel.GetBySetvalNote("print");

        }
        private void translate()
        {
            txt_printCount.Text = MainWindow.resourcemanager.GetString("trPrintCount");
            tt_printCount.Content = MainWindow.resourcemanager.GetString("trPrintCount");

            txt_reportlang.Text = MainWindow.resourcemanager.GetString("reportsLanguage");
            txt_invlang.Text = MainWindow.resourcemanager.GetString("invoicesLanguage");

            txt_systmSetting.Text = MainWindow.resourcemanager.GetString("trDirectPrinting");
            txt_systmSettingHint.Text = MainWindow.resourcemanager.GetString("trDirectPrintingHint") + "...";

            txt_printerSetting.Text = MainWindow.resourcemanager.GetString("defaultPrinters");
            txt_printerSettingHint.Text = MainWindow.resourcemanager.GetString("trPrinterSettingsHint") + "...";

            txt_copyCount.Text = MainWindow.resourcemanager.GetString("trCopyCount");
            txt_copyCountHint.Text = MainWindow.resourcemanager.GetString("trCopyCountHint") + "...";

            txt_printCount.Text = MainWindow.resourcemanager.GetString("trPrintCount");
            txt_printHeader.Text = MainWindow.resourcemanager.GetString("trPrintHeader");
            txt_itemsTaxNote.Text= MainWindow.resourcemanager.GetString("trItemsTax");
            txt_itemsTaxNoteHint.Text = MainWindow.resourcemanager.GetString("trItemsTaxNote");
            txt_salesInvoiceNote.Text = MainWindow.resourcemanager.GetString("trSalesInvoice");
            txt_salesInvoiceNoteHint.Text = MainWindow.resourcemanager.GetString("trSalesInvoiceNote");

            txt_posPrinters.Text = MainWindow.resourcemanager.GetString("trPrinterSettings");
            txt_posPrintersHint.Text = MainWindow.resourcemanager.GetString("trAddUpdateDelete");
        }
        private int fillPrintHeader()
        {
            cb_printHeader.DisplayMemberPath = "Text";
            cb_printHeader.SelectedValuePath = "Value";
            var typelist = new[] {
                 new { Text = MainWindow.resourcemanager.GetString("trShowOption")       , Value = "1" },
                 new { Text = MainWindow.resourcemanager.GetString("trHide")       , Value = "0" },
                };
            cb_printHeader.ItemsSource = typelist;
            cb_printHeader.SelectedIndex = 0;

            getshowHeader();
            if (show_header_row.value == "1")
            {
                cb_printHeader.SelectedIndex = 0;
            }
            else
            {
                cb_printHeader.SelectedIndex = 1;
            }
            return 1;


        }
        public static async Task<SetValues> getDefaultPrintCount()
        {
            //List<SettingCls> settingsCls = await setModel.GetAll();
            //List<SetValues> settingsValues = await valueModel.GetAll();
           


            List<SettingCls> settingsCls = FillCombo.settingsCls;
            List<SetValues> settingsValues = FillCombo.settingsValues;
            set = settingsCls.Where(s => s.name == "Allow_print_inv_count").FirstOrDefault();
            printCountId = set.settingId;
            printCount = settingsValues.Where(i => i.settingId == printCountId).FirstOrDefault();

            return printCount;

        }

        public string getshowHeader()
        {
            SettingCls setls = FillCombo.settingsCls.Where(s => s.name == "show_header").FirstOrDefault();
            show_header_row = FillCombo.settingsValues.Where(X => X.settingId == setls.settingId).FirstOrDefault();
            show_header = show_header_row.value;
            return show_header;

        }
        //     printList = await setvalueModel.GetBySetvalNote("print");

        private void Btn_systmSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                //{
                Window.GetWindow(this).Opacity = 0.2;
                wd_reportSystmSetting w = new wd_reportSystmSetting();
                w.windowType = "r";
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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

        private void Btn_copyCount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                //if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                //{
                Window.GetWindow(this).Opacity = 0.2;
                wd_reportCopyCountSetting w = new wd_reportCopyCountSetting();
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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


        private async void Btn_printCount_Click(object sender, RoutedEventArgs e)
        {//save print count
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.validateEmptyTextBox(tb_printCount, p_errorPrintCount, tt_errorPrintCount, "trEmptyPrintCount");
                if (!tb_printCount.Text.Equals(""))
                {

                    if (printCount == null)
                        printCount = new SetValues();
                    printCount.value = tb_printCount.Text;
                    printCount.isSystem = 1;
                    printCount.isDefault = 1;
                    printCount.settingId = printCountId;

                    int s = (int)await valueModel.Save(printCount);
                    if (!s.Equals(0))
                    {
                        //update tax in main window
                        AppSettings.Allow_print_inv_count = printCount.value;

                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
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
        private void Btn_printerSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                //if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                //{
                Window.GetWindow(this).Opacity = 0.2;
                wd_reportPrinterSetting w = new wd_reportPrinterSetting();
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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

        private async void Btn_saveReportlang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //  string msg = "";
                int msg = 0;
                if (cb_reportlang.SelectedIndex != -1)
                {
                    replangrow = replangList.Where(r => r.valId == (int)cb_reportlang.SelectedValue).FirstOrDefault();
                    replangrow.isDefault = 1;
                    msg = (int)await setvalueModel.Save(replangrow);
                    //  replangrow.valId=
                    //await MainWindow.GetReportlang();
                    AppSettings.Reportlang = replangrow.value;
                    //  FillCombo.settingsValues.Where(r => r.valId == replangrow.valId).ToList().ForEach(r => r = replangrow);

                    if (msg > 0)
                    {
                        await FillCombo.RefreshSettingsValues();
                        await fillRepLang();
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                    }
                    else
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                }
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
        private async void Btn_saveinvlang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                int msg = 0;
                if (cb_invlang.SelectedIndex != -1)
                {
                    invoice_langrow.value =  (string)cb_invlang.SelectedValue ;
                   // replangrow.isDefault = 1;
                    msg = (int)await setvalueModel.Save(invoice_langrow);
                    //  replangrow.valId=
                    //await MainWindow.GetReportlang();
                    AppSettings.invoice_lang = invoice_langrow.value;
                    //  FillCombo.settingsValues.Where(r => r.valId == replangrow.valId).ToList().ForEach(r => r = replangrow);

                    if (msg > 0)
                    {
                        await FillCombo.RefreshSettingsValues();
                        await fillInvoiceLang();
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                    }
                    else
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }

                }

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
        private void Tb_digit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^1-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Tb_PreventSpaces(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = e.Key == Key.Space;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_validateEmptyLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void validateEmpty(string name, object sender)
        {//validate
            try
            {
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_printCount")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorPrintCount, tt_errorPrintCount, "trEmptyPrintCount");
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_reportlang")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorReportlang, tt_errorReportlang, "trEmptyLanguage");
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #region Numeric

        private int _numValue = 0;

        public int NumValue
        {
            get { return _numValue; }
            set
            {
                _numValue = value;
                tb_printCount.Text = value.ToString();
            }
        }
        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NumValue++;

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NumValue > 1)
                    NumValue--;

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion

        private async void Btn_savePrintHeader_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //  SectionData.validateEmptyComboBox(cb_serverStatus, p_errorServerStatus, tt_errorServerStatus, "trEmptyServerStatus");
                if (!cb_printHeader.Text.Equals(""))
                {

                    int res = 0;
                    show_header_row.value = cb_printHeader.SelectedValue.ToString();
                    res = (int)await setvalueModel.Save(show_header_row);

                    //   int res = await progDetailsModel.updateIsonline(isOnline);


                    if (res > 0)
                    {
                        AppSettings.show_header = show_header_row.value;
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);

                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                  await  FillCombo.RefreshSettingsValues();
                    await FillprintList();
                    fillPrintHeader();
                    await MainWindow.Getprintparameter();
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

        private async void Btn_itemsTaxNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;

                wd_notes w = new wd_notes();
                w.maxLength = 200;
                //   w.note = "Test note...";
               // itemtax_note_row = printList.Where(X => X.name == "itemtax_note").FirstOrDefault();
                SettingCls tmpset = FillCombo.settingsCls.Where(s => s.name == "itemtax_note").FirstOrDefault();
                itemtax_note_row = printList.Where(i => i.settingId == tmpset.settingId).FirstOrDefault();

                itemtax_note = itemtax_note_row.value;
            
                w.note=itemtax_note;
                w.ShowDialog();
                if (w.isOk)
                {
                   // MessageBox.Show(w.note);
                    //save
                    int res = 0;
                    itemtax_note_row.value = w.note.Trim();
                    res = (int)await setvalueModel.Save(itemtax_note_row);



                    if (res > 0)
                    {
                        AppSettings.itemtax_note = itemtax_note_row.value;
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);

                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                    await FillCombo.RefreshSettingsValues();
                    await FillprintList();
                  
                    await MainWindow.Getprintparameter();


                }

                Window.GetWindow(this).Opacity = 1;
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                    Window.GetWindow(this).Opacity = 1;
                SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Btn_salesInvoiceNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;

                wd_notes w = new wd_notes();
                w.maxLength = 3000;


             //   sales_invoice_note_row = printList.Where(X => X.name == "sales_invoice_note").FirstOrDefault();
             SettingCls   tmpset = FillCombo.settingsCls.Where(s => s.name == "sales_invoice_note").FirstOrDefault();
                sales_invoice_note_row = printList.Where(i => i.settingId == tmpset.settingId).FirstOrDefault();
                sales_invoice_note = sales_invoice_note_row.value;
                w.note = sales_invoice_note;
                w.ShowDialog();
                if (w.isOk)
                {
                    // MessageBox.Show(w.note);
                    //save
                    int res = 0;
                    sales_invoice_note_row.value = w.note.Trim();
                    res = (int)await setvalueModel.Save(sales_invoice_note_row);
                    if (res > 0)
                    {
                        AppSettings.sales_invoice_note = sales_invoice_note_row.value;
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);

                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                
                    await FillCombo.RefreshSettingsValues();
                    await FillprintList();

                    await MainWindow.Getprintparameter();

                }

                Window.GetWindow(this).Opacity = 1;
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                    Window.GetWindow(this).Opacity = 1;
                SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_posPrinters_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;

                wd_posPrinters w = new wd_posPrinters();
                w.ShowDialog();
                //if (w.isOk)
                //{

                //}
                Window.GetWindow(this).Opacity = 1;
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
