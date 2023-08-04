using LiveCharts.Wpf;
using Microsoft.Win32;
using netoaster;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace POS.Classes
{
    public class PrintingChart
    {
        static Image image;
        static public void Print(CartesianChart cartesianChart, Window window)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF|*.pdf;";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filepath = saveFileDialog.FileName;
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)cartesianChart.ActualWidth, (int)cartesianChart.ActualHeight, 93, 93, PixelFormats.Pbgra32);
                rtb.Render(cartesianChart);

                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));
                MemoryStream stream = new MemoryStream();
                png.Save(stream);
                image = Image.FromStream(stream);

                if (!AppSettings.lang.Equals("en"))
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);


                //ReportCls.pdfChart(image);

                //PrintDocument pd = new PrintDocument();
                //pd.PrintPage += PrintPage;
                //pd.Print();



                ReportCls reportclass = new ReportCls();
                string imgepath = @"\Reports\image\chartimg.png";
                string pdfpath = @"\Reports\image\chartimg.pdf";
                imgepath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, imgepath);
                pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                image.Save(imgepath);

             
                PdfHelper.Instance.SaveImageAsPdf(imgepath, filepath, 900, true);

                Toaster.ShowSuccess(window, message: MainWindow.resourcemanager.GetString("savedSuccessfully"), animation: ToasterAnimation.FadeIn);

            }
        }

            static public void Print(PieChart pieChart, Window window)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF|*.pdf;";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filepath = saveFileDialog.FileName;
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)pieChart.ActualWidth, (int)pieChart.ActualHeight, 93, 93, PixelFormats.Pbgra32);
                rtb.Render(pieChart);

                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));
                MemoryStream stream = new MemoryStream();
                png.Save(stream);
                image = Image.FromStream(stream);

                if (!AppSettings.lang.Equals("en"))
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);


                //ReportCls.pdfChart(image);

                //PrintDocument pd = new PrintDocument();
                //pd.PrintPage += PrintPage;
                //pd.Print();



                ReportCls reportclass = new ReportCls();
                string imgepath = @"\Reports\image\chartimg.png";
                string pdfpath = @"\Reports\image\chartimg.pdf";
                imgepath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, imgepath);
                pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                image.Save(imgepath);

                PdfHelper.Instance.SaveImageAsPdf(imgepath, filepath, 900, true);

                Toaster.ShowSuccess(window, message: MainWindow.resourcemanager.GetString("savedSuccessfully"), animation: ToasterAnimation.FadeIn);


            }
        }

       
        /*
        static private void PrintPage(object o, PrintPageEventArgs e)
        {
            Point loc = new Point(100, 100);
            e.Graphics.DrawImage(image, loc);
        }
        */
    }
}
