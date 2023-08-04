using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Reporting.WinForms;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Windows;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
namespace POS.Classes
{
    public class reportSize
    {

        //   public string result { get; set; }
        public string reppath { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        // public string path { get; set; }
        //     public LocalReport rep { get; set; }


        // public Invoice prInvoice { get; set; }
        // public List<ReportParameter> paramarr { get; set; }
        public string printerName { get; set; }
        public string paperSize { get; set; }
    }
    class ReportCls
    {


        List<CurrencyInfo> currencies = new List<CurrencyInfo>();
        public static void clearFolder(string FolderName)
        {
            string filename = "";
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                filename = fi.FullName;

                if (!FileIsLocked(filename) && (fi.Extension == ".PDF" || fi.Extension == ".pdf"))
                {
                    fi.Delete();
                }

            }


        }


        public static bool FileIsLocked(string strFullFileName)
        {
            bool blnReturn = false;
            FileStream fs = null;

            try
            {
                fs = File.Open(strFullFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                fs.Close();
            }
            catch (IOException ex)
            {
                blnReturn = true;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return blnReturn;

        }
        public void Fillcurrency()
        {

            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Kuwait));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Saudi_Arabia));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Oman));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.United_Arab_Emirates));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Qatar));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Bahrain));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Iraq));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Lebanon));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Syria));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Yemen));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Jordan));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Algeria));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Egypt));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Tunisia));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Sudan));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Morocco));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Libya));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Somalia));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Turkey));


        }

        public string PathUp(string path, int levelnum, string addtopath)
        {
            int pos1 = 0;
            levelnum = 0;
            //for (int i = 1; i <= levelnum; i++)
            //{
            //    //pos1 = path.LastIndexOf("\\");
            //    //path = path.Substring(0, pos1);
            //}

            string newPath = path + addtopath;
            try
            {
                FileAttributes attr = File.GetAttributes(newPath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                { }
                else
                {
                    string finalDir = Path.GetDirectoryName(newPath);
                    if (!Directory.Exists(finalDir))
                        Directory.CreateDirectory(finalDir);
                    if (!File.Exists(newPath))
                        File.Create(newPath);
                }
            }
            catch { }
            return newPath;
        }

        public string TimeToString(TimeSpan? time)
        {

            if (time != null)
            {

                TimeSpan ts = TimeSpan.Parse(time.ToString());
                // @"hh\:mm\:ss"
                string stime = ts.ToString(@"hh\:mm");
                return stime;
            }
            else
            {
                return "-";
            }
        }

        public string DateToString(DateTime? date)
        {
            string sdate = "";
            if (date != null)
            {
                //DateTime ts = DateTime.Parse(date.ToString());
                // @"hh\:mm\:ss"
                //sdate = ts.ToString(@"d/M/yyyy");
                DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;

                switch (AppSettings.dateFormat)
                {
                    case "ShortDatePattern":
                        sdate = date.Value.ToString(dtfi.ShortDatePattern);
                        break;
                    case "LongDatePattern":
                        sdate = date.Value.ToString(dtfi.LongDatePattern);
                        break;
                    case "MonthDayPattern":
                        sdate = date.Value.ToString(dtfi.MonthDayPattern);
                        break;
                    case "YearMonthPattern":
                        sdate = date.Value.ToString(dtfi.YearMonthPattern);
                        break;
                    default:
                        sdate = date.Value.ToString(dtfi.ShortDatePattern);
                        break;
                }
            }

            return sdate;
        }
        public static string DateToStringPatern(DateTime? date)
        {
            string sdate = "";
            if (date != null)
            {
                //DateTime ts = DateTime.Parse(date.ToString());
                // @"hh\:mm\:ss"
                //sdate = ts.ToString(@"d/M/yyyy");
                DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;

                switch (AppSettings.dateFormat)
                {
                    case "ShortDatePattern":
                        sdate = date.Value.ToString(@"dd/MM/yyyy");
                        break;
                    case "LongDatePattern":
                        sdate = date.Value.ToString(@"dddd, MMMM d, yyyy");
                        break;
                    case "MonthDayPattern":
                        sdate = date.Value.ToString(@"MMMM dd");
                        break;
                    case "YearMonthPattern":
                        sdate = date.Value.ToString(@"MMMM yyyy");
                        break;
                    default:
                        sdate = date.Value.ToString(@"dd/MM/yyyy");
                        break;
                }
            }

            return sdate;
        }


        public string DecTostring(decimal? dec)
        {
            string sdc = "0";
            if (dec == null)
            {

            }
            else
            {
                decimal dc = decimal.Parse(dec.ToString());
                if (dc == 0)
                {
                    sdc = "0";
                }
                else
                {
                    switch (AppSettings.accuracy)
                    {
                        case "0":
                            sdc = string.Format("{0:F0}", dc);
                            break;
                        case "1":
                            sdc = string.Format("{0:F1}", dc);
                            break;
                        case "2":
                            sdc = string.Format("{0:F2}", dc);
                            break;
                        case "3":
                            sdc = string.Format("{0:F3}", dc);
                            break;
                        default:
                            sdc = string.Format("{0:F1}", dc);
                            break;
                    }
                }
                //sdc = dc.ToString("0.00");


            }


            return sdc;
        }

        public string BarcodeToImage(string barcodeStr, string imagename)
        {
            // create encoding object
            Zen.Barcode.Code128BarcodeDraw barcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
            string addpath = @"\Thumb\" + imagename + ".png";
            string imgpath = this.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            if (File.Exists(imgpath))
            {
                File.Delete(imgpath);
            }
            if (barcodeStr != "")
            {
                System.Drawing.Bitmap serial_bitmap = (System.Drawing.Bitmap)barcode.Draw(barcodeStr, 60);
                // System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();

                serial_bitmap.Save(imgpath);

                //  generate bitmap
                //  img_barcode.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(serial_bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            else
            {

                imgpath = "";
            }
            if (File.Exists(imgpath))
            {
                return imgpath;
            }
            else
            {
                return "";
            }


        }
        public decimal percentValue(decimal? value, decimal? percent)
        {
            decimal? perval = (value * percent / 100);
            return (decimal)perval;
        }

        public string BarcodeToImage28(string barcodeStr, string imagename)
        {
            // create encoding object
            Zen.Barcode.Code128BarcodeDraw barcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
            string addpath = @"\Thumb\" + imagename + ".png";
            string imgpath = this.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            if (File.Exists(imgpath))
            {
                File.Delete(imgpath);
            }
            if (barcodeStr != "")
            {
                System.Drawing.Bitmap serial_bitmap = (System.Drawing.Bitmap)barcode.Draw(barcodeStr, 60);
                // System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();

                serial_bitmap.Save(imgpath);

                //  generate bitmap
                //  img_barcode.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(serial_bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            else
            {

                imgpath = "";
            }
            if (File.Exists(imgpath))
            {
                return imgpath;
            }
            else
            {
                return "";
            }
        }
        public static bool checkLang()
        {
            bool isArabic;

            //        var set = FillCombo.settingsCls.Where(x => x.name == "report_lang").FirstOrDefault();

            //List<SetValues> replangList = new List<SetValues>();
            //replangList = FillCombo.settingsValues.Where(x=>x.settingId==set.settingId).ToList();
            //AppSettings.Reportlang = replangList.Where(r => r.isDefault == 1).FirstOrDefault().value;

            if (AppSettings.Reportlang.Equals("en"))
            {
                MainWindow.resourcemanagerreport = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                isArabic = false;
            }
            else
            {
                MainWindow.resourcemanagerreport = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                isArabic = true;
            }
            return isArabic;
        }
        public static string checkInvLang()
        {
            string invlang = "en";
            MainWindow.resourcemanagerAr = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
            MainWindow.resourcemanagerEn = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
            //        var set = FillCombo.settingsCls.Where(x => x.name == "report_lang").FirstOrDefault();

            //List<SetValues> replangList = new List<SetValues>();
            //replangList = FillCombo.settingsValues.Where(x=>x.settingId==set.settingId).ToList();
            //AppSettings.Reportlang = replangList.Where(r => r.isDefault == 1).FirstOrDefault().value;

            if (AppSettings.invoice_lang.Equals("en"))
            {
                MainWindow.resourcemanagerreport = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                invlang = "en";
            }
            else if (AppSettings.invoice_lang.Equals("ar"))
            {
                MainWindow.resourcemanagerreport = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                invlang = "ar";
            }
            else
            {
                MainWindow.resourcemanagerreport = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                invlang = "both";
            }
            return invlang;
        }

        public List<ReportParameter> fillPayReport(CashTransfer cashtrans)
        {
            bool isArabic = checkLang();
            Fillcurrency();
            string title;
            string purposeval = "";
            if (cashtrans.transType == "p")
            {
                title = MainWindow.resourcemanagerreport.GetString("trPayVocher");
                if (cashtrans.isInvPurpose)
                {
                    clsReports.ConvertInvType(cashtrans.invType);
                    purposeval = MainWindow.resourcemanagerreport.GetString("Paymentfor") + " " + clsReports.ConvertInvType(cashtrans.invType) + " " + MainWindow.resourcemanagerreport.GetString("invoicenumber") + " : " + cashtrans.invNumber;
                }
                else
                {
                    purposeval = cashtrans.purpose;
                }
            }

            else
            {
                title = MainWindow.resourcemanagerreport.GetString("trReceiptVoucher");
                if (cashtrans.isInvPurpose)
                {

                    purposeval = MainWindow.resourcemanagerreport.GetString("Depositfor") + " " + clsReports.ConvertInvType(cashtrans.invType) + " " + MainWindow.resourcemanagerreport.GetString("invoicenumber") + " : " + cashtrans.invNumber;
                }
                else
                {
                    purposeval = cashtrans.purpose;
                }
            }

            string company_name = AppSettings.companyName;
            string comapny_address = AppSettings.Address;
            string company_phone = AppSettings.Phone;
            string company_fax = AppSettings.Fax;
            string company_email = AppSettings.Email;
            //   string company_logo_img = GetLogoImagePath();
            //string amount = cashtrans.cash.ToString();
            string amount = DecTostring(cashtrans.cash);
            string voucher_num = cashtrans.transNum.ToString();
            string type = "";
            string isCash = "0";
            string trans_num_txt = "";
            string check_num = cashtrans.docNum;
            //string date = cashtrans.createDate.ToString();
            string date = DateToString(cashtrans.createDate);
            string from = "";
            string amount_in_words = "";
            // string purpose = "";
            string recived_by = "";
            string user_name = cashtrans.createUserName + " " + cashtrans.createUserLName;
            string job = MainWindow.resourcemanagerreport.GetString("trAccoutant");
            string pay_to = "";
            if (cashtrans.side == "u" || cashtrans.side == "s")
            {
                pay_to = cashtrans.usersName + " " + cashtrans.usersLName;
            }
            else if (cashtrans.side == "v" || cashtrans.side == "c")
            {
                pay_to = (cashtrans.agentId == null || cashtrans.agentId == 0) ? MainWindow.resourcemanagerreport.GetString("trUnKnown") : cashtrans.agentName;

            }
            else if (cashtrans.side == "sh" || cashtrans.side == "shd")
            {
                pay_to = cashtrans.shippingCompanyName;
            }
            else if (cashtrans.side == "e" || cashtrans.side == "m" || cashtrans.side == "tax")
            {
                pay_to = cashtrans.otherSide;
            }
            else
            {
                pay_to = "";
            }
            if (cashtrans.processType == "cheque")
            {

                type = MainWindow.resourcemanagerreport.GetString("trCheque");
                trans_num_txt = MainWindow.resourcemanagerreport.GetString("ChequeNum");
                //if (isArabic)
                //{
                //    trans_num_txt = "رقم الشيك:";
                //}
                //else
                //{
                //    trans_num_txt = "Cheque Num:";
                //}
                //ChequeNum
                //    MainWindow.resourcemanagerreport.GetString("trCheque");
            }
            else if (cashtrans.processType == "card")
            {
                type = cashtrans.cardName;

                //if (isArabic)
                //{
                //    trans_num_txt = "رقم العملية:";
                //}
                //else
                //{
                //    trans_num_txt = "Transfer Num:";
                //}
                trans_num_txt = MainWindow.resourcemanagerreport.GetString("TransferNum");
                //TransferNum

                // card name and number
            }
            else if (cashtrans.processType == "cash")
            {
                //type = "Cash";
                isCash = "1";
                //trCash
                type = MainWindow.resourcemanagerreport.GetString("trCash");
            }
            else if (cashtrans.processType == "doc")
            {
                //if (isArabic)
                //{
                //    type = "مستند";
                //    trans_num_txt = "رقم المستند:";
                //}
                //else
                //{
                //    type = "Document";
                //    //trDocument
                //    trans_num_txt = "Document Num:";
                //    //DocumentNum
                //}
                type = MainWindow.resourcemanagerreport.GetString("trDocument");
                trans_num_txt = MainWindow.resourcemanagerreport.GetString("DocumentNum");
            }
            /////
            try
            {

                int id = AppSettings.CurrencyId;
                ToWord toWord = new ToWord(Convert.ToDecimal(amount), currencies[id]);

                if (isArabic)
                {
                    amount_in_words = toWord.ConvertToArabic();
                    // cashtrans.cash
                }
                else
                {
                    amount_in_words = toWord.ConvertToEnglish(); ;
                }

            }
            catch (Exception ex)
            {
                amount_in_words = String.Empty;

            }

            //  rep.DataSources.Add(new ReportDataSource("DataSetBank", banksQuery));

            List<ReportParameter> paramarr = new List<ReportParameter>();
            clsReports.Header(paramarr);
            paramarr.Add(new ReportParameter("lang", AppSettings.Reportlang));
            paramarr.Add(new ReportParameter("title", title));
            paramarr.Add(new ReportParameter("company_name", company_name));
            paramarr.Add(new ReportParameter("comapny_address", comapny_address));
            paramarr.Add(new ReportParameter("company_phone", company_phone));
            paramarr.Add(new ReportParameter("company_fax", company_fax));
            paramarr.Add(new ReportParameter("company_email", company_email));
            paramarr.Add(new ReportParameter("company_logo_img", "file:\\" + GetLogoImagePath()));
            paramarr.Add(new ReportParameter("amount", amount));
            paramarr.Add(new ReportParameter("voucher_num", voucher_num));
            paramarr.Add(new ReportParameter("type", type));
            paramarr.Add(new ReportParameter("check_num", check_num));
            paramarr.Add(new ReportParameter("date", date));
            paramarr.Add(new ReportParameter("from", from));


            paramarr.Add(new ReportParameter("amount_in_words", amount_in_words));
            paramarr.Add(new ReportParameter("purpose", purposeval));
            paramarr.Add(new ReportParameter("recived_by", recived_by));
            //paramarr.Add(new ReportParameter("purpose", purpose));
            paramarr.Add(new ReportParameter("user_name", user_name));
            paramarr.Add(new ReportParameter("pay_to", pay_to));
            paramarr.Add(new ReportParameter("job", job));
            paramarr.Add(new ReportParameter("isCash", isCash));
            paramarr.Add(new ReportParameter("trans_num_txt", trans_num_txt));


            paramarr.Add(new ReportParameter("show_header", AppSettings.show_header));
            //
            paramarr.Add(new ReportParameter("trcashAmount", MainWindow.resourcemanagerreport.GetString("cashAmount")));
            paramarr.Add(new ReportParameter("trVoucherno", MainWindow.resourcemanagerreport.GetString("Voucherno")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trRecivedFromMr", MainWindow.resourcemanagerreport.GetString("RecivedFromMr")));
            paramarr.Add(new ReportParameter("trPaytoMr", MainWindow.resourcemanagerreport.GetString("PaytoMr")));
            paramarr.Add(new ReportParameter("trAmountInWords", MainWindow.resourcemanagerreport.GetString("AmountInWords")));
            paramarr.Add(new ReportParameter("trRecivedPurpose", MainWindow.resourcemanagerreport.GetString("RecivedPurpose")));
            paramarr.Add(new ReportParameter("trPaymentPurpose", MainWindow.resourcemanagerreport.GetString("PaymentPurpose")));
            paramarr.Add(new ReportParameter("trReceiver", MainWindow.resourcemanagerreport.GetString("Receiver")));
            paramarr.Add(new ReportParameter("currency", AppSettings.Currency));
            return paramarr;
        }
        public string ConvertAmountToWords(Nullable<decimal> amount)
        {
            Fillcurrency();
            string amount_in_words = "";
            try
            {

                bool isArabic;
                int id = AppSettings.CurrencyId;
                ToWord toWord = new ToWord(Convert.ToDecimal(amount), currencies[id]);
                isArabic = checkLang();
                if (isArabic)
                {
                    amount_in_words = toWord.ConvertToArabic();
                    // cashtrans.cash
                }
                else
                {
                    amount_in_words = toWord.ConvertToEnglish(); ;
                }

            }
            catch (Exception ex)
            {
                amount_in_words = String.Empty;

            }
            return amount_in_words;

        }
        public static string NumberToWordsEN(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWordsEN(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWordsEN(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWordsEN(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWordsEN(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
        public static string NumberToWordsAR(int number)
        {
            if (number == 0)
                return "صفر";

            if (number < 0)
                return "ناقص " + NumberToWordsAR(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWordsAR(number / 1000000) + " مليون ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWordsAR(number / 1000) + " الف ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWordsAR(number / 100) + " مئة ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "و ";

                var unitsMap = new[] { "صفر", "واحد", "اثنان", "ثلاثة", "اربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة", "عشرة", "احدى عشر", "اثنا عشر", "ثلاثة عشر", "اربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر" };
                var tensMap = new[] { "صفر", "عشرة", "عشرون", "ثلاثون", "اربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        public string GetLogoImagePath()
        {
            try
            {
                string imageName = AppSettings.logoImage;
                string dir = Directory.GetCurrentDirectory();
                string tmpPath = Path.Combine(dir, @"Thumb\setting");
                tmpPath = Path.Combine(tmpPath, imageName);
                if (File.Exists(tmpPath))
                {

                    return tmpPath;
                }
                else
                {
                    return Path.Combine(Directory.GetCurrentDirectory(), @"Thumb\setting\emptylogo.png");
                }



                //string addpath = @"\Thumb\setting\" ;

            }
            catch
            {
                return Path.Combine(Directory.GetCurrentDirectory(), @"Thumb\setting\emptylogo.png");
            }
        }
        public string GetIconImagePath(string iconName)
        {
            try
            {
                string imageName = iconName + ".png";
                string dir = Directory.GetCurrentDirectory();
                string tmpPath = Path.Combine(dir, @"pic\socialMedia");
                tmpPath = Path.Combine(tmpPath, imageName);
                if (File.Exists(tmpPath))
                {

                    return tmpPath;
                }
                else
                {
                    return Path.Combine(Directory.GetCurrentDirectory(), @"Thumb\setting\emptylogo.png");
                }



                //string addpath = @"\Thumb\setting\" ;

            }
            catch
            {
                return Path.Combine(Directory.GetCurrentDirectory(), @"Thumb\setting\emptylogo.png");
            }
        }
        //

        public string GetPath(string localpath)
        {
            //string imageName = AppSettings.logoImage;
            //string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string dir = Directory.GetCurrentDirectory();
            string tmpPath = Path.Combine(dir, localpath);



            //string addpath = @"\Thumb\setting\" ;

            return tmpPath;
        }

        public string ReadFile(string localpath)
        {
            string path = GetPath(localpath);
            StreamReader str = new StreamReader(path);
            string content = str.ReadToEnd();
            str.Close();
            return content;
        }

        public reportSize GetpayInvoiceRdlcpath(Invoice invoice, int itemscount, string PaperSize)
        {
            string addpath;
            string isArabic = ReportCls.checkInvLang();
            reportSize rs = new reportSize();
            if (isArabic == "ar")
            {
                if (invoice.invType == "or" || invoice.invType == "po" || invoice.invType == "pos" || invoice.invType == "pod" || invoice.invType == "ors")
                {
                    //order Ar
                    //if (MainWindow.salePaperSize == "5.7cm")
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\Ar\SmallPurOrder.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\Ar\ArInvPurOrderReport.rdlc";
                    }
                }
                else
                {
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\Ar\SmallPur.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);
                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\Ar\ArInvPurReport.rdlc";
                    }
                }
            }
            else if (isArabic == "en")
            {
                if (invoice.invType == "or" || invoice.invType == "po" || invoice.invType == "pos" || invoice.invType == "pod" || invoice.invType == "ors")
                {
                    //order Ar
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\En\SmallPurOrder.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);
                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\En\InvPurOrderReport.rdlc";
                    }
                }
                else
                {
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\En\SmallPur.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);
                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\En\InvPurReport.rdlc";
                    }
                }
            }
            else
            {//Both
                if (invoice.invType == "or" || invoice.invType == "po" || invoice.invType == "pos" || invoice.invType == "pod" || invoice.invType == "ors")
                {
                    //order Both
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\Both\SmallPurOrder.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\Both\InvPurOrderReport.rdlc";
                    }
                }
                else
                {
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\Both\SmallPur.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\Both\InvPurReport.rdlc";
                    }
                }
            }
            //
            string reppath = PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            rs.reppath = reppath;
            rs.paperSize = PaperSize;
            return rs;
        }
        public reportSize GetDirectEntryRdlcpath(Invoice invoice, int itemscount, string PaperSize)
        {
            string addpath;
            string isArabic = ReportCls.checkInvLang();
            reportSize rs = new reportSize();
            if (isArabic == "ar")
            {
                if (invoice.invType == "or" || invoice.invType == "po" || invoice.invType == "pos" || invoice.invType == "pod" || invoice.invType == "ors")
                {

                    //order Ar
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\Ar\SmallPurOrder.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\Ar\ArInvPurOrderReport.rdlc";
                    }

                }
                else if (invoice.invType == "is" || invoice.invType == "isd")
                {
                    //DIRECTr Ar
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Store\Invoice\Ar\SmallDirectEntry.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Store\Ar\ArDirectEntryReport.rdlc";
                    }

                }
                else
                {//purchase Ar
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\Ar\SmallPur.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);
                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\Ar\ArInvPurReport.rdlc";
                    }

                }

            }
            else if (isArabic == "en")
            {
                if (invoice.invType == "or" || invoice.invType == "po" || invoice.invType == "pos" || invoice.invType == "pod" || invoice.invType == "ors")
                {

                    //order En
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\En\SmallPurOrder.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);
                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\En\InvPurOrderReport.rdlc";
                    }

                }
                else if (invoice.invType == "is" || invoice.invType == "isd")
                {
                    //DIRECTr En
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Store\Invoice\En\SmallDirectEntry.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //PaperSize == "A4"
                    {
                        addpath = @"\Reports\Store\En\DirectEntryReport.rdlc";
                    }

                }
                else
                {//purchase En
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\En\SmallPur.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);
                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\En\InvPurReport.rdlc";
                    }

                }
            }
            else
            {//Both
                if (invoice.invType == "or" || invoice.invType == "po" || invoice.invType == "pos" || invoice.invType == "pod" || invoice.invType == "ors")
                {
                    //order Both

                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\Both\SmallPurOrder.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\Both\InvPurOrderReport.rdlc";
                    }

                }
                else if (invoice.invType == "is" || invoice.invType == "isd")
                {//DIRECTr Both

                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Store\Invoice\Both\SmallDirectEntry.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //PaperSize == "A4"
                    {
                        addpath = @"\Reports\Store\Both\DirectEntryReport.rdlc";
                    }

                }
                else
                {//purchase Both
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Purchase\Both\SmallPur.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Purchase\Both\InvPurReport.rdlc";
                    }
                    addpath = @"\Reports\Purchase\Both\InvPurReport.rdlc";
                }
            }
            //
            string reppath = PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            rs.reppath = reppath;
            rs.paperSize = PaperSize;
            return rs;
        }
        public reportSize GetMovementRdlcpath(Invoice invoice, int itemscount, string PaperSize)
        {
            string addpath;
            string isArabic = ReportCls.checkInvLang();
            reportSize rs = new reportSize();
            if (isArabic == "ar")
            {//ItemsExport
                if (PaperSize == "5.7cm")
                {
                    addpath = @"\Reports\Store\Invoice\Ar\SmallMovement.rdlc";
                    rs.width = 224;//224 =5.7cm
                    rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                }
                else //PaperSize == "A4"
                {

                    addpath = @"\Reports\Store\Ar\ArMovement.rdlc";
                }

            }
            else if (isArabic == "en")
            {
                if (PaperSize == "5.7cm")
                {
                    addpath = @"\Reports\Store\Invoice\En\SmallMovement.rdlc";
                    rs.width = 224;//224 =5.7cm
                    rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                }
                else //PaperSize == "A4"
                {

                    addpath = @"\Reports\Store\En\Movement.rdlc";
                }


            }
            else
            {
                if (PaperSize == "5.7cm")
                {
                    addpath = @"\Reports\Store\Invoice\Both\SmallMovement.rdlc";
                    rs.width = 224;//224 =5.7cm
                    rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                }
                else //MainWindow.salePaperSize == "A4"
                {

                    addpath = @"\Reports\Store\Both\Movement.rdlc";
                }

            }

            //
            string reppath = PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            rs.reppath = reppath;
            rs.paperSize = PaperSize;
            return rs;
        }
        public int GetpageHeight(int itemcount, int repheight)
        {
            // int repheight = 457;
            int tableheight = 33 * itemcount;// 33 is cell height


            int totalheight = repheight + tableheight;
            return totalheight;

        }
        //public reportSize GetreceiptInvoiceRdlcpath(Invoice invoice,int itemscount, string PaperSize)
        //{
        //    string addpath;
        //    reportSize rs = new reportSize();
        //    string isArabic = checkInvLang();
        //    if (isArabic == "ar")
        //    {

        //        if (invoice.invType == "q" || invoice.invType == "qd" || invoice.invType == "qs")
        //        {//QT ar
        //            if (PaperSize == "5.7cm")
        //            {
        //                addpath = @"\Reports\Sale\Invoice\Ar\SmallQt.rdlc";
        //                rs.width = 224;//224 =5.7cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else //PaperSize == "A4"
        //            {

        //                addpath = @"\Reports\Sale\Ar\ArInvPurQtReport.rdlc";
        //            }

        //        }
        //        else if (invoice.invType == "or" || invoice.invType == "ord" || invoice.invType == "ors")
        //        {//order
        //            //if (PaperSize == "10cm")
        //            //{
        //            //    addpath = @"\Reports\Sale\Ar\LargeSaleReport.rdlc";
        //            //    rs.width = 400;//400 =10cm
        //            //    rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 550);

        //            //}
        //            //else if (PaperSize == "8cm")
        //            //{
        //            //    addpath = @"\Reports\Sale\Ar\MediumSaleReport.rdlc";
        //            //    rs.width = 315;//315 =8cm
        //            //    rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);


        //            //}
        //            //else
        //            if (PaperSize == "5.7cm")
        //            {
        //                addpath = @"\Reports\Sale\Invoice\Ar\SmallSaleOrder.rdlc";
        //                rs.width = 224;//224 =5.7cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else //MainWindow.salePaperSize == "A4"
        //            {

        //                addpath = @"\Reports\Sale\Ar\ArInvPurOrderReport.rdlc";
        //            }

        //        }
        //        else
        //        {

        //            if (PaperSize == "10cm")
        //            {
        //                addpath = @"\Reports\Sale\Ar\LargeSaleReport.rdlc";
        //                rs.width = 400;//400 =10cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 550);

        //            }
        //            else if (PaperSize == "8cm")
        //            {
        //                addpath = @"\Reports\Sale\Ar\MediumSaleReport.rdlc";
        //                rs.width = 315;//315 =8cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);


        //            }
        //            else if (PaperSize == "5.7cm")
        //            {
        //                addpath = @"\Reports\Sale\Ar\SmallSaleReport.rdlc";
        //                rs.width = 224;//224 =5.7cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else //PaperSize == "A4"
        //            {

        //                addpath = @"\Reports\Sale\Ar\ArInvPurReport.rdlc";
        //            }

        //            //   addpath = @"\Reports\Sale\Ar\LargeSaleReport.rdlc";
        //            //   addpath = @"\Reports\Sale\Ar\MediumSaleReport.rdlc";
        //            //   addpath = @"\Reports\Sale\Ar\SmallSaleReport.rdlc";
        //        }

        //    }
        //    else if (isArabic == "en")
        //    {
        //        if (invoice.invType == "q" || invoice.invType == "qd" || invoice.invType == "qs")
        //        {
        //            //QT En
        //            if (PaperSize == "5.7cm")
        //            {
        //                addpath = @"\Reports\Sale\Invoice\En\SmallQt.rdlc";
        //                rs.width = 224;//224 =5.7cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else //MainWindow.salePaperSize == "A4"
        //            {

        //                addpath = @"\Reports\Sale\En\InvPurQtReport.rdlc";
        //            }

        //        }
        //        else if (invoice.invType == "or" || invoice.invType == "ord" || invoice.invType == "ors")
        //        {
        //            //Order En
        //            if (PaperSize == "5.7cm")
        //            {
        //                addpath = @"\Reports\Sale\Invoice\En\SmallSaleOrder.rdlc";
        //                rs.width = 224;//224 =5.7cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else //MainWindow.salePaperSize == "A4"
        //            {

        //                addpath = @"\Reports\Sale\En\InvPurOrderReport.rdlc";
        //            }


        //        }
        //        else
        //        {
        //            if (PaperSize == "10cm")
        //            {
        //                addpath = @"\Reports\Sale\En\LargeSaleReport.rdlc";
        //                rs.width = 400;//400 =10cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else if (PaperSize == "8cm")
        //            {
        //                addpath = @"\Reports\Sale\En\MediumSaleReport.rdlc";
        //                rs.width = 315;//315 =8cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else if (PaperSize == "5.7cm")
        //            {
        //                addpath = @"\Reports\Sale\En\SmallSaleReport.rdlc";
        //                rs.width = 224;//224 =5.7cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else //MainWindow.salePaperSize == "A4"
        //            {

        //                addpath = @"\Reports\Sale\En\InvPurReport.rdlc";
        //            }
        //            //  addpath = @"\Reports\Sale\En\InvPurReport.rdlc";
        //            //    addpath = @"\Reports\Sale\En\LargeSaleReport.rdlc";
        //            //   addpath = @"\Reports\Sale\En\MediumSaleReport.rdlc";
        //            // addpath = @"\Reports\Sale\En\SmallSaleReport.rdlc";
        //        }

        //    }
        //    else
        //    {
        //        //both lang
        //        if (invoice.invType == "q" || invoice.invType == "qd" || invoice.invType == "qs")
        //        {
        //            //Qt Both

        //            if (PaperSize == "5.7cm")
        //            {
        //                addpath = @"\Reports\Sale\Invoice\Both\SmallQt.rdlc";
        //                rs.width = 224;//224 =5.7cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else //PaperSize == "A4"
        //            {

        //                addpath = @"\Reports\Sale\Both\InvPurQtReport.rdlc";
        //            }


        //        }
        //        else if (invoice.invType == "or" || invoice.invType == "ord" || invoice.invType == "ors")
        //        {

        //            //Order Both
        //            if (PaperSize == "5.7cm")
        //            {
        //                addpath = @"\Reports\Sale\Invoice\Both\SmallSaleOrder.rdlc";
        //                rs.width = 224;//224 =5.7cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else //PaperSize == "A4"
        //            {

        //                addpath = @"\Reports\Sale\Both\InvPurOrderReport.rdlc";
        //            }
        //        }
        //        else
        //        {
        //            if (PaperSize == "10cm")
        //            {
        //                addpath = @"\Reports\Sale\Both\LargeSaleReport.rdlc";
        //                rs.width = 400;//400 =10cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else if (PaperSize == "8cm")
        //            {
        //                addpath = @"\Reports\Sale\Both\MediumSaleReport.rdlc";
        //                rs.width = 315;//315 =8cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else if (PaperSize == "5.7cm")
        //            {
        //                addpath = @"\Reports\Sale\Both\SmallSaleReport.rdlc";
        //                rs.width = 224;//224 =5.7cm
        //                rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

        //            }
        //            else //PaperSize == "A4"
        //            {

        //                addpath = @"\Reports\Sale\Both\InvPurReportBoth.rdlc";
        //            }
        //            //  addpath = @"\Reports\Sale\En\InvPurReport.rdlc";
        //            //    addpath = @"\Reports\Sale\En\LargeSaleReport.rdlc";
        //            //   addpath = @"\Reports\Sale\En\MediumSaleReport.rdlc";
        //            // addpath = @"\Reports\Sale\En\SmallSaleReport.rdlc";
        //        }
        //    }


        //    //
        //    string reppath = PathUp(Directory.GetCurrentDirectory(), 2, addpath);

        //    rs.reppath = reppath;

        //    return rs;
        //}

        public reportSize GetreceiptInvoiceRdlcpath(Invoice invoice, int isPreview, int itemscount, string PaperSize)
        {
            reportSize rs = new reportSize();
            string addpath = "";
            string isArabic = checkInvLang();
            if (isArabic == "ar")
            {

                if ((invoice.invType == "q" || invoice.invType == "qd" || invoice.invType == "qs"))
                {
                    //QT ar
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Sale\Invoice\Ar\SmallQt.rdlc";

                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Sale\Ar\ArInvPurQtReport.rdlc";
                    }


                }
                else if (invoice.invType == "or" || invoice.invType == "ord" || invoice.invType == "ors")
                {
                    //order Ar
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Sale\Invoice\Ar\SmallSaleOrder.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Sale\Ar\ArInvPurOrderReport.rdlc";
                    }
                }
                else
                {

                    if (PaperSize == "10cm")
                    {
                        addpath = @"\Reports\Sale\Ar\LargeSaleReport.rdlc";
                        rs.width = 400;//400 =10cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else if (PaperSize == "8cm")
                    {
                        addpath = @"\Reports\Sale\Ar\MediumSaleReport.rdlc";
                        rs.width = 315;//315 =8cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);


                    }
                    else if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Sale\Ar\SmallSaleReport.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //PaperSize == "A4"
                    {

                        addpath = @"\Reports\Sale\Ar\ArInvPurReport.rdlc";
                    }

                    //   addpath = @"\Reports\Sale\Ar\LargeSaleReport.rdlc";
                    //   addpath = @"\Reports\Sale\Ar\MediumSaleReport.rdlc";
                    //   addpath = @"\Reports\Sale\Ar\SmallSaleReport.rdlc";
                }

            }
            else if (isArabic == "en")
            {
                if (invoice.invType == "q" || invoice.invType == "qd" || invoice.invType == "qs")
                {
                    //QT En
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Sale\Invoice\En\SmallQt.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Sale\En\InvPurQtReport.rdlc";
                    }
                }
                else if (invoice.invType == "or" || invoice.invType == "ord" || invoice.invType == "ors")
                {
                    //Order En
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Sale\Invoice\En\SmallSaleOrder.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Sale\En\InvPurOrderReport.rdlc";
                    }
                }
                else
                {
                    if (PaperSize == "10cm")
                    {
                        addpath = @"\Reports\Sale\En\LargeSaleReport.rdlc";
                        rs.width = 400;//400 =10cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else if (PaperSize == "8cm")
                    {
                        addpath = @"\Reports\Sale\En\MediumSaleReport.rdlc";
                        rs.width = 315;//315 =8cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Sale\En\SmallSaleReport.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Sale\En\InvPurReport.rdlc";
                    }
                    //  addpath = @"\Reports\Sale\En\InvPurReport.rdlc";
                    //    addpath = @"\Reports\Sale\En\LargeSaleReport.rdlc";
                    //   addpath = @"\Reports\Sale\En\MediumSaleReport.rdlc";
                    // addpath = @"\Reports\Sale\En\SmallSaleReport.rdlc";
                }

            }
            else
            {
                //both
                if (invoice.invType == "q" || invoice.invType == "qd" || invoice.invType == "qs")
                {
                    // Qt Both


                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Sale\Invoice\Both\SmallQt.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Sale\Both\InvPurQtReport.rdlc";
                    }
                }
                else if (invoice.invType == "or" || invoice.invType == "ord" || invoice.invType == "ors")
                {
                    //Order Both
                    if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Sale\Invoice\Both\SmallSaleOrder.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Sale\Both\InvPurOrderReport.rdlc";
                    }
                }
                else
                {
                    if (PaperSize == "10cm")
                    {
                        addpath = @"\Reports\Sale\Both\LargeSaleReport.rdlc";
                        rs.width = 400;//400 =10cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else if (PaperSize == "8cm")
                    {
                        addpath = @"\Reports\Sale\Both\MediumSaleReport.rdlc";
                        rs.width = 315;//315 =8cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else if (PaperSize == "5.7cm")
                    {
                        addpath = @"\Reports\Sale\Both\SmallSaleReport.rdlc";
                        rs.width = 224;//224 =5.7cm
                        rs.height = GetpageHeight(itemscount + clsReports.serialsCount, 500);

                    }
                    else //MainWindow.salePaperSize == "A4"
                    {

                        addpath = @"\Reports\Sale\Both\InvPurReportBoth.rdlc";
                    }
                    //  addpath = @"\Reports\Sale\En\InvPurReport.rdlc";
                    //    addpath = @"\Reports\Sale\En\LargeSaleReport.rdlc";
                    //   addpath = @"\Reports\Sale\En\MediumSaleReport.rdlc";
                    // addpath = @"\Reports\Sale\En\SmallSaleReport.rdlc";
                }

            }

            //
            string reppath = PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            rs.paperSize = PaperSize;
            rs.reppath = reppath;
            return rs;
        }
        public List<ReportParameter> fillPurInvReport(Invoice invoice, List<ReportParameter> paramarr)
        {
            string lang = checkInvLang();

            decimal disval = calcpercentval(invoice.discountType, invoice.DBDiscountValue, invoice.total);
            decimal manualdisval = calcpercentval(invoice.manualDiscountType, invoice.manualDiscountValue, invoice.total);
            invoice.DBDiscountValue = disval + manualdisval;
            invoice.discountType = "1";


            //decimal totalafterdis;
            //if (invoice.total != null)
            //{
            //    totalafterdis = (decimal)invoice.total - disval;
            //}
            //else
            //{
            //    totalafterdis = 0;
            //}
            string userName = invoice.uuserName + " " + invoice.uuserLast;
            //string agentName = (invoice.agentCompany != null && invoice.agentCompany != "") ? invoice.agentCompany.Trim()
            //   : ((invoice.agentName != null && invoice.agentName != "") ? invoice.agentName.Trim() : "-");
            string agentName = ((invoice.agentName != null && invoice.agentName != "") ? invoice.agentName.Trim()
                : (invoice.agentCompany != null && invoice.agentCompany != "") ? invoice.agentCompany.Trim() : "-");
            //    decimal taxval = calcpercentval("2", invoice.tax, totalafterdis);
            // decimal totalnet = totalafterdis + taxval;
            //  rep.DataSources.Add(new ReportDataSource("DataSetBank", banksQuery));
            //discountType
            paramarr.Add(new ReportParameter("invNumber", invoice.invNumber == null ? "-" : invoice.invNumber.ToString()));//paramarr[6]
            paramarr.Add(new ReportParameter("invoiceId", invoice.invoiceId.ToString()));
            paramarr.Add(new ReportParameter("invDate", DateToString(invoice.updateDate) == null ? "-" : DateToString(invoice.updateDate)));
            paramarr.Add(new ReportParameter("invTime", invoice.updateDate == null ? "-" : TimeToString(((DateTime)(invoice.updateDate)).TimeOfDay)));
            paramarr.Add(new ReportParameter("vendorInvNum", invoice.agentCode == "-" ? "-" : invoice.agentCode.ToString()));
            paramarr.Add(new ReportParameter("agentName", agentName));
            paramarr.Add(new ReportParameter("total", DecTostring(invoice.total) == null ? "-" : DecTostring(invoice.total)));

            //  paramarr.Add(new ReportParameter("discountValue", DecTostring(disval) == null ? "-" : DecTostring(disval)));
            paramarr.Add(new ReportParameter("discountValue", invoice.DBDiscountValue == null ? "0" : DecTostring(invoice.DBDiscountValue)));
            paramarr.Add(new ReportParameter("discountType", invoice.discountType == null ? "1" : invoice.discountType.ToString()));

            paramarr.Add(new ReportParameter("totalNet", DecTostring(invoice.totalNet) == null ? "-" : DecTostring(invoice.totalNet)));
            paramarr.Add(new ReportParameter("paid", DecTostring(invoice.paid) == null ? "-" : DecTostring(invoice.paid)));
            paramarr.Add(new ReportParameter("deserved", DecTostring(invoice.deserved) == null ? "-" : DecTostring(invoice.deserved)));
            paramarr.Add(new ReportParameter("remain", DecTostring(invoice.cashReturn) == null ? "0" : DecTostring(invoice.cashReturn)));
            //paramarr.Add(new ReportParameter("deservedDate", invoice.deservedDate.ToString() == null ? "-" : invoice.deservedDate.ToString()));
            paramarr.Add(new ReportParameter("deservedDate", invoice.deservedDate.ToString() == null ? "-" : DateToString(invoice.deservedDate)));
            paramarr.Add(new ReportParameter("tax", invoice.tax == null ? "0" : SectionData.PercentageDecTostring(invoice.tax)));
            string invNum = invoice.invNumber == null ? "-" : invoice.invNumber.ToString();
            paramarr.Add(new ReportParameter("barcodeimage", "file:\\" + BarcodeToImage(invNum, "invnum")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("logoImage", "file:\\" + GetLogoImagePath()));
            paramarr.Add(new ReportParameter("branchName", invoice.branchCreatorName == null ? "-" : invoice.branchCreatorName));
            paramarr.Add(new ReportParameter("branchReciever", invoice.branchName == null ? "-" : invoice.branchName));
            paramarr.Add(new ReportParameter("deserveDate", invoice.deservedDate == null ? "-" : DateToString(invoice.deservedDate)));
            paramarr.Add(new ReportParameter("venInvoiceNumber", (invoice.vendorInvNum == null || invoice.vendorInvNum == "") ? "-" : invoice.vendorInvNum.ToString()));//paramarr[6]
            paramarr.Add(new ReportParameter("trTheRemine", MainWindow.resourcemanagerreport.GetString("trTheRemine")));
            paramarr.Add(new ReportParameter("trReceiverName", MainWindow.resourcemanagerreport.GetString("receiverName")));
            paramarr.Add(new ReportParameter("trDepartment", MainWindow.resourcemanagerreport.GetString("Purchases Department")));



            paramarr.Add(new ReportParameter("userName", userName.Trim()));
            if (invoice.invType == "pd" || invoice.invType == "sd" || invoice.invType == "qd"
                    || invoice.invType == "sbd" || invoice.invType == "pbd" || invoice.invType == "pod"
                    || invoice.invType == "ord" || invoice.invType == "imd" || invoice.invType == "exd" || invoice.invType == "isd")
            {

                paramarr.Add(new ReportParameter("watermark", "1"));
                paramarr.Add(new ReportParameter("isSaved", "n"));
            }
            else
            {
                paramarr.Add(new ReportParameter("watermark", "0"));
                paramarr.Add(new ReportParameter("isSaved", "y"));
            }
            if (invoice.invType == "pbd" || invoice.invType == "pb" || invoice.invType == "pbw")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trPurchaseReturnInvTitle")));
                paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trPurchaseReturnInvTitle")));
            }
            else if (invoice.invType == "p" || invoice.invType == "pd" || invoice.invType == "pw" || invoice.invType == "pwd")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trPurchasesInvoice")));
                paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trPurchasesInvoice")));
            }
            else if (invoice.invType == "is" || invoice.invType == "isd")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trDirectEntry")));
                paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trDirectEntry")));
            }
            else if (invoice.invType == "pod" || invoice.invType == "po" || invoice.invType == "ors" || invoice.invType == "pos" || invoice.invType == "pos")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trPurchaceOrder")));
                paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trPurchaceOrder")));
            }
            //trPurchaceOrder

            paramarr.Add(new ReportParameter("trDraftInv", MainWindow.resourcemanagerreport.GetString("trDraft")));

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trDescription")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trSerials", MainWindow.resourcemanagerreport.GetString("trSerials")));
            paramarr.Add(new ReportParameter("By", MainWindow.resourcemanagerreport.GetString("By")));
            paramarr.Add(new ReportParameter("isArchived", invoice.isArchived.ToString()));
            paramarr.Add(new ReportParameter("trArchived", MainWindow.resourcemanagerreport.GetString("trArchived")));
            paramarr.Add(new ReportParameter("mainInvNumber", invoice.mainInvNumber));
            paramarr.Add(new ReportParameter("trRefNo", MainWindow.resourcemanagerreport.GetString("trRefNo.")));
            paramarr.Add(new ReportParameter("invType", invoice.invType));
            paramarr.Add(new ReportParameter("trUpdatedInvoice", MainWindow.resourcemanagerreport.GetString("UpdatedInvoice")));
            //
            paramarr.Add(new ReportParameter("trInvoiceCharp", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trBranchStore", MainWindow.resourcemanagerreport.GetString("trBranch/Store")));
            paramarr.Add(new ReportParameter("trCreator", MainWindow.resourcemanagerreport.GetString("trCreator")));
            paramarr.Add(new ReportParameter("Receiver", MainWindow.resourcemanagerreport.GetString("Receiver")));
            paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            paramarr.Add(new ReportParameter("trVendor", MainWindow.resourcemanagerreport.GetString("trVendor")));
            paramarr.Add(new ReportParameter("trPayments", MainWindow.resourcemanagerreport.GetString("trPayments")));
            paramarr.Add(new ReportParameter("trCashPaid", MainWindow.resourcemanagerreport.GetString("trCashPaid")));
            paramarr.Add(new ReportParameter("trDeservedDate", MainWindow.resourcemanagerreport.GetString("trDeservedDate")));
            paramarr.Add(new ReportParameter("trSum", MainWindow.resourcemanagerreport.GetString("trSum")));
            paramarr.Add(new ReportParameter("trDiscount", MainWindow.resourcemanagerreport.GetString("trDiscount")));
            paramarr.Add(new ReportParameter("trTax", MainWindow.resourcemanagerreport.GetString("trTax")));
            paramarr.Add(new ReportParameter("trShippingAmount", MainWindow.resourcemanagerreport.GetString("trShippingAmount")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trTo", MainWindow.resourcemanagerreport.GetString("trTo")));
            paramarr.Add(new ReportParameter("trDeserved", MainWindow.resourcemanagerreport.GetString("deserved")));

            string agentMobile = (invoice.agentMobile == null || invoice.agentMobile == "") ? "" : invoice.agentMobile;
            agentMobile = agentMobile.Length <= 7 ? "" : agentMobile;
            paramarr.Add(new ReportParameter("agentMobile", agentMobile));
            paramarr.Add(new ReportParameter("trAgentMobile", MainWindow.resourcemanagerreport.GetString("trMobile")));
            paramarr.Add(new ReportParameter("trReceiverName", MainWindow.resourcemanagerreport.GetString("receiverName")));
            paramarr.Add(new ReportParameter("trDepartment", MainWindow.resourcemanagerreport.GetString("purchasesDepartment")));
            paramarr.Add(new ReportParameter("taxValue", invoice.taxValue == null ? "0" : SectionData.DecTostring(invoice.taxValue)));


            if (lang == "both")
            {
                paramarr.Add(new ReportParameter("trDraftInvAr", MainWindow.resourcemanagerAr.GetString("trDraft")));
                paramarr.Add(new ReportParameter("trRefNoAr", MainWindow.resourcemanagerAr.GetString("trRefNo.")));
                paramarr.Add(new ReportParameter("trAddressAr", MainWindow.resourcemanagerAr.GetString("trAddress")));
                paramarr.Add(new ReportParameter("trItemAr", MainWindow.resourcemanagerAr.GetString("trDescription")));
                paramarr.Add(new ReportParameter("trQTRAr", MainWindow.resourcemanagerAr.GetString("trQTR")));
                paramarr.Add(new ReportParameter("trPriceAr", MainWindow.resourcemanagerAr.GetString("trPrice")));
                paramarr.Add(new ReportParameter("trTotalAr", MainWindow.resourcemanagerAr.GetString("trTotal")));
                paramarr.Add(new ReportParameter("cashTrAr", MainWindow.resourcemanagerAr.GetString("trCashType")));
                paramarr.Add(new ReportParameter("trOnDeliveryAr", MainWindow.resourcemanagerAr.GetString("OnDelivery")));
                paramarr.Add(new ReportParameter("trTheShippingCompanyAr", MainWindow.resourcemanagerAr.GetString("trTheShippingCompany")));
                paramarr.Add(new ReportParameter("trDeliveryManAr", MainWindow.resourcemanagerAr.GetString("trDeliveryMan")));
                paramarr.Add(new ReportParameter("trUpdatedInvoiceAr", MainWindow.resourcemanagerAr.GetString("UpdatedInvoice")));
                //  paramarr.Add(new ReportParameter("trUpdatedInvoice", MainWindow.resourcemanagerreport.GetString("UpdatedInvoice")));
                paramarr.Add(new ReportParameter("trInvoiceCharpAr", MainWindow.resourcemanagerAr.GetString("trInvoiceCharp")));
                paramarr.Add(new ReportParameter("trDateAr", MainWindow.resourcemanagerAr.GetString("trDate")));
                paramarr.Add(new ReportParameter("trBranchStoreAr", MainWindow.resourcemanagerAr.GetString("trBranch/Store")));
                paramarr.Add(new ReportParameter("trCreatorAr", MainWindow.resourcemanagerAr.GetString("trCreator")));
                paramarr.Add(new ReportParameter("ReceiverAr", MainWindow.resourcemanagerAr.GetString("Receiver")));
                paramarr.Add(new ReportParameter("trCompanyAr", MainWindow.resourcemanagerAr.GetString("trCompany")));
                paramarr.Add(new ReportParameter("trVendorAr", MainWindow.resourcemanagerAr.GetString("trVendor")));
                paramarr.Add(new ReportParameter("trPaymentsAr", MainWindow.resourcemanagerAr.GetString("trPayments")));
                paramarr.Add(new ReportParameter("trCashPaidAr", MainWindow.resourcemanagerAr.GetString("trCashPaid")));
                paramarr.Add(new ReportParameter("trDeservedDateAr", MainWindow.resourcemanagerAr.GetString("trDeservedDate")));
                paramarr.Add(new ReportParameter("trSumAr", MainWindow.resourcemanagerAr.GetString("trSum")));
                paramarr.Add(new ReportParameter("trDiscountAr", MainWindow.resourcemanagerAr.GetString("trDiscount")));
                paramarr.Add(new ReportParameter("trTaxAr", MainWindow.resourcemanagerAr.GetString("trTax")));
                paramarr.Add(new ReportParameter("trShippingAmountAr", MainWindow.resourcemanagerAr.GetString("trShippingAmount")));
                paramarr.Add(new ReportParameter("trBranchAr", MainWindow.resourcemanagerAr.GetString("trBranch")));
                paramarr.Add(new ReportParameter("trToAr", MainWindow.resourcemanagerAr.GetString("trTo")));
                paramarr.Add(new ReportParameter("trDeservedAr", MainWindow.resourcemanagerAr.GetString("deserved")));
                paramarr.Add(new ReportParameter("trTheRemineAr", MainWindow.resourcemanagerAr.GetString("trTheRemine")));
                paramarr.Add(new ReportParameter("trReceiverNameAr", MainWindow.resourcemanagerAr.GetString("receiverName")));
                paramarr.Add(new ReportParameter("trDepartmentAr", MainWindow.resourcemanagerAr.GetString("Purchases Department")));
                paramarr.Add(new ReportParameter("trAgentMobileAr", MainWindow.resourcemanagerAr.GetString("trMobile")));
                paramarr.Add(new ReportParameter("trReceiverNameAr", MainWindow.resourcemanagerAr.GetString("receiverName")));
                paramarr.Add(new ReportParameter("trDepartmentAr", MainWindow.resourcemanagerAr.GetString("purchasesDepartment")));


            }
            paramarr.Add(new ReportParameter("Notes", (invoice.notes == null || invoice.notes == "") ? "" : invoice.notes.Trim()));
            paramarr.Add(new ReportParameter("invoiceMainId", invoice.invoiceMainId == null ? "0" : invoice.invoiceMainId.ToString()));
            paramarr.Add(new ReportParameter("isUpdated", invoice.ChildInvoice == null ? (0).ToString() : (1).ToString()));
            paramarr.Add(new ReportParameter("shippingCost", DecTostring(invoice.shippingCost)));
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            return paramarr;
        }

        public List<ReportParameter> fillMovment(Invoice invoice, List<ReportParameter> paramarr)
        {
            string lang = checkInvLang();


            string userName = invoice.uuserName + " " + invoice.uuserLast;
            //string agentName = (invoice.agentCompany != null || invoice.agentCompany != "") ? invoice.agentCompany.Trim()
            //   : ((invoice.agentName != null || invoice.agentName != "") ? invoice.agentName.Trim() : "-");


            //    decimal taxval = calcpercentval("2", invoice.tax, totalafterdis);
            // decimal totalnet = totalafterdis + taxval;

            //  rep.DataSources.Add(new ReportDataSource("DataSetBank", banksQuery));

            //discountType
            paramarr.Add(new ReportParameter("invNumber", invoice.invNumber == null ? "-" : invoice.invNumber.ToString()));//paramarr[6]
            paramarr.Add(new ReportParameter("invoiceId", invoice.invoiceId.ToString()));



            paramarr.Add(new ReportParameter("invDate", DateToString(invoice.invDate) == null ? "-" : DateToString(invoice.invDate)));
            paramarr.Add(new ReportParameter("invTime", TimeToString(invoice.invTime)));
            //   paramarr.Add(new ReportParameter("vendorInvNum", invoice.agentCode == "-" ? "-" : invoice.agentCode.ToString()));
            //  paramarr.Add(new ReportParameter("agentName", agentName));
            paramarr.Add(new ReportParameter("total", DecTostring(invoice.total) == null ? "-" : DecTostring(invoice.total)));

            //  paramarr.Add(new ReportParameter("discountValue", DecTostring(disval) == null ? "-" : DecTostring(disval)));
            paramarr.Add(new ReportParameter("discountValue", invoice.discountValue == null ? "0" : DecTostring(invoice.discountValue)));
            paramarr.Add(new ReportParameter("discountType", invoice.discountType == null ? "1" : invoice.discountType.ToString()));

            paramarr.Add(new ReportParameter("totalNet", DecTostring(invoice.totalNet) == null ? "-" : DecTostring(invoice.totalNet)));
            paramarr.Add(new ReportParameter("paid", DecTostring(invoice.paid) == null ? "-" : DecTostring(invoice.paid)));
            paramarr.Add(new ReportParameter("deserved", DecTostring(invoice.deserved) == null ? "-" : DecTostring(invoice.deserved)));
            //paramarr.Add(new ReportParameter("deservedDate", invoice.deservedDate.ToString() == null ? "-" : invoice.deservedDate.ToString()));
            paramarr.Add(new ReportParameter("deservedDate", invoice.deservedDate.ToString() == null ? "-" : DateToString(invoice.deservedDate)));
            paramarr.Add(new ReportParameter("tax", invoice.tax == null ? "0" : SectionData.PercentageDecTostring(invoice.tax)));
            string invNum = invoice.invNumber == null ? "-" : invoice.invNumber.ToString();
            paramarr.Add(new ReportParameter("barcodeimage", "file:\\" + BarcodeToImage(invNum, "invnum")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("logoImage", "file:\\" + GetLogoImagePath()));
            paramarr.Add(new ReportParameter("branchName", invoice.branchCreatorName == null ? "-" : invoice.branchCreatorName));
            paramarr.Add(new ReportParameter("branchReciever", invoice.branchName == null ? "-" : invoice.branchName));
            paramarr.Add(new ReportParameter("deserveDate", invoice.deservedDate == null ? "-" : DateToString(invoice.deservedDate)));
            //  paramarr.Add(new ReportParameter("venInvoiceNumber", (invoice.vendorInvNum == null || invoice.vendorInvNum == "") ? "-" : invoice.vendorInvNum.ToString()));//paramarr[6]

            paramarr.Add(new ReportParameter("userName", userName.Trim()));
            if (invoice.invType == "pd" || invoice.invType == "sd" || invoice.invType == "qd"
                    || invoice.invType == "sbd" || invoice.invType == "pbd" || invoice.invType == "pod"
                    || invoice.invType == "ord" || invoice.invType == "imd" || invoice.invType == "exd" || invoice.invType == "isd")
            {

                paramarr.Add(new ReportParameter("watermark", "1"));
                paramarr.Add(new ReportParameter("isSaved", "n"));
            }
            else
            {
                paramarr.Add(new ReportParameter("watermark", "0"));
                paramarr.Add(new ReportParameter("isSaved", "y"));
            }



            paramarr.Add(new ReportParameter("trDraftInv", MainWindow.resourcemanagerreport.GetString("trDraft")));

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("By", MainWindow.resourcemanagerreport.GetString("By")));

            paramarr.Add(new ReportParameter("isArchived", invoice.isArchived.ToString()));
            paramarr.Add(new ReportParameter("trArchived", MainWindow.resourcemanagerreport.GetString("trArchived")));
            paramarr.Add(new ReportParameter("mainInvNumber", invoice.mainInvNumber));
            paramarr.Add(new ReportParameter("trRefNo", MainWindow.resourcemanagerreport.GetString("trRefNo.")));
            paramarr.Add(new ReportParameter("invType", invoice.invType));
            paramarr.Add(new ReportParameter("trContents", MainWindow.resourcemanagerreport.GetString("contents")));
            paramarr.Add(new ReportParameter("trSerial", MainWindow.resourcemanagerreport.GetString("trSerial")));
            paramarr.Add(new ReportParameter("trWarrantyPeriod", MainWindow.resourcemanagerreport.GetString("warranty")));
            paramarr.Add(new ReportParameter("trInvoiceCharp", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trBranchStore", MainWindow.resourcemanagerreport.GetString("trBranch/Store")));
            paramarr.Add(new ReportParameter("trTo", MainWindow.resourcemanagerreport.GetString("trTo")));
            paramarr.Add(new ReportParameter("trFrom", MainWindow.resourcemanagerreport.GetString("trFrom")));


            if (lang == "both")
            {
                paramarr.Add(new ReportParameter("trDraftInvAr", MainWindow.resourcemanagerAr.GetString("trDraft")));
                paramarr.Add(new ReportParameter("trRefNoAr", MainWindow.resourcemanagerAr.GetString("trRefNo.")));
                paramarr.Add(new ReportParameter("trAddressAr", MainWindow.resourcemanagerAr.GetString("trAddress")));
                paramarr.Add(new ReportParameter("trItemAr", MainWindow.resourcemanagerAr.GetString("trDescription")));
                paramarr.Add(new ReportParameter("trQTRAr", MainWindow.resourcemanagerAr.GetString("trQTR")));
                paramarr.Add(new ReportParameter("trPriceAr", MainWindow.resourcemanagerAr.GetString("trPrice")));
                paramarr.Add(new ReportParameter("trTotalAr", MainWindow.resourcemanagerAr.GetString("trTotal")));
                paramarr.Add(new ReportParameter("cashTrAr", MainWindow.resourcemanagerAr.GetString("trCashType")));
                paramarr.Add(new ReportParameter("trOnDeliveryAr", MainWindow.resourcemanagerAr.GetString("OnDelivery")));
                paramarr.Add(new ReportParameter("trTheShippingCompanyAr", MainWindow.resourcemanagerAr.GetString("trTheShippingCompany")));
                paramarr.Add(new ReportParameter("trDeliveryManAr", MainWindow.resourcemanagerAr.GetString("trDeliveryMan")));
                paramarr.Add(new ReportParameter("trUpdatedInvoiceAr", MainWindow.resourcemanagerAr.GetString("UpdatedInvoice")));
                paramarr.Add(new ReportParameter("trInvoiceCharpAr", MainWindow.resourcemanagerAr.GetString("trInvoiceCharp")));
                paramarr.Add(new ReportParameter("trDateAr", MainWindow.resourcemanagerAr.GetString("trDate")));
                paramarr.Add(new ReportParameter("trBranchStoreAr", MainWindow.resourcemanagerAr.GetString("trBranch/Store")));
                paramarr.Add(new ReportParameter("trToAr", MainWindow.resourcemanagerAr.GetString("trTo")));
                paramarr.Add(new ReportParameter("trFromAr", MainWindow.resourcemanagerAr.GetString("trFrom")));
            }
            paramarr.Add(new ReportParameter("trUpdatedInvoice", MainWindow.resourcemanagerreport.GetString("UpdatedInvoice")));

            return paramarr;
        }
        public decimal calcpercentval(string discountType, decimal? discountValue, decimal? total)
        {

            decimal disval;
            if (discountValue == null || discountValue == 0)
            {
                disval = 0;

            }
            else if (discountValue > 0)
            {

                if (discountType == null || discountType == "-1" || discountType == "0" || discountType == "1")
                {
                    disval = (decimal)discountValue;
                }
                else

                {//percent
                    if (total == null || total == 0)
                    {
                        disval = 0;
                    }
                    else
                    {
                        disval = percentValue(total, discountValue);
                    }
                }
            }
            else
            {
                disval = 0;
            }

            return disval;
        }
        public List<ReportParameter> fillSaleInvReport(Invoice invoice, List<ReportParameter> paramarr)
        {
            checkLang();

            //string agentName = (invoice.agentCompany != null && invoice.agentCompany != "") ? invoice.agentCompany.Trim()
            //: ((invoice.agentName != null && invoice.agentName != "") ? invoice.agentName.Trim() : "-");
            string agentName = ((invoice.agentName != null && invoice.agentName != "") ? invoice.agentName.Trim()
                : (invoice.agentCompany != null && invoice.agentCompany != "") ? invoice.agentCompany.Trim() : "-");
            string userName = invoice.uuserName + " " + invoice.uuserLast;

            //  rep.DataSources.Add(new ReportDataSource("DataSetBank", banksQuery));

            decimal disval = calcpercentval(invoice.discountType, invoice.DBDiscountValue, invoice.total);
            decimal manualdisval = calcpercentval(invoice.manualDiscountType, invoice.manualDiscountValue, invoice.total);
            invoice.DBDiscountValue = disval + manualdisval;
            invoice.discountType = "1";

            //  decimal totalafterdis;
            //if (invoice.total != null)
            //{
            //    totalafterdis = (decimal)invoice.total - disval;
            //}
            //else
            //{
            //    totalafterdis = 0;
            //}

            // discountType
            //  decimal taxval = calcpercentval("2", invoice.tax, totalafterdis);

            // decimal totalnet = totalafterdis + taxval;
            //  percentValue(decimal ? value, decimal ? percent);
            // paramarr.Add(new ReportParameter("sales_invoice_note", AppSettings.sales_invoice_note));
            paramarr.Add(new ReportParameter("sales_invoice_note", invoice.sales_invoice_note));
            paramarr.Add(new ReportParameter("Notes", (invoice.notes == null || invoice.notes == "") ? "-" : invoice.notes.Trim()));
            paramarr.Add(new ReportParameter("invNumber", (invoice.invNumber == null || invoice.invNumber == "") ? "-" : invoice.invNumber.ToString()));//paramarr[6]
            paramarr.Add(new ReportParameter("invoiceId", invoice.invoiceId.ToString()));

            paramarr.Add(new ReportParameter("invDate", DateToString(invoice.updateDate) == null ? "-" : DateToString(invoice.invDate)));
            paramarr.Add(new ReportParameter("invTime", TimeToString(invoice.invTime)));
            paramarr.Add(new ReportParameter("vendorInvNum", invoice.agentCode == "-" ? "-" : invoice.agentCode.ToString()));
            paramarr.Add(new ReportParameter("agentName", agentName.Trim()));
            paramarr.Add(new ReportParameter("total", DecTostring(invoice.total) == null ? "-" : DecTostring(invoice.total)));



            //  paramarr.Add(new ReportParameter("discountValue", DecTostring(disval) == null ? "-" : DecTostring(disval)));
            paramarr.Add(new ReportParameter("discountValue", invoice.DBDiscountValue == null ? "0" : DecTostring(invoice.DBDiscountValue)));
            paramarr.Add(new ReportParameter("discountType", invoice.discountType == null ? "1" : invoice.discountType.ToString()));

            paramarr.Add(new ReportParameter("totalNet", DecTostring(invoice.totalNet) == null ? "-" : DecTostring(invoice.totalNet)));
            paramarr.Add(new ReportParameter("paid", DecTostring(invoice.paid) == null ? "-" : DecTostring(invoice.paid)));
            paramarr.Add(new ReportParameter("deserved", DecTostring(invoice.deserved) == null ? "-" : DecTostring(invoice.deserved)));
            //paramarr.Add(new ReportParameter("deservedDate", invoice.deservedDate.ToString() == null ? "-" : invoice.deservedDate.ToString()));
            paramarr.Add(new ReportParameter("deservedDate", invoice.deservedDate.ToString() == null ? "-" : DateToString(invoice.deservedDate)));


            paramarr.Add(new ReportParameter("tax", DecTostring(invoice.tax) == null ? "0" : DecTostring(invoice.tax)));
            string invNum = invoice.invNumber == null ? "-" : invoice.invNumber.ToString();
            paramarr.Add(new ReportParameter("barcodeimage", "file:\\" + BarcodeToImage(invNum, "invnum")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("branchName", invoice.branchName == null ? "-" : invoice.branchName));
            paramarr.Add(new ReportParameter("userName", userName.Trim()));
            paramarr.Add(new ReportParameter("logoImage", "file:\\" + GetLogoImagePath()));
            if (invoice.invType == "pd" || invoice.invType == "sd" || invoice.invType == "qd"
                        || invoice.invType == "sbd" || invoice.invType == "pbd" || invoice.invType == "pod"
                        || invoice.invType == "ord" || invoice.invType == "imd" || invoice.invType == "exd")
            {

                paramarr.Add(new ReportParameter("watermark", "1"));
            }
            else
            {
                paramarr.Add(new ReportParameter("watermark", "0"));
            }
            paramarr.Add(new ReportParameter("shippingCost", DecTostring(invoice.shippingCost)));

            if (invoice.invType == "sbd" || invoice.invType == "sb")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trSalesReturnInvTitle")));
            }
            else if (invoice.invType == "s" || invoice.invType == "sd")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trSalesInvoice")));

            }
            return paramarr;

        }
        public LocalReport AddDataset(LocalReport rep, List<InvoiceTaxes> invoiceTaxesList)
        {
            List<InvoiceTaxes> emptyList = new List<InvoiceTaxes>();
            if (invoiceTaxesList != null)
            {
                foreach (InvoiceTaxes row in invoiceTaxesList)
                {
                    row.rate = decimal.Parse(SectionData.PercentageDecTostring(row.rate));
                }
                rep.DataSources.Add(new ReportDataSource("DataSetInvoiceTaxes", invoiceTaxesList));
            }
            else
            {
                rep.DataSources.Add(new ReportDataSource("DataSetInvoiceTaxes", emptyList));
            }
            return rep;
        }

        public List<ReportParameter> fillSaleInvReport(Invoice invoice, List<ReportParameter> paramarr, ShippingCompanies shippingcompany)
        {
            string lang = checkInvLang();

            //string agentName = (invoice.agentCompany != null && invoice.agentCompany != "") ? invoice.agentCompany.Trim()
            //: ((invoice.agentName != null && invoice.agentName != "") ? invoice.agentName.Trim() : "-");
            string agentName = ((invoice.agentName != null && invoice.agentName != "") ? invoice.agentName.Trim()
                : (invoice.agentCompany != null && invoice.agentCompany != "") ? invoice.agentCompany.Trim() : "-");
            string userName = invoice.uuserName + " " + invoice.uuserLast;

            //  rep.DataSources.Add(new ReportDataSource("DataSetBank", banksQuery));

            decimal disval = calcpercentval(invoice.discountType, invoice.DBDiscountValue, invoice.total);
            decimal manualdisval = calcpercentval(invoice.manualDiscountType, invoice.manualDiscountValue, invoice.total);
            decimal totaldiscount = disval + manualdisval;
            //invoice.DBDiscountValue = disval + manualdisval;
            invoice.discountType = "1";

            // code for tax
            #region multitax
            decimal totalnotax = (decimal)invoice.total - totaldiscount;
            decimal totaltaxvalue = 0;
            decimal totaltaxrate = 0;
            if (invoice.invoiceTaxes != null)
            {
                totaltaxvalue = invoice.invoiceTaxes.Sum(t => (decimal)t.taxValue);
                totaltaxrate = invoice.invoiceTaxes.Sum(t => (decimal)t.rate);// %
            }


            paramarr.Add(new ReportParameter("totalNotax", DecTostring(totalnotax)));
            paramarr.Add(new ReportParameter("totalTaxvalue", DecTostring(totaltaxvalue)));
            paramarr.Add(new ReportParameter("totalTaxrate", SectionData.PercentageDecTostring(totaltaxrate)));
            paramarr.Add(new ReportParameter("trTotalNotax", MainWindow.resourcemanagerreport.GetString("TAXABLEAMT")));
            paramarr.Add(new ReportParameter("trTotalTax", MainWindow.resourcemanagerreport.GetString("TAXAMT")));
            paramarr.Add(new ReportParameter("trTotalTaxRate", MainWindow.resourcemanagerreport.GetString("RATE")));
            #endregion
            paramarr.Add(new ReportParameter("sales_invoice_note", (invoice.sales_invoice_note == null || invoice.sales_invoice_note == "") ? "" : invoice.sales_invoice_note.Trim()));
            paramarr.Add(new ReportParameter("Notes", (invoice.notes == null || invoice.notes == "") ? "" : invoice.notes.Trim()));
            paramarr.Add(new ReportParameter("invNumber", (invoice.invNumber == null || invoice.invNumber == "") ? "-" : invoice.invNumber.ToString()));//paramarr[6]
            paramarr.Add(new ReportParameter("invoiceId", invoice.invoiceId.ToString()));

            paramarr.Add(new ReportParameter("invDate", DateToString(invoice.updateDate) == null ? "-" : DateToString(invoice.updateDate)));
            paramarr.Add(new ReportParameter("invTime", invoice.updateDate == null ? "-" : TimeToString(((DateTime)(invoice.updateDate)).TimeOfDay))); ;
            paramarr.Add(new ReportParameter("vendorInvNum", invoice.agentCode == "-" ? "-" : invoice.agentCode.ToString()));
            paramarr.Add(new ReportParameter("agentName", agentName.Trim()));
            string agentMobile = (invoice.agentMobile == null || invoice.agentMobile == "") ? "" : invoice.agentMobile;
            agentMobile = agentMobile.Length <= 7 ? "" : agentMobile;
            paramarr.Add(new ReportParameter("agentMobile", agentMobile));
            paramarr.Add(new ReportParameter("trAgentMobile", MainWindow.resourcemanagerreport.GetString("trMobile")));


            paramarr.Add(new ReportParameter("agentAddress", (invoice.agentAddress == null || invoice.agentAddress == "") ? "-" : invoice.agentAddress.Trim()));
            paramarr.Add(new ReportParameter("trAddress", MainWindow.resourcemanagerreport.GetString("trAddress")));
            paramarr.Add(new ReportParameter("total", DecTostring(invoice.total) == null ? "-" : DecTostring(invoice.total)));
            paramarr.Add(new ReportParameter("remain", DecTostring(invoice.cashReturn) == null ? "0" : DecTostring(invoice.cashReturn)));
            //  paramarr.Add(new ReportParameter("discountValue", DecTostring(disval) == null ? "-" : DecTostring(disval)));
            paramarr.Add(new ReportParameter("discountValue", DecTostring(totaldiscount)));
            paramarr.Add(new ReportParameter("discountType", invoice.discountType == null ? "1" : invoice.discountType.ToString()));

            paramarr.Add(new ReportParameter("totalNet", invoice.totalNet == null ? "0" : DecTostring(invoice.totalNet)));
            if (invoice.isFreeShip == 1)
            {
                paramarr.Add(new ReportParameter("totalNoShip", DecTostring(invoice.totalNet)));

            }
            else
            {
                paramarr.Add(new ReportParameter("totalNoShip", DecTostring(invoice.totalNet - invoice.shippingCost)));

            }

            paramarr.Add(new ReportParameter("paid", invoice.paid == null ? "0" : DecTostring(invoice.paid)));
            //   paramarr.Add(new ReportParameter("deserved", DecTostring(invoice.deserved) == null ? "-" : DecTostring(invoice.deserved)));
            //paramarr.Add(new ReportParameter("deservedDate", invoice.deservedDate.ToString() == null ? "-" : invoice.deservedDate.ToString()));
            paramarr.Add(new ReportParameter("deservedDate", invoice.deservedDate.ToString() == null ? "-" : DateToString(invoice.deservedDate)));

            paramarr.Add(new ReportParameter("tax", invoice.tax == null ? "0" : SectionData.PercentageDecTostring(invoice.tax)));
            paramarr.Add(new ReportParameter("taxValue", invoice.taxValue == null ? "0" : SectionData.DecTostring(invoice.taxValue)));

            string invNum = invoice.invNumber == null ? "-" : invoice.invNumber.ToString();
            paramarr.Add(new ReportParameter("barcodeimage", "file:\\" + BarcodeToImage(invNum, "invnum")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("branchName", invoice.branchName == null ? "-" : invoice.branchName));
            paramarr.Add(new ReportParameter("userName", userName.Trim()));
            paramarr.Add(new ReportParameter("logoImage", "file:\\" + GetLogoImagePath()));
            if (invoice.invType == "pd" || invoice.invType == "sd" || invoice.invType == "qd"
                        || invoice.invType == "sbd" || invoice.invType == "pbd" || invoice.invType == "pod"
                        || invoice.invType == "ord" || invoice.invType == "imd" || invoice.invType == "exd")
            {

                paramarr.Add(new ReportParameter("watermark", "1"));
            }
            else
            {
                paramarr.Add(new ReportParameter("watermark", "0"));
            }
            paramarr.Add(new ReportParameter("shippingCost", DecTostring(invoice.shippingCost)));

            if (invoice.invType == "sbd" || invoice.invType == "sb")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trSalesReturnInvTitle")));
                paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trSalesReturnInvTitle")));
            }
            else if (invoice.invType == "s" || invoice.invType == "sd")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trSalesInvoice")));
                paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trSalesInvoice")));

            }
            else if (invoice.invType == "or" || invoice.invType == "ors" || invoice.invType == "ord")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trSaleOrder")));
                paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trSaleOrder")));
            }
            else if (invoice.invType == "q" || invoice.invType == "qs" || invoice.invType == "qd")
            {
                paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trQuotations")));
                paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trQuotations")));
            }
            paramarr.Add(new ReportParameter("trDeliveryMan", MainWindow.resourcemanagerreport.GetString("trDeliveryMan")));
            paramarr.Add(new ReportParameter("trTheShippingCompany", MainWindow.resourcemanagerreport.GetString("trTheShippingCompany")));
            paramarr.Add(new ReportParameter("DeliveryMan", invoice.shipUserName));
            paramarr.Add(new ReportParameter("ShippingCompany", shippingcompany.name));
            paramarr.Add(new ReportParameter("deliveryType", shippingcompany.deliveryType));
            paramarr.Add(new ReportParameter("shippingCompanyId", invoice.shippingCompanyId == null ? "0" : invoice.shippingCompanyId.ToString()));
            paramarr.Add(new ReportParameter("trFree", MainWindow.resourcemanagerreport.GetString("trFree")));
            paramarr.Add(new ReportParameter("trDraftInv", MainWindow.resourcemanagerreport.GetString("trDraft")));

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trDescription")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));

            paramarr.Add(new ReportParameter("trSerials", MainWindow.resourcemanagerreport.GetString("trSerials")));
            paramarr.Add(new ReportParameter("By", MainWindow.resourcemanagerreport.GetString("By")));
            paramarr.Add(new ReportParameter("trWarrantyPeriod", MainWindow.resourcemanagerreport.GetString("warranty")));
            paramarr.Add(new ReportParameter("trOnDelivery", MainWindow.resourcemanagerreport.GetString("OnDelivery")));

            paramarr.Add(new ReportParameter("isArchived", invoice.isArchived.ToString()));
            //   paramarr.Add(new ReportParameter("trArchived", MainWindow.resourcemanagerreport.GetString("trArchived")));
            paramarr.Add(new ReportParameter("mainInvNumber", invoice.mainInvNumber));
            paramarr.Add(new ReportParameter("trRefNo", MainWindow.resourcemanagerreport.GetString("trRefNo.")));
            paramarr.Add(new ReportParameter("invType", invoice.invType));
            paramarr.Add(new ReportParameter("trContents", MainWindow.resourcemanagerreport.GetString("contents")));
            paramarr.Add(new ReportParameter("trSerial", MainWindow.resourcemanagerreport.GetString("trSerial")));
            paramarr.Add(new ReportParameter("invoiceMainId", invoice.invoiceMainId == null ? "0" : invoice.invoiceMainId.ToString()));
            paramarr.Add(new ReportParameter("trinvoiceClass", MainWindow.resourcemanagerreport.GetString("invoiceClass")));
            paramarr.Add(new ReportParameter("invoiceClass", invoice.sliceName));
            //
            paramarr.Add(new ReportParameter("trInvoiceCharp", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trTo", MainWindow.resourcemanagerreport.GetString("trTo")));
            paramarr.Add(new ReportParameter("trPayments", MainWindow.resourcemanagerreport.GetString("trPayments")));
            paramarr.Add(new ReportParameter("trCashPaid", MainWindow.resourcemanagerreport.GetString("trCashPaid")));
            paramarr.Add(new ReportParameter("trUnPaid", MainWindow.resourcemanagerreport.GetString("trUnPaid")));
            paramarr.Add(new ReportParameter("trUnPaidCash", MainWindow.resourcemanagerreport.GetString("trUnPaidCash")));
            paramarr.Add(new ReportParameter("trTheRemine", MainWindow.resourcemanagerreport.GetString("trTheRemine")));
            paramarr.Add(new ReportParameter("trSum", MainWindow.resourcemanagerreport.GetString("trSum")));
            paramarr.Add(new ReportParameter("trDiscount", MainWindow.resourcemanagerreport.GetString("trDiscount")));
            paramarr.Add(new ReportParameter("trTax", MainWindow.resourcemanagerreport.GetString("trTax")));
            paramarr.Add(new ReportParameter("trDelivery", MainWindow.resourcemanagerreport.GetString("trDelivery")));
            paramarr.Add(new ReportParameter("trDeserved", MainWindow.resourcemanagerreport.GetString("deserved")));

            paramarr.Add(new ReportParameter("trReceiverName", MainWindow.resourcemanagerreport.GetString("receiverName")));
            paramarr.Add(new ReportParameter("trDepartment", MainWindow.resourcemanagerreport.GetString("salesDepartment")));


            if (lang == "both")
            {
                paramarr.Add(new ReportParameter("trDraftInvAr", MainWindow.resourcemanagerAr.GetString("trDraft")));
                paramarr.Add(new ReportParameter("trRefNoAr", MainWindow.resourcemanagerAr.GetString("trRefNo.")));
                paramarr.Add(new ReportParameter("trAddressAr", MainWindow.resourcemanagerAr.GetString("trAddress")));
                paramarr.Add(new ReportParameter("trItemAr", MainWindow.resourcemanagerAr.GetString("trDescription")));
                paramarr.Add(new ReportParameter("trQTRAr", MainWindow.resourcemanagerAr.GetString("trQTR")));
                paramarr.Add(new ReportParameter("trPriceAr", MainWindow.resourcemanagerAr.GetString("trPrice")));
                paramarr.Add(new ReportParameter("trTotalAr", MainWindow.resourcemanagerAr.GetString("trTotal")));
                paramarr.Add(new ReportParameter("cashTrAr", MainWindow.resourcemanagerAr.GetString("trCashType")));
                paramarr.Add(new ReportParameter("trOnDeliveryAr", MainWindow.resourcemanagerAr.GetString("OnDelivery")));
                paramarr.Add(new ReportParameter("trTheShippingCompanyAr", MainWindow.resourcemanagerAr.GetString("trTheShippingCompany")));
                paramarr.Add(new ReportParameter("trDeliveryManAr", MainWindow.resourcemanagerAr.GetString("trDeliveryMan")));
                paramarr.Add(new ReportParameter("trUpdatedInvoiceAr", MainWindow.resourcemanagerAr.GetString("UpdatedInvoice")));
                paramarr.Add(new ReportParameter("trinvoiceClassAr", MainWindow.resourcemanagerAr.GetString("invoiceClass")));
                paramarr.Add(new ReportParameter("trFreeAr", MainWindow.resourcemanagerAr.GetString("trFree")));
                //   
                paramarr.Add(new ReportParameter("trInvoiceCharpAr", MainWindow.resourcemanagerAr.GetString("trInvoiceCharp")));
                paramarr.Add(new ReportParameter("trDateAr", MainWindow.resourcemanagerAr.GetString("trDate")));
                paramarr.Add(new ReportParameter("trBranchAr", MainWindow.resourcemanagerAr.GetString("trBranch")));
                paramarr.Add(new ReportParameter("trToAr", MainWindow.resourcemanagerAr.GetString("trTo")));
                paramarr.Add(new ReportParameter("trPaymentsAr", MainWindow.resourcemanagerAr.GetString("trPayments")));
                paramarr.Add(new ReportParameter("trCashPaidAr", MainWindow.resourcemanagerAr.GetString("trCashPaid")));
                paramarr.Add(new ReportParameter("trUnPaidAr", MainWindow.resourcemanagerAr.GetString("trUnPaid")));
                paramarr.Add(new ReportParameter("trUnPaidCashAr", MainWindow.resourcemanagerAr.GetString("trUnPaidCash")));
                paramarr.Add(new ReportParameter("trTheRemineAr", MainWindow.resourcemanagerAr.GetString("trTheRemine")));
                paramarr.Add(new ReportParameter("trSumAr", MainWindow.resourcemanagerAr.GetString("trSum")));
                paramarr.Add(new ReportParameter("trDiscountAr", MainWindow.resourcemanagerAr.GetString("trDiscount")));
                paramarr.Add(new ReportParameter("trTaxAr", MainWindow.resourcemanagerAr.GetString("trTax")));
                paramarr.Add(new ReportParameter("trDeliveryAr", MainWindow.resourcemanagerAr.GetString("trDelivery")));
                paramarr.Add(new ReportParameter("trDeservedAr", MainWindow.resourcemanagerAr.GetString("deserved")));
                paramarr.Add(new ReportParameter("trAgentMobileAr", MainWindow.resourcemanagerAr.GetString("trMobile")));
                paramarr.Add(new ReportParameter("trReceiverNameAr", MainWindow.resourcemanagerAr.GetString("receiverName")));
                paramarr.Add(new ReportParameter("trDepartmentAr", MainWindow.resourcemanagerAr.GetString("salesDepartment")));
                #region multitax
                paramarr.Add(new ReportParameter("trTotalNotaxAr", MainWindow.resourcemanagerAr.GetString("TAXABLEAMT")));
                paramarr.Add(new ReportParameter("trTotalTaxAr", MainWindow.resourcemanagerAr.GetString("TAXAMT")));
                paramarr.Add(new ReportParameter("trTotalTaxRateAr", MainWindow.resourcemanagerAr.GetString("RATE")));

                #endregion

            }
            paramarr.Add(new ReportParameter("trUpdatedInvoice", MainWindow.resourcemanagerreport.GetString("UpdatedInvoice")));
            paramarr.Add(new ReportParameter("isPrePaid", invoice.isPrePaid.ToString()));
            // paramarr.Add(new ReportParameter("isShipPaid", invoice.isShipPaid.ToString()));
            paramarr.Add(new ReportParameter("trOfferOnRep", MainWindow.resourcemanagerreport.GetString("trOfferOnRep")));


            paramarr.Add(new ReportParameter("isUpdated", invoice.ChildInvoice == null ? (0).ToString() : (1).ToString()));
            paramarr.Add(new ReportParameter("trDiscountOffer", MainWindow.resourcemanagerreport.GetString("trDiscountOffer")));
            paramarr.Add(new ReportParameter("isFreeShip", invoice.isFreeShip.ToString()));
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));

            return paramarr;
        }
        public static List<ItemTransferInvoice> converter(List<ItemTransferInvoice> query)
        {
            foreach (ItemTransferInvoice item in query)
            {
                if (item.invType == "p")
                {
                    item.invType = MainWindow.resourcemanagerreport.GetString("trPurchaseInvoice");
                }
                else if (item.invType == "pw")
                {
                    item.invType = MainWindow.resourcemanagerreport.GetString("trPurchaseInvoice");
                }
                else if (item.invType == "pb")
                {
                    item.invType = MainWindow.resourcemanagerreport.GetString("trPurchaseReturnInvoice");
                }
                else if (item.invType == "pd")
                {
                    item.invType = MainWindow.resourcemanagerreport.GetString("trDraftPurchaseBill");
                }
                else if (item.invType == "pbd")
                {
                    item.invType = MainWindow.resourcemanagerreport.GetString("trPurchaseReturnDraft");
                }
            }
            return query;

        }


        /////////
        ///


        public bool encodefile(string source, string dest)
        {
            try
            {

                byte[] arr = File.ReadAllBytes(source);

                arr = Encrypt(arr);

                File.WriteAllBytes(dest, arr);
                return true;
            }
            catch
            {
                return false;
            }

        }



        public static byte[] Encrypt(byte[] ordinary)
        {
            BitArray bits = ToBits(ordinary);
            BitArray LHH = SubBits(bits, 0, bits.Length / 2);
            BitArray RHH = SubBits(bits, bits.Length / 2, bits.Length / 2);
            BitArray XorH = LHH.Xor(RHH);
            RHH = RHH.Not();
            XorH = XorH.Not();
            BitArray encr = ConcateBits(XorH, RHH);
            byte[] b = new byte[encr.Length / 8];
            encr.CopyTo(b, 0);
            return b;
        }


        private static BitArray ToBits(byte[] Bytes)
        {
            BitArray bits = new BitArray(Bytes);
            return bits;
        }
        private static BitArray SubBits(BitArray Bits, int Start, int Length)
        {
            BitArray half = new BitArray(Length);
            for (int i = 0; i < half.Length; i++)
                half[i] = Bits[i + Start];
            return half;
        }
        private static BitArray ConcateBits(BitArray LHH, BitArray RHH)
        {
            BitArray bits = new BitArray(LHH.Length + RHH.Length);
            for (int i = 0; i < LHH.Length; i++)
                bits[i] = LHH[i];
            for (int i = 0; i < RHH.Length; i++)
                bits[i + LHH.Length] = RHH[i];
            return bits;
        }
        public void DelFile(string fileName)
        {

            bool inuse = false;

            inuse = IsFileInUse(fileName);
            if (inuse == false)
            {
                File.Delete(fileName);
            }






        }

        private bool IsFileInUse(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                //throw new ArgumentException("'path' cannot be null or empty.", "path");
                return true;
            }


            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite)) { }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }


        //////////


        public static bool encodestring(string sourcetext, string dest)
        {
            try
            {
                byte[] arr = ConvertToBytes(sourcetext);
                //  byte[] arr = File.ReadAllBytes(source);

                arr = Encrypt(arr);

                File.WriteAllBytes(dest, arr);
                return true;
            }
            catch
            {
                return false;
            }

        }

        private static byte[] ConvertToBytes(string text)
        {
            return System.Text.Encoding.Unicode.GetBytes(text);
        }

        public static string Decrypt(string EncryptedText)
        {
            byte[] b = ConvertToBytes(EncryptedText);
            b = Decrypt(b);
            return ConvertToText(b);
        }
        public static string DeCompressThenDecrypt(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            text = Encoding.UTF8.GetString(bytes);

            return (Decrypt(text));
        }
        public static bool decodefile(string Source, string DestPath)
        {
            try
            {
                byte[] restorearr = File.ReadAllBytes(Source);

                restorearr = Decrypt(restorearr);
                File.WriteAllBytes(DestPath, restorearr);
                return true;

            }
            catch
            {
                return false;
            }
        }

        public static string decodetoString(string Source)
        {
            try
            {
                byte[] restorearr = File.ReadAllBytes(Source);

                restorearr = Decrypt(restorearr);
                return ConvertToText(restorearr);
                // File.WriteAllBytes(DestPath, restorearr);


            }
            catch
            {
                return "0";
            }
        }
        private static string ConvertToText(byte[] ByteAarry)
        {
            return System.Text.Encoding.Unicode.GetString(ByteAarry);
        }
        public static byte[] Decrypt(byte[] Encrypted)
        {
            BitArray enc = ToBits(Encrypted);
            BitArray XorH = SubBits(enc, 0, enc.Length / 2);
            XorH = XorH.Not();
            BitArray RHH = SubBits(enc, enc.Length / 2, enc.Length / 2);
            RHH = RHH.Not();
            BitArray LHH = XorH.Xor(RHH);
            BitArray bits = ConcateBits(LHH, RHH);
            byte[] decr = new byte[bits.Length / 8];
            bits.CopyTo(decr, 0);
            return decr;
        }

        static public void pdfChart(System.Drawing.Image img)
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            //  bool isArabic = ReportCls.checkLang();
            ReportCls reportclass = new ReportCls();
            LocalReport rep = new LocalReport();
            addpath = @"\Reports\image\Chart.rdlc";
            string imgepath = @"\Reports\image\chartimg.png";
            imgepath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, imgepath);
            img.Save(imgepath);
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            // ReportCls.checkLang();
            clsReports.chartExportReport(rep, reppath, paramarr, imgepath);
            rep.SetParameters(paramarr);
            rep.Refresh();
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF|*.pdf;";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filepath = saveFileDialog.FileName;
                LocalReportExtensions.ExportToPDF(rep, filepath);
            }
        }

    }
}

