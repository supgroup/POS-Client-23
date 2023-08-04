﻿using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
using WIA;
using System.Web.UI.WebControls;
using System.Windows.Resources;
using System.Resources;
using System.Reflection;


using System.IO;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_uploadImage.xaml
    /// </summary>
    public partial class wd_uploadImage : Window
    {
        public string tableName { get; set; }
        public int tableId { get; set; }
        public string docNum { get; set; }

        private int docId = 0;
        string imagepath;
        DocImage docImgModel = new DocImage();
        List<DocImage> imageList;

        ImageBrush brush = new ImageBrush();

        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        public wd_uploadImage()
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = System.Windows.FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = System.Windows.FlowDirection.RightToLeft;
                }
                translate();
                #endregion

                await refreshImageList();

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
            txt_image.Text = MainWindow.resourcemanager.GetString("trImage");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));

            tt_name.Content = MainWindow.resourcemanager.GetString("trName");
            tt_notes.Content = MainWindow.resourcemanager.GetString("trNote");
            tt_file.Content = MainWindow.resourcemanager.GetString("trSelectImage");
            tt_scanner.Content = MainWindow.resourcemanager.GetString("trScan");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_pdf.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_printInvoice.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_delete.Content = MainWindow.resourcemanager.GetString("trDelete");
            tt_update.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_save.Content = MainWindow.resourcemanager.GetString("trAdd");
        }

        private void Tb_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_name_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Img_upload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //select image
                openFileDialog.Filter = "Images|*.png;*.jpg;*.bmp;*.jpeg;*.jfif";

                if (openFileDialog.ShowDialog() == true)
                {
                    FileInfo fs = new FileInfo(openFileDialog.FileName);
                    long filesize = fs.Length / 1024;
                    if (filesize >= 500)
                    {
                        string str = "file size must be les than 500 KB";
                        MessageBox.Show(str);

                    }
                    else
                    {
                        brush.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Relative));
                        img_upload.Background = brush;
                    }



                }


                // MessageBox.Show(filesize.ToString());

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
        private void validateDocValues()
        {
            SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
        }
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                validateDocValues();
                if (!tb_name.Text.Equals(""))
                {
                    docId = 0;
                    docImgModel.id = docId;
                    docImgModel.tableName = tableName;
                    docImgModel.tableId = tableId;
                    docImgModel.docnum = docNum;
                    docImgModel.docName = tb_name.Text;
                    docImgModel.note = tb_notes.Text;
                    docImgModel.createUserId = MainWindow.userID;

                   int res = (int)await docImgModel.saveDocImage(docImgModel);
                    if (!res.Equals(0))
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                    else
                        Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                    int docImageId = res;

                    // await docImgModel.uploadImage(openFileDialog.FileName, tableName, docImageId);
                    await docImgModel.uploadOrginalImage(openFileDialog.FileName, tableName, docImageId);

                    //refresh image list
                    await refreshImageList();
                    clear();
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

        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {//delete
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (docImgModel.id != 0)
                {
                    int res = (int)await docImgModel.delete(docId);

                    if (res>0)
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                    else
                        Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                    docId = 0;
                    //clear img
                    Uri resourceUri = new Uri("/pic/no-image-icon-125x125.png", UriKind.Relative);
                    StreamResourceInfo streamInfo = System.Windows.Application.GetResourceStream(resourceUri);
                    BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                    brush.ImageSource = temp;
                    img_upload.Background = brush;

                    //refresh images
                    await refreshImageList();
                    clear();
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
        private void clear()
        {
            tb_name.Clear();
            tb_notes.Clear();
            openFileDialog.FileName = "";
            docImgModel = new DocImage();

            SectionData.clearValidate(tb_name, p_errorName);
            SectionData.clearImg(img_upload);
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

        private async Task refreshImageList()
        {
            imageList = await docImgModel.GetDocImages(tableName, tableId);

            lst_images.ItemsSource = imageList.ToList();
            lst_images.DisplayMemberPath = "docName";
            lst_images.SelectedValuePath = "id";
        }
        private async void getImg(string imageName)
        {

            try
            {
                if (docImgModel.image.Equals(""))
                {
                    SectionData.clearImg(img_upload);
                }
                else
                {
                    byte[] imageBuffer = await docImgModel.downloadImage(imageName); // read this as BLOB from your DB

                    var bitmapImage = new BitmapImage();
                    if (imageBuffer != null)
                    {
                        using (var memoryStream = new MemoryStream(imageBuffer))
                        {
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = memoryStream;
                            bitmapImage.EndInit();
                        }

                        img_upload.Background = new ImageBrush(bitmapImage);
                        // configure trmporary path
                        //string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                        string dir = Directory.GetCurrentDirectory();
                        string tmpPath = System.IO.Path.Combine(dir, Global.TMPAgentsFolder);

                        tmpPath = System.IO.Path.Combine(tmpPath, docImgModel.image);
                        openFileDialog.FileName = tmpPath;
                        // + @"\agents\" +
                        imagepath = System.IO.Path.Combine(dir, Global.TMPFolder + @"\" + docImgModel.image);
                    }
                    else
                        SectionData.clearImg(img_upload);
                }
            }
            catch { }

        }
        // display image in IMG_customer 
        private void Lst_images_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (lst_images.SelectedIndex != -1)
                {
                    docId = (int)lst_images.SelectedValue;
                    docImgModel = imageList.ToList().Find(c => c.id == docId);
                    tb_name.Text = docImgModel.docName;
                    tb_notes.Text = docImgModel.note;
                    string imageName = docImgModel.image;
                    getImg(imageName);
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
        private void Btn_scan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                CommonDialogClass commonDialogClass = new CommonDialogClass();
                Device scannerDevice = commonDialogClass.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, false, false);
                if (scannerDevice != null)
                {
                    WIA.Item scannnerItem = scannerDevice.Items[1];
                    AdjustScannerSettings(scannnerItem, 600, 0, 0, 1010, 620, 0, 0);
                    object scanResult = commonDialogClass.ShowTransfer(scannnerItem, WIA.FormatID.wiaFormatPNG, false);
                    if (scanResult != null)
                    {
                        ImageFile image = (ImageFile)scanResult;

                        var imageBytes = (byte[])image.FileData.get_BinaryData();// <-- Converts the ImageFile to a byte array
                        var bitmapImage = new BitmapImage();
                        if (imageBytes != null)
                        {
                            using (var memoryStream = new MemoryStream(imageBytes))
                            {
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = memoryStream;
                                bitmapImage.EndInit();
                            }

                            img_upload.Background = new ImageBrush(bitmapImage);
                        }
                        // configure trmporery path
                        //string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                        string dir = Directory.GetCurrentDirectory();
                        string tmpPath = System.IO.Path.Combine(dir, Global.ScannedImageLocation);
                        if (System.IO.File.Exists(tmpPath))
                        {
                            System.IO.File.Delete(tmpPath);
                        }
                        SaveImageToJpgFile(image, tmpPath);
                        openFileDialog.FileName = tmpPath;
                    }
                }

                if (sender != null)
                    SectionData.EndAwait(grid_main);

            }
            catch (System.Runtime.InteropServices.COMException)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                System.Windows.MessageBox.Show("Problem with scanning device. Please ensure that the scanner is properly connected and switched on", "Inweon Grain Management System");
            }

        }
        private static void SaveImageToJpgFile(ImageFile image, string fileName)
        {
            WIA.ImageProcess imgProcess = new WIA.ImageProcess();
            object convertFilter = "Convert";
            string convertFilterID = imgProcess.FilterInfos.get_Item(ref convertFilter).FilterID;
            imgProcess.Filters.Add(convertFilterID, 0);
            SetWIAProperty(imgProcess.Filters[imgProcess.Filters.Count].Properties, "FormatID", WIA.FormatID.wiaFormatJPEG);
            image = imgProcess.Apply(image);
            image.SaveFile(fileName);
        }
        private static void AdjustScannerSettings(IItem scannnerItem, int scanResolutionDPI, int scanStartLeftPixel, int scanStartTopPixel,
             int scanWidthPixels, int scanHeightPixels, int brightnessPercents, int contrastPercents)
        {
            const string WIA_HORIZONTAL_SCAN_RESOLUTION_DPI = "6147";
            const string WIA_VERTICAL_SCAN_RESOLUTION_DPI = "6148";
            const string WIA_HORIZONTAL_SCAN_START_PIXEL = "6149";
            const string WIA_VERTICAL_SCAN_START_PIXEL = "6150";
            const string WIA_HORIZONTAL_SCAN_SIZE_PIXELS = "6151";
            const string WIA_VERTICAL_SCAN_SIZE_PIXELS = "6152";
            const string WIA_SCAN_BRIGHTNESS_PERCENTS = "6154";
            const string WIA_SCAN_CONTRAST_PERCENTS = "6155";
            const string WIA_SCAN_BIT_DEPTH = "4104";
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_RESOLUTION_DPI, scanResolutionDPI);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_RESOLUTION_DPI, scanResolutionDPI);
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_START_PIXEL, scanStartLeftPixel);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_START_PIXEL, scanStartTopPixel);
            //SetWIAProperty(scannnerItem.Properties, WIA_SCAN_BIT_DEPTH, 48);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_BRIGHTNESS_PERCENTS, brightnessPercents);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_CONTRAST_PERCENTS, contrastPercents);
        }
        private static void SetWIAProperty(IProperties properties, object propName, object propValue)
        {
            WIA.Property prop = properties.get_Item(ref propName);
            prop.set_Value(ref propValue);
        }
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        int Width = 1;
        int Height = 1;
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath;
            System.Drawing.Image img = System.Drawing.Image.FromFile(imagepath);
            Width = (int)img.PhysicalDimension.Width + 100;
            Height = (int)img.PhysicalDimension.Height + 100;

            addpath = @"\Reports\image\image.rdlc";


            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.EnableExternalImages = true;
            // paramarr.Add(new ReportParameter("logoImage", "file:\\" + rep.GetLogoImagePath()));
            paramarr.Add(new ReportParameter("logoImage", "file:\\" + @imagepath));
            rep.SetParameters(paramarr);

            rep.Refresh();
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BuildReport();
                //   D:\mailtemp\images\image-122.png

                saveFileDialog.Filter = "PDF|*.pdf;";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;

                    LocalReportExtensions.customExportToPDFwh(rep, filepath, Width, Height);
                }


                // LocalReportExtensions.customExportbyPrinter(rep, filepath);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {//update
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                validateDocValues();
                if (!tb_name.Text.Equals(""))
                {
                    docImgModel.tableName = tableName;
                    docImgModel.tableId = tableId;
                    docImgModel.docnum = docNum;
                    docImgModel.docName = tb_name.Text;
                    docImgModel.note = tb_notes.Text;
                    docImgModel.updateUserId = MainWindow.userID;

                   int res = (int)await docImgModel.saveDocImage(docImgModel);
                    if (!res.Equals(0))
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                    else
                        Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                    int docImageId = res;

                    if (openFileDialog.FileName != "")
                        //    await docImgModel.uploadImage(openFileDialog.FileName, tableName, docImageId);
                        await docImgModel.uploadOrginalImage(openFileDialog.FileName, tableName, docImageId);

                    //refresh image list
                    await refreshImageList();
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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception)
            {

            }
        }
        private void HandleKeyPress(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                {
                    switch (e.Key)
                    {
                        case Key.P:
                            //handle D key
                            Btn_print_Click(null, null);
                            break;
                        case Key.S:
                            //handle X key
                            Btn_save_Click(btn_save, null);
                            break;
                    }
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

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BuildReport();
                //   D:\mailtemp\images\image-122.png

                //  LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(MainWindow.rep_print_count));
                LocalReportExtensions.customPrintToPrinterwh(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count), Width, Height);



            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window.GetWindow(this).Opacity = 0.2;

                string pdfpath = "";

                //
                pdfpath = @"\Thumb\report\temp.pdf";
                pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                BuildReport();
                LocalReportExtensions.customExportToPDFwh(rep, pdfpath, Width, Height);
                wd_previewPdf w = new wd_previewPdf();
                w.pdfPath = pdfpath;
                if (!string.IsNullOrEmpty(w.pdfPath))
                {
                    w.ShowDialog();
                    w.wb_pdfWebViewer.Dispose();


                }
                Window.GetWindow(this).Opacity = 1;
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
