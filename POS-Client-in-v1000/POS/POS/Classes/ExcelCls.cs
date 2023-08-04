using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using xl = Microsoft.Office.Interop.Excel;

namespace POS.Classes
{
    public class ExcelCls
    {

        public List<string> readFile(string path)
        {
            // path = @"D:\ee.xlsx";
            // path = @"D:\ee.csv";
            List<string> serialstr = new List<string>();
     //      path = @"D:\ee.xls";
            xl.Application excel = new xl.Application();
            Workbook wb;
            Worksheet ws;
            xl.Workbooks woorkboks = excel.Workbooks;
            wb =  woorkboks.Open(path);
            ws = wb.Worksheets[1];
            //ws.Columns.ClearFormats();
            //ws.Rows.ClearFormats();
            Range cellrange = ws.UsedRange.Columns[1];
          //  cellrange = cellrange.CurrentArray;
            //string cellValue = "";
            if (cellrange.Rows.Count>1)
            {
                for (int i = 2; i <= cellrange.Rows.Count; i++)
                {
                    string NameValue = Convert.ToString((cellrange.Cells[i, 1] as xl.Range).Value);
                    if (NameValue != "" && NameValue != null)
                    {
                        serialstr.Add(NameValue);
                     //   cellValue += NameValue + " - ";
                    }

                }
            }
          
       //     string cellValue = Convert.ToString((cellrange.Cells["$A"] as xl.Range).Value2);

           // string cellValue = cell.Value;
            //close
            wb.Close(false, path, null); // Close the connection to workbook                                        //   Marshal.FinalReleaseComObject(workbook); // Release unmanaged object references.
            wb = null;
            excel.Workbooks.Close();
            // Marshal.FinalReleaseComObject(workbooks);
            woorkboks  = null;
            excel.Quit();
            //  Marshal.FinalReleaseComObject(xlApp);
            excel = null;
            return serialstr;
        }

    }
}
