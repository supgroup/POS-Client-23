using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims; 
using System.Web;

namespace POS.Classes
{
    public class InvoiceTaxes
    {
        #region properties
        public int invoiceTaxId { get; set; }
        public Nullable<int> taxId { get; set; }
        public Nullable<decimal> rate { get; set; }
        public Nullable<decimal> taxValue { get; set; }
        public string notes { get; set; }
        public Nullable<int> invoiceId { get; set; }
        public string taxType { get; set; }
        public string name { get; set; }
        public string nameAr { get; set; }
        #endregion

    }
}
