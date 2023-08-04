using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS.Classes
{
    class rptSectionData
    {
        public static void setReportLanguage(List<ReportParameter> paramarr)
        {
            paramarr.Add(new ReportParameter("lang", AppSettings.Reportlang));
        }


        public static void Header(List<ReportParameter> paramarr)
        {
          
           // List<ReportParameter> listParam = new List<ReportParameter>();

            ReportCls rep = new ReportCls();
            paramarr.Add(new ReportParameter("companyName", AppSettings.companyName));
            paramarr.Add(new ReportParameter("Fax", AppSettings.Fax));
            paramarr.Add(new ReportParameter("Tel", AppSettings.Mobile));
            paramarr.Add(new ReportParameter("Address", AppSettings.Address));
            paramarr.Add(new ReportParameter("Email", AppSettings.Email));
            paramarr.Add(new ReportParameter("logoImage", "file:\\" + rep.GetLogoImagePath()));
            paramarr.Add(new ReportParameter("show_header", AppSettings.show_header));


        }
        public static void bankReport(IEnumerable<Bank> banksQuery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetBank", banksQuery));
        }

        public static void posReport(IEnumerable<Pos> possQuery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetPos", possQuery));
        }

        public static void customerReport(IEnumerable<Agent> customersQuery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("AgentDataSet", customersQuery));
        }

        public static void branchReport(IEnumerable<Branch> branchQuery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetBranches", branchQuery));
        }

        public static void userReport(IEnumerable<User> usersQuery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetUser", usersQuery));
        }

        public static void vendorReport(IEnumerable<Agent> vendorsQuery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("AgentDataSet", vendorsQuery));
        }

        public static void storeReport(IEnumerable<Branch> storesQuery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetBranches", storesQuery));
        }

    }
}
