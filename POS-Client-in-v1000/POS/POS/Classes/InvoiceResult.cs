using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Classes
{
   public class InvoiceResult
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public string InvTime { get; set; }
        public DateTime? UpdateDate { get; set; }

        public decimal PosBalance { get; set; }
        public int SalesDraftCount { get; set; }
        public int InvoiceCount { get; set; }
        public int OrdersCount { get; set; }
        public int SalesWaitingOrdersCount { get; set; }
        public int SalesQuotationCount { get; set; }
        public int PurchaseDraftCount { get; set; }
        public int ImExpDraftCount { get; set; }
        public int WaitngExportCount { get; set; }
        public int PaymentsCount { get; set; }

        public string isThereLack { get; set; }
        public string isThereInventoryDraft { get; set; }
        public string isThereSavedInventory { get; set; }
    }
}
