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
using Microsoft.Reporting.WinForms;
using System.IO;

namespace POS.Classes
{
    public class ErrorClass
    {

        public int errorId { get; set; }
        public string num { get; set; }
        public string msg { get; set; }
        public string stackTrace { get; set; }
        public string targetSite { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public string programNamePos { get; set; }
        public string versionNamePos { get; set; }
        public string programNameServer { get; set; }
        public string versionNameServer { get; set; }
        public string source { get; set; }
        public string method { get; set; }

        
            

        /// <summary>
        /// ///////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        /// 
        public async Task<List<ErrorClass>> Get()
        {
            List<ErrorClass> items = new List<ErrorClass>();
            IEnumerable<Claim> claims = await APIResult.getList("errorcontroller/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<ErrorClass>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<ErrorClass> getById(int itemId)
        {
            ErrorClass item = new ErrorClass();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("errorcontroller/GetByID", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<ErrorClass>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }
        public async Task<List<ErrorClass>> GetByPos(int itemId)
        {
            List<ErrorClass> items = new List<ErrorClass>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("errorcontroller/GetByPos", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<ErrorClass>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<decimal> save(ErrorClass item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "errorcontroller/Save";
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
            return await APIResult.post(method, parameters);
        }
        public async Task<decimal> delete(int errorId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", errorId.ToString());
            string method = "errorcontroller/Delete";
            return await APIResult.post(method, parameters);
        }
        public async Task<string> saveTodayErrors()
        {
            try
            {
                string pdfpath = "";
                string DestPath = "";
                ReportCls reportclass = new ReportCls();
                //saveFileDialog.Filter = "File|*.er;";
                string ErrorFolder = @"\ErrorsFolder\";
                string newfile = DateTime.Now.ToString("dd-MM-yyyy") + ".er";
                DestPath = ErrorFolder + newfile;
                DestPath = reportclass.PathUp(Directory.GetCurrentDirectory(), 0, DestPath);
                if (!File.Exists(DestPath))
                {          
                    LocalReport rep = new LocalReport();
                    List<ReportParameter> paramarr = new List<ReportParameter>();
                    string addpath;
                    bool isArabic = ReportCls.checkLang();
                    pdfpath = @"\Thumb\report\temp2.pdf";
                    pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                    addpath = @"\Reports\image\error.rdlc";
                    string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
                    List<ErrorClass> eList = new List<ErrorClass>();
                    ErrorClass errorModel = new ErrorClass();
                    eList = await errorModel.GetByPos((int)MainWindow.posID);
                    clsReports.ErrorsReport(eList, rep, reppath);
                    //  clsReports.setReportLanguage(paramarr);
                    clsReports.HeaderNoLogo(paramarr);
                    rep.SetParameters(paramarr);
                    rep.Refresh();
                    bool res = false;
                    LocalReportExtensions.ExportToExcel(rep, pdfpath);
                    res = reportclass.encodefile(pdfpath, DestPath);
                    reportclass.DelFile(pdfpath);
                }

                return "1";
            }
            catch (Exception ex)
            {
                return "0";
            }

        }
    }
}
