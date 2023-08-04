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

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_returnInvoice.xaml
    /// </summary>
    public partial class wd_returnInvoice : Window
    {
        public Invoice invoice;
        public int userId;      
        public bool fromPurchase = false;
        public string invoiceType;
        bool _IsFocused = false;
        public List<Control> controls;
        DateTime _lastKeystroke = new DateTime(0);
        static private string _BarcodeStr = "";
        Invoice invoiceModel = new Invoice();
        public wd_returnInvoice()
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

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                string barcode = tb_invoiceNum.Text;
                await dealWithBarcode(barcode);               
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
        private async void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_IsFocused)
                {
                    Control c = CheckActiveControl();
                    if (c == null)
                        tb_invoiceNum.Focus();
                    _IsFocused = true;
                }
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 150)
                {
                    _BarcodeStr = "";
                }
                string digit = "";
                // record keystroke & timestamp 
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    //digit pressed!         
                    digit = e.Key.ToString().Substring(1);
                    // = "1" when D1 is pressed
                }
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                {
                    digit = e.Key.ToString().Substring(6); // = "1" when NumPad1 is pressed
                }
                else if (e.Key >= Key.A && e.Key <= Key.Z)
                    digit = e.Key.ToString();
                else if (e.Key == Key.OemMinus)
                {
                    digit = "-";
                }
                _BarcodeStr += digit;
                _lastKeystroke = DateTime.Now;
                // process barcode

                if (e.Key.ToString() == "Return" && _BarcodeStr != "")
                {
                    await dealWithBarcode(_BarcodeStr);
                   
                    tb_invoiceNum.Text = _BarcodeStr;
                    _BarcodeStr = "";
                    _IsFocused = false;
                    e.Handled = true;
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
        public Control CheckActiveControl()
        {
            for (int i = 0; i < controls.Count; i++)
            {
                Control c = controls[i];
                if (c.IsFocused)
                {
                    return c;
                }
            }
            return null;
        }
        private async Task dealWithBarcode(string barcode)
        {
            int codeindex = barcode.IndexOf("-");
            string prefix = "";
            if (codeindex >= 0)
                prefix = barcode.Substring(0, codeindex);
            prefix = prefix.ToLower();
            barcode = barcode.ToLower();
 
            if (prefix == "pi" && fromPurchase)
            {
                invoice = await invoiceModel.GetInvoicesByBarcodeAndUser(barcode,MainWindow.userID.Value, MainWindow.branchID.Value);                
            }
            else if(prefix == "si" && !fromPurchase)
            {
                invoice = await invoiceModel.GetInvoicesByBarcodeAndUser(barcode, MainWindow.userID.Value, MainWindow.branchID.Value);
            }
            if (invoice == null) // check if agent invoice number
            {
                invoice = await invoiceModel.getInvoiceByNumAndUser(invoiceType ,barcode, MainWindow.userID.Value);
            }
            if (invoice != null)
            {
                DialogResult = true;
                this.Close();
            }
            else
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoInvoice"), animation: ToasterAnimation.FadeIn);
            tb_invoiceNum.Clear();
            tb_invoiceNum.Focus();
        }
        private  void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.KeyDown += HandleKeyPress;
            try
            {
                tb_invoiceNum.Focus();
                if (sender != null)
                    SectionData.StartAwait(grid_main);            
                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                }

                translate();
                #endregion

                controls = new List<Control>();
                FindControl(this.grid_main, controls);
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
        public void FindControl(DependencyObject root, List<Control> controls)
        {
            controls.Clear();
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var control = current as Control;
                if (control != null && control.IsTabStop)
                {
                    controls.Add(control);
                }
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child != null)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trReturn");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSelect");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_invoiceNum, MainWindow.resourcemanager.GetString("trInvoiceNumber"));
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_IsFocused)
                {
                    Control c = CheckActiveControl();
                    if (c == null)
                        tb_invoiceNum.Focus();
                    _IsFocused = true;
                }
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 150)
                {
                    _BarcodeStr = "";
                }
                string digit = "";
                // record keystroke & timestamp 
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    //digit pressed!         
                    digit = e.Key.ToString().Substring(1);
                    // = "1" when D1 is pressed
                }
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                {
                    digit = e.Key.ToString().Substring(6); // = "1" when NumPad1 is pressed
                }
                else if (e.Key >= Key.A && e.Key <= Key.Z)
                    digit = e.Key.ToString();
                else if (e.Key == Key.OemMinus)
                {
                    digit = "-";
                }
                _BarcodeStr += digit;
                _lastKeystroke = DateTime.Now;
                // process barcode

                if (e.Key.ToString() == "Return" && _BarcodeStr != "")
                {
                    await dealWithBarcode(_BarcodeStr);

                    tb_invoiceNum.Text = _BarcodeStr;
                    _BarcodeStr = "";
                    _IsFocused = false;
                    e.Handled = true;
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

        private void space_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            SectionData.InputJustNumber(ref textBox);
            e.Handled = e.Key == Key.Space;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.KeyDown -= HandleKeyPress;
        }

        private async void Tb_invoiceNum_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (e.Key == Key.Return)
                {
                    string barcode = tb_invoiceNum.Text;
                    await dealWithBarcode(barcode);
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
    }
}
