﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Mime;

using System.Net;
using System.IO;
namespace POS.Classes
{
    class MailimageClass
    {
        public string path { get; set; }
        public string imageId { get; set; }
        public string objectId { get; set; }

    }
    class EmailClass
    {
        public string smtpclient { get; set; }
        public string from { get; set; }

        public string password { get; set; }
        public List<string> Toemails = new List<string>();
        public List<string> CCemails = new List<string>();
        public List<string> BCCemails = new List<string>();
        public string subject { get; set; }
        public List<string> attachfiles = new List<string>();

        public string body { get; set; }
        public int port { get; set; }
        public bool isSSl { get; set; }

        public static string force_email = "no";
        public AlternateView htmlView;



        public string Sendmail()
        {
            string res = "";

            try
            {

                MailMessage mail = new MailMessage();

                SmtpClient Smtpserver = new SmtpClient(smtpclient);
                mail.From = new MailAddress(from);

                if (Toemails != null)
                {
                    foreach (string to in Toemails)
                    {
                        mail.To.Add(to);
                    }
                }
                if (CCemails != null)
                {
                    foreach (string ccto in CCemails)
                    {
                        mail.CC.Add(ccto);
                    }
                }
                if (BCCemails != null)
                {
                    foreach (string bcc in BCCemails)
                    {
                        mail.Bcc.Add(bcc);
                    }
                }
                if (attachfiles != null)
                {


                    foreach (string attachfile in attachfiles)
                    {
                        Attachment attachment = new Attachment(attachfile);

                        mail.Attachments.Add(attachment);
                    }
                }

                // replace placeholder


                mail.Subject = subject;
                mail.IsBodyHtml = true;
                //  mail.BodyEncoding = Encoding.UTF8;
                mail.Body = body;

                if (htmlView != null)
                {
                    mail.AlternateViews.Add(htmlView);
                }


                Smtpserver.Port = port;
                Smtpserver.Credentials = new System.Net.NetworkCredential(from, password);
                Smtpserver.EnableSsl = isSSl;
                Smtpserver.Send(mail);
                res = "mailsent";
                return res;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }


            // System.Threading.Thread.Sleep(1000);

        }

        public void AddTolist(string value)
        {

            Toemails.Add(value);

        }
        public void AddrangeTolist(List<string> value)
        {

            Toemails.AddRange(value);
        }

        public void AddAttachTolist(string value)
        {

            attachfiles.Add(value);

        }
        public void AddAttachrangeTolist(List<string> value)
        {
            attachfiles.AddRange(value);
        }
        public void AddCCTolist(string value)
        {

            CCemails.Add(value);

        }
        public void AddCCrangeTolist(List<string> value)
        {

            CCemails.AddRange(value);
        }
        public void AddBCCTolist(string value)
        {

            BCCemails.Add(value);

        }
        public void AddBCCrangeTolist(List<string> value)
        {

            BCCemails.AddRange(value);
        }
        public void ResetBCClist()
        {
            BCCemails = new List<string>();

        }
        public void ResetCClist()
        {
            CCemails = new List<string>();

        }
        public void ResetTolist()
        {
            Toemails = new List<string>();

        }
        public void Resetattachfileslist()
        {
            attachfiles = new List<string>();

        }


        public LinkedResource Linkimage(string path, string ContentId)
        {

            LinkedResource LinkedImage = new LinkedResource(@path);
            LinkedImage.ContentId = ContentId;
            //Added the patch for Thunderbird as suggested by Jorge
            LinkedImage.ContentType = new ContentType(MediaTypeNames.Image.Jpeg);
            return LinkedImage;
        }


        public EmailClass fillOrderTempData(Invoice invoice, List<ItemTransfer> invoiceItems, SysEmails email, Agent toAgent, List<SetValues> setvlist)
        {
            string invheader;
            string invfooter;
            string invbody;
            string invitemtable;
            string invitemrow;

            EmailClass mailtosend = new EmailClass();

            mailtosend.from = email.email;
            mailtosend.smtpclient = email.smtpClient;
            mailtosend.port = (int)email.port;

            mailtosend.password = Encoding.UTF8.GetString(Convert.FromBase64String(email.password));
            mailtosend.isSSl = (bool)email.isSSL;
            mailtosend.AddTolist(toAgent.email);
            mailtosend.subject = "Order" + DateTime.Now.ToString();



            // data
            ReportCls repm = new ReportCls();
            List<MailimageClass> imgs = new List<MailimageClass>();
            MailimageClass img = new MailimageClass();
          string isArabic = ReportCls.checkInvLang();

            decimal disval = repm.calcpercentval(invoice.discountType, invoice.discountValue, invoice.total);
            decimal manualdisval = repm.calcpercentval(invoice.manualDiscountType, invoice.manualDiscountValue, invoice.total);
            invoice.discountValue = disval + manualdisval;
            invoice.discountType = "1";
            if (isArabic=="ar")
            {
                invheader = repm.ReadFile(@"EmailTemplates\ordertemplate\ar\invheader.tmp");
                invfooter = repm.ReadFile(@"EmailTemplates\ordertemplate\ar\invfooter.tmp");
                invbody = repm.ReadFile(@"EmailTemplates\ordertemplate\ar\invbody.tmp");
                invitemtable = repm.ReadFile(@"EmailTemplates\ordertemplate\ar\invitemtable.tmp");
                invitemrow = repm.ReadFile(@"EmailTemplates\ordertemplate\ar\invitemrow.tmp");


            }
            else
            { // en
                invheader = repm.ReadFile(@"EmailTemplates\ordertemplate\en\invheader.tmp");
                invfooter = repm.ReadFile(@"EmailTemplates\ordertemplate\en\invfooter.tmp");
                invbody = repm.ReadFile(@"EmailTemplates\ordertemplate\en\invbody.tmp");
                invitemtable = repm.ReadFile(@"EmailTemplates\ordertemplate\en\invitemtable.tmp");
                invitemrow = repm.ReadFile(@"EmailTemplates\ordertemplate\en\invitemrow.tmp");

            }


            //header info

            invheader = invheader.Replace("[[companyname]]", AppSettings.companyName.Trim());
            invheader = invheader.Replace("[[phone]]", AppSettings.Phone.Trim());
            invheader = invheader.Replace("[[Email]]", AppSettings.Email.Trim());
            invheader = invheader.Replace("[[fax]]", AppSettings.Fax.Trim());
            invheader = invheader.Replace("[[address]]", AppSettings.Address.Trim());
            invheader = invheader.Replace("[[trphone]]", MainWindow.resourcemanagerreport.GetString("trPhone").Trim() + ": ");
            invheader = invheader.Replace("[[trfax]]", MainWindow.resourcemanagerreport.GetString("trFax").Trim() + ": ");
            invheader = invheader.Replace("[[traddress]]", MainWindow.resourcemanagerreport.GetString("trAddress").Trim() + ": ");


            //BODY

            // string title = "Purchase Order";
            string title = setvlist.Where(x => x.notes == "title").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "title").FirstOrDefault().value.ToString();
            invheader = invheader.Replace("[[title]]", title.Trim());

            invbody = invbody.Replace("[[thankstitle]]", title);
            //   string thankstext = "Please provide to us,with a price list,along with your terms and conditions of sale, applicable discounts, shipping dates and additional sales and corporate policies. Should the information you provide be acceptable and competitive. ";
            string thankstext = setvlist.Where(x => x.notes == "text1").FirstOrDefault() is null ? ""
                  : setvlist.Where(x => x.notes == "text1").FirstOrDefault().value.ToString();
            invbody = invbody.Replace("[[thankstext]]", thankstext);


            if (invoice.invoiceId > 0)
            {
                invbody = invbody.Replace("[[invoicecode]]", invoice.invNumber);
                invbody = invbody.Replace("[[invoicedate]]", repm.DateToString(invoice.invDate));
                //invbody = invbody.Replace("[[invoicetotal]]", invoice.total.ToString());
                invbody = invbody.Replace("[[invoicetotal]]", repm.DecTostring(invoice.total));
                //invbody = invbody.Replace("[[invoicediscount]]", invoice.discountValue.ToString());
                invbody = invbody.Replace("[[invoicediscount]]", repm.DecTostring(invoice.discountValue));
                //invbody = invbody.Replace("[[invoicetax]]", invoice.tax.ToString());
                invbody = invbody.Replace("[[invoicetax]]", repm.DecTostring(invoice.tax));
                //invbody = invbody.Replace("[[totalnet]]", invoice.totalNet.ToString());
                invbody = invbody.Replace("[[totalnet]]", repm.DecTostring(invoice.totalNet));
            }

            //  invoiceItems.

            invitemtable = invitemtable.Replace("[[tritems]]", MainWindow.resourcemanagerreport.GetString("trItem").Trim());
            invitemtable = invitemtable.Replace("[[trunit]]", MainWindow.resourcemanagerreport.GetString("trUnit").Trim());
            invitemtable = invitemtable.Replace("[[trquantity]]", MainWindow.resourcemanagerreport.GetString("trQTR").Trim());
            invitemtable = invitemtable.Replace("[[trtotalrow]]", MainWindow.resourcemanagerreport.GetString("trPrice").Trim());

            invbody = invbody.Replace("[[trinvoicecode]]", MainWindow.resourcemanagerreport.GetString("trInvoiceNumber").Trim() + ": ");
            invbody = invbody.Replace("[[trinvoicedate]]", MainWindow.resourcemanagerreport.GetString("trDate").Trim() + ": ");
            invbody = invbody.Replace("[[trinvoicetotal]]", MainWindow.resourcemanagerreport.GetString("trSum").Trim() + ": ");
            invbody = invbody.Replace("[[currency]]", AppSettings.Currency);
            //
            invbody = invbody.Replace("[[trinvoicediscount]]", MainWindow.resourcemanagerreport.GetString("trDiscount").Trim() + ": ");

            invbody = invbody.Replace("[[trinvoicetax]]", MainWindow.resourcemanagerreport.GetString("trTax").Trim() + ": ");

            invbody = invbody.Replace("[[trtotalnet]]", MainWindow.resourcemanagerreport.GetString("trTotal").Trim() + ": ");

            // string invoicenote = "Thank you for your cooperation. We have also enclosed our procurement specifications and conditions for your review <br/> Sincerely";
            string invoicenote = setvlist.Where(x => x.notes == "text2").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "text2").FirstOrDefault().value.ToString();
            invbody = invbody.Replace("[[invoicenote]]", invoicenote);
            string link1 = setvlist.Where(x => x.notes == "link1text").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "link1text").FirstOrDefault().value.ToString();

            string link2 = setvlist.Where(x => x.notes == "link2text").FirstOrDefault() is null ? ""
                 : setvlist.Where(x => x.notes == "link2text").FirstOrDefault().value.ToString();
            string link3 = setvlist.Where(x => x.notes == "link3text").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "link3text").FirstOrDefault().value.ToString();

            invfooter = invfooter.Replace("[[support]]", link1);
            invfooter = invfooter.Replace("[[returnpolicy]]", link2);
            invfooter = invfooter.Replace("[[aboutus]]", link3);
            string link1url = setvlist.Where(x => x.notes == "link1url").FirstOrDefault() is null ? ""
                       : setvlist.Where(x => x.notes == "link1url").FirstOrDefault().value.ToString();
            string link2url = setvlist.Where(x => x.notes == "link2url").FirstOrDefault() is null ? ""
                       : setvlist.Where(x => x.notes == "link2url").FirstOrDefault().value.ToString();
            string link3url = setvlist.Where(x => x.notes == "link3url").FirstOrDefault() is null ? ""
                       : setvlist.Where(x => x.notes == "link3url").FirstOrDefault().value.ToString();

            invfooter = invfooter.Replace("[[supporturl]]", link1url);
            invfooter = invfooter.Replace("[[returnpolicyurl]]", link2url);
            invfooter = invfooter.Replace("[[aboutusurl]]", link3url);

            invfooter = invfooter.Replace("[[year]]", DateTime.Now.Year.ToString());



            //  invitemtable
            // foreach
            string datarows = "";
            foreach (ItemTransfer row in invoiceItems)
            {
                string rowhtml = invitemrow;
                rowhtml = rowhtml.Replace("[[col1]]", row.itemName.Trim());
                rowhtml = rowhtml.Replace("[[col2]]", row.unitName.Trim());
                rowhtml = rowhtml.Replace("[[col3]]", row.quantity.ToString());
                //     rowhtml = rowhtml.Replace("[[col4]]", (row.quantity * row.price).ToString());
                rowhtml = rowhtml.Replace("[[col4]]", "");
                datarows += rowhtml;

            }
            invitemtable = invitemtable.Replace("[[invitemrow]]", datarows);
            // end foreach
            invbody = invbody.Replace("[[invitemtable]]", invitemtable);

            string mailbody = invheader + invbody + invfooter;



            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailbody, null, "text/html");
            string testpath = repm.GetPath(@"EmailTemplates\mail.html");
            //
            if (!File.Exists(testpath))
            {
                // Create a file to write to.
                string createText = mailbody;
                File.WriteAllText(testpath, createText);
            }
            else
            {
                File.Delete(testpath);
                // Create a file to write to.
                string createText = mailbody;
                File.WriteAllText(testpath, createText);
            }



            img.path = repm.GetLogoImagePath();
            img.imageId = "logo";
            imgs.Add(img);
            img = new MailimageClass();
            img.path = repm.GetPath(@"EmailTemplates\images\image-2.gif");

            img.imageId = "image-2";
            imgs.Add(img);

            foreach (MailimageClass row in imgs)
            {
                htmlView.LinkedResources.Add(mailtosend.Linkimage(@row.path, row.imageId));
            }

            // 

            mailtosend.htmlView = htmlView;

            return mailtosend;


        }

        public EmailClass fillSaleTempData(Invoice invoice, List<ItemTransfer> invoiceItems, List<PayedInvclass> mailpayedList, SysEmails email, Agent toAgent, List<SetValues> setvlist)
        {

            string invheader = "";
            string invfooter = "";
            string invbody = "";
            string invitemtable = "";
            string invitemrow = "";
            string paytable = "";
            string payrow = "";
            string taxdiv = "";
            string deliverydiv = "";
            string paiddiv = "";
            string unpaiddiv = "";
            string ondeliverydiv = "";
            string remaindiv = "";
            //payrow.tmp
            //    paytable.tmp
            EmailClass mailtosend = new EmailClass();
            ReportCls reportclass = new ReportCls();

            mailtosend.from = email.email;
            mailtosend.smtpclient = email.smtpClient;
            mailtosend.port = (int)email.port;

            mailtosend.password = Encoding.UTF8.GetString(Convert.FromBase64String(email.password));
            mailtosend.isSSl = (bool)email.isSSL;
            mailtosend.AddTolist(toAgent.email);
            string cashTr = "";
            string sumP = "";
            string deservedcash = "";

            // data
            ReportCls repm = new ReportCls();
            List<MailimageClass> imgs = new List<MailimageClass>();
            MailimageClass img = new MailimageClass();
            decimal disval = repm.calcpercentval(invoice.discountType, invoice.discountValue, invoice.total);
            decimal manualdisval = repm.calcpercentval(invoice.manualDiscountType, invoice.manualDiscountValue, invoice.total);
            invoice.discountValue = disval + manualdisval;
            invoice.discountType = "1";
           string isArabic = ReportCls.checkInvLang();
            if (isArabic=="ar")
            {
                invheader = repm.ReadFile(@"EmailTemplates\ordertemplate\ar\invheader.tmp");
                invfooter = repm.ReadFile(@"EmailTemplates\ordertemplate\ar\invfooter.tmp");
                deliverydiv = repm.ReadFile(@"EmailTemplates\saletemplate\ar\deliverydiv.tmp");
                if (invoice.invType == "s" || invoice.invType == "pw" || invoice.invType == "p")
                {
                    invbody = repm.ReadFile(@"EmailTemplates\saletemplate\ar\invbody.tmp");
                    invitemtable = repm.ReadFile(@"EmailTemplates\saletemplate\ar\invitemtable.tmp");
                    invitemrow = repm.ReadFile(@"EmailTemplates\saletemplate\ar\invitemrow.tmp");

                    paytable = repm.ReadFile(@"EmailTemplates\saletemplate\ar\paytable.tmp");
                    payrow = repm.ReadFile(@"EmailTemplates\saletemplate\ar\payrow.tmp");
                    taxdiv = repm.ReadFile(@"EmailTemplates\saletemplate\ar\taxdiv.tmp");
                    //
                    paiddiv = repm.ReadFile(@"EmailTemplates\saletemplate\ar\paiddiv.tmp");
                    unpaiddiv = repm.ReadFile(@"EmailTemplates\saletemplate\ar\unpaiddiv.tmp");
                    remaindiv = repm.ReadFile(@"EmailTemplates\saletemplate\ar\remaindiv.tmp");
                    ondeliverydiv = repm.ReadFile(@"EmailTemplates\saletemplate\ar\ondeliverydiv.tmp");

                }
                else if (invoice.invType == "or" || invoice.invType == "ors")
                {
                    invbody = repm.ReadFile(@"EmailTemplates\saleordertemplate\ar\invbody.tmp");
                    invitemtable = repm.ReadFile(@"EmailTemplates\saleordertemplate\ar\invitemtable.tmp");
                    invitemrow = repm.ReadFile(@"EmailTemplates\saleordertemplate\ar\invitemrow.tmp");
                }
                else if (invoice.invType == "q" || invoice.invType == "qs")
                {
                    invbody = repm.ReadFile(@"EmailTemplates\quotationtemplate\ar\invbody.tmp");
                    invitemtable = repm.ReadFile(@"EmailTemplates\quotationtemplate\ar\invitemtable.tmp");
                    invitemrow = repm.ReadFile(@"EmailTemplates\quotationtemplate\ar\invitemrow.tmp");
                }
            }
            else
            { // en
                invheader = repm.ReadFile(@"EmailTemplates\ordertemplate\en\invheader.tmp");
                invfooter = repm.ReadFile(@"EmailTemplates\ordertemplate\en\invfooter.tmp");
                deliverydiv = repm.ReadFile(@"EmailTemplates\saletemplate\en\deliverydiv.tmp");
                if (invoice.invType == "s" || invoice.invType == "pw" || invoice.invType == "p")
                {

                    invbody = repm.ReadFile(@"EmailTemplates\saletemplate\en\invbody.tmp");
                    invitemtable = repm.ReadFile(@"EmailTemplates\saletemplate\en\invitemtable.tmp");
                    invitemrow = repm.ReadFile(@"EmailTemplates\saletemplate\en\invitemrow.tmp");

                    paytable = repm.ReadFile(@"EmailTemplates\saletemplate\en\paytable.tmp");
                    payrow = repm.ReadFile(@"EmailTemplates\saletemplate\en\payrow.tmp");

                    taxdiv = repm.ReadFile(@"EmailTemplates\saletemplate\en\taxdiv.tmp");
                    //
                    paiddiv = repm.ReadFile(@"EmailTemplates\saletemplate\en\paiddiv.tmp");
                    unpaiddiv = repm.ReadFile(@"EmailTemplates\saletemplate\en\unpaiddiv.tmp");
                    remaindiv = repm.ReadFile(@"EmailTemplates\saletemplate\en\remaindiv.tmp");
                    ondeliverydiv = repm.ReadFile(@"EmailTemplates\saletemplate\en\ondeliverydiv.tmp");
                }
                else if (invoice.invType == "or" || invoice.invType == "ors")
                {
                    invbody = repm.ReadFile(@"EmailTemplates\saleordertemplate\en\invbody.tmp");
                    invitemtable = repm.ReadFile(@"EmailTemplates\saleordertemplate\en\invitemtable.tmp");
                    invitemrow = repm.ReadFile(@"EmailTemplates\saleordertemplate\en\invitemrow.tmp");
                }
                else if (invoice.invType == "q" || invoice.invType == "qs")
                {
                    invbody = repm.ReadFile(@"EmailTemplates\quotationtemplate\en\invbody.tmp");
                    invitemtable = repm.ReadFile(@"EmailTemplates\quotationtemplate\en\invitemtable.tmp");
                    invitemrow = repm.ReadFile(@"EmailTemplates\quotationtemplate\en\invitemrow.tmp");
                }
                else
                {
                    invbody = repm.ReadFile(@"EmailTemplates\saletemplate\en\invbody.tmp");
                    invitemtable = repm.ReadFile(@"EmailTemplates\saletemplate\en\invitemtable.tmp");
                    invitemrow = repm.ReadFile(@"EmailTemplates\saletemplate\en\invitemrow.tmp");

                    paytable = repm.ReadFile(@"EmailTemplates\saletemplate\en\paytable.tmp");
                    payrow = repm.ReadFile(@"EmailTemplates\saletemplate\en\payrow.tmp");
                }
            }
            //header info
            invheader = invheader.Replace("[[companyname]]", AppSettings.companyName.Trim());
            invheader = invheader.Replace("[[phone]]", AppSettings.Phone.Trim());
            invheader = invheader.Replace("[[Email]]", AppSettings.Email.Trim());
            invheader = invheader.Replace("[[fax]]", AppSettings.Fax.Trim());
            invheader = invheader.Replace("[[address]]", AppSettings.Address.Trim());
            invheader = invheader.Replace("[[trphone]]", MainWindow.resourcemanagerreport.GetString("trPhone").Trim() + ": ");
            invheader = invheader.Replace("[[trfax]]", MainWindow.resourcemanagerreport.GetString("trFax").Trim() + ": ");
            invheader = invheader.Replace("[[traddress]]", MainWindow.resourcemanagerreport.GetString("trAddress").Trim() + ": ");
            //BODY
            // string title = "Purchase Order";
            string title = setvlist.Where(x => x.notes == "title").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "title").FirstOrDefault().value.ToString();
            mailtosend.subject = title.Trim();
            invheader = invheader.Replace("[[title]]", title.Trim());
            invbody = invbody.Replace("[[thankstitle]]", title);
            //   string thankstext = "Please provide to us,with a price list,along with your terms and conditions of sale, applicable discounts, shipping dates and additional sales and corporate policies. Should the information you provide be acceptable and competitive. ";
            string thankstext = setvlist.Where(x => x.notes == "text1").FirstOrDefault() is null ? ""
                  : setvlist.Where(x => x.notes == "text1").FirstOrDefault().value.ToString();
            invbody = invbody.Replace("[[thankstext]]", thankstext);
            if (invoice.invoiceId > 0)
            {

                if ((invoice.invType == "s" || invoice.invType == "sd" || invoice.invType == "sbd" || invoice.invType == "sb" || invoice.invType == "p" || invoice.invType == "pw"))
                {
                    decimal sump = mailpayedList.Sum(x => x.cash).Value;
                    //decimal deservd = (decimal)invoice.totalNet - sump;
                    decimal deservd = (decimal)invoice.totalNet - sump + invoice.cashReturn;
                    cashTr = MainWindow.resourcemanagerreport.GetString("trCashType");
                    //paid
                    if (sump == 0)
                    {
                        invbody = invbody.Replace("[[paiddiv]]", "");
                    }
                    else
                    {
                        sumP = reportclass.DecTostring(sump);

                        paiddiv = paiddiv.Replace("[[payedsum]]", sumP);
                        paiddiv = paiddiv.Replace("[[trPaid]]", MainWindow.resourcemanagerreport.GetString("trCashPaid"));
                        paiddiv = paiddiv.Replace("[[currency]]", AppSettings.Currency);
                        invbody = invbody.Replace("[[paiddiv]]", paiddiv);
                    }

                    //end paid
                    //Unpaid
                    if ((invoice.shippingCompanyId == 0 || (invoice.shippingCompanyId > 0 && invoice.isPrePaid == 1)) && (deservd != 0))
                    {
                        //show
                        //=iif((Parameters!shippingCompanyId.Value = 0 Or Parameters!shippingCompanyId.Value = "0" or( Parameters!shippingCompanyId.Value >0 and Parameters!isPrePaid.Value=1 ) )and Parameters!deserved.Value<>0 , false, true)                       

                        deservedcash = reportclass.DecTostring(deservd);
                        unpaiddiv = unpaiddiv.Replace("[[deservedcash]]", deservedcash);
                        unpaiddiv = unpaiddiv.Replace("[[trUnpaid]]", MainWindow.resourcemanagerreport.GetString("trUnPaidCash"));
                        unpaiddiv = unpaiddiv.Replace("[[currency]]", AppSettings.Currency);
                        invbody = invbody.Replace("[[unpaiddiv]]", unpaiddiv);
                    }
                    else
                    {
                        //hide
                        invbody = invbody.Replace("[[unpaiddiv]]", "");
                    }
                    //end Unpaid
                    //OnDelivery
                    if (invoice.shippingCompanyId == 0|| deservd==0 || invoice.isPrePaid == 1)
                    {
                        // =iif(Parameters!shippingCompanyId.Value = 0 Or Parameters!shippingCompanyId.Value = "0" Or Parameters!deserved.Value=0 or Parameters!isPrePaid.Value=1, True, False)
                        //hide
                        invbody = invbody.Replace("[[ondeliverydiv]]", "");                        
                    }
                    else
                    {
                        //show

                        deservedcash = reportclass.DecTostring(deservd);
                        ondeliverydiv = ondeliverydiv.Replace("[[deservedcash]]", deservedcash);
                        ondeliverydiv = ondeliverydiv.Replace("[[trOnDelivery]]", MainWindow.resourcemanagerreport.GetString("OnDelivery"));
                        ondeliverydiv = ondeliverydiv.Replace("[[currency]]", AppSettings.Currency);
                        invbody = invbody.Replace("[[ondeliverydiv]]", ondeliverydiv);
                    }
                    //end OnDelivery
                    //remain
                    // 
                    if (invoice.cashReturn == 0)
                    {
                        //hide
                        invbody = invbody.Replace("[[remaindiv]]", "");
                    }
                    else
                    {
                        string remain = reportclass.DecTostring(invoice.cashReturn) == null ? "0" : reportclass.DecTostring(invoice.cashReturn);
                        remaindiv = remaindiv.Replace("[[remain]]", remain);
                        remaindiv = remaindiv.Replace("[[trRemain]]", MainWindow.resourcemanagerreport.GetString("trTheRemine").Trim());

                        invbody = invbody.Replace("[[remaindiv]]", remaindiv);
                    }
                    //end remain

                    //  paytable
                    // foreach
                    string datapayrows = "";
                    string paymethod = "";
                    payrow = payrow.Replace("[[currency]]", AppSettings.Currency);
                    foreach (PayedInvclass row in mailpayedList)
                    {
                        string rowhtml = payrow;
                        rowhtml = rowhtml.Replace("[[cashpayrow]]", reportclass.DecTostring(row.cash));
                        paymethod = row.processType == "cash" ? cashTr : row.cardName;
                        rowhtml = rowhtml.Replace("[[paymethodrow]]", paymethod);
                        datapayrows += rowhtml;
                    }
                    paytable = paytable.Replace("[[payrow]]", datapayrows);

                    // end foreach
                    invbody = invbody.Replace("[[paytable]]", paytable);
                }

                invbody = invbody.Replace("[[invoicecode]]", invoice.invNumber);
                invbody = invbody.Replace("[[invoicedate]]", repm.DateToString(invoice.invDate));
                //invbody = invbody.Replace("[[invoicetotal]]", invoice.total.ToString());
                invbody = invbody.Replace("[[invoicetotal]]", repm.DecTostring(invoice.total));
                //invbody = invbody.Replace("[[invoicediscount]]", invoice.discountValue.ToString());
                if (invoice.discountType == "2")
                {
                    if (isArabic=="ar")
                    {
                        invbody = invbody.Replace("[[invoicediscount]]", "% " + repm.DecTostring(invoice.discountValue));
                    }
                    else
                    {
                        invbody = invbody.Replace("[[invoicediscount]]", repm.DecTostring(invoice.discountValue) + " %");
                    }
                }
                else
                {
                    if (isArabic=="ar")
                    {
                        invbody = invbody.Replace("[[invoicediscount]]", AppSettings.Currency + " " + repm.DecTostring(invoice.discountValue));
                    }
                    else
                    {
                        invbody = invbody.Replace("[[invoicediscount]]", repm.DecTostring(invoice.discountValue) + " " + AppSettings.Currency);
                    }

                }
                //invbody = invbody.Replace("[[invoicetax]]", invoice.tax.ToString());
                if (invoice.tax == 0 || invoice.tax == null)
                {
                    invbody = invbody.Replace("[[invoicetax]]", repm.DecTostring(invoice.tax));
                    invbody = invbody.Replace("[[trinvoicetax]]", MainWindow.resourcemanagerreport.GetString("trTax").Trim());
                    invbody = invbody.Replace("[[taxdiv]]", "");
                }
                else
                {

                    taxdiv = taxdiv.Replace("[[invoicetax]]", repm.DecTostring(invoice.tax));
                    taxdiv = taxdiv.Replace("[[trinvoicetax]]", MainWindow.resourcemanagerreport.GetString("trTax").Trim());
                    invbody = invbody.Replace("[[taxdiv]]", taxdiv);
                }
                //shipping cost section
                if ((invoice.invType == "s" || invoice.invType == "or" || invoice.invType == "q" || invoice.invType == "qs" || invoice.invType == "ors"))
                {
                    deliverydiv = deliverydiv.Replace("[[trDelivery]]", MainWindow.resourcemanagerreport.GetString("trDelivery"));
                    if (invoice.shippingCost > 0)
                    {
                        if (invoice.isFreeShip == 1)
                        {
                            deliverydiv = deliverydiv.Replace("[[shippingcost]]", MainWindow.resourcemanagerreport.GetString("trFree").Trim());
                            deliverydiv = deliverydiv.Replace("[[totaldeserved]]", repm.DecTostring(invoice.totalNet));
                            deliverydiv = deliverydiv.Replace("[[currencyh]]", "");

                            invbody = invbody.Replace("[[totalnet]]", repm.DecTostring(invoice.totalNet));
                            invbody = invbody.Replace("[[deliverydiv]]", deliverydiv);
                        }
                        else
                        {
                            deliverydiv = deliverydiv.Replace("[[shippingcost]]", repm.DecTostring(invoice.shippingCost));
                            deliverydiv = deliverydiv.Replace("[[totaldeserved]]", repm.DecTostring(invoice.totalNet));
                            deliverydiv = deliverydiv.Replace("[[currencyh]]", AppSettings.Currency);
                            invbody = invbody.Replace("[[totalnet]]", repm.DecTostring(invoice.totalNet - invoice.shippingCost));
                            invbody = invbody.Replace("[[deliverydiv]]", deliverydiv);
                        }

                    }
                    else
                    {
                        invbody = invbody.Replace("[[deliverydiv]]", "");
                        invbody = invbody.Replace("[[totalnet]]", repm.DecTostring(invoice.totalNet));
                    }
                }else if (invoice.invType == "pw" || invoice.invType == "p")
                {
                    deliverydiv = deliverydiv.Replace("[[trDelivery]]", MainWindow.resourcemanagerreport.GetString("trShippingAmount"));

                    if (invoice.shippingCost > 0)
                    {
                        deliverydiv = deliverydiv.Replace("[[shippingcost]]", repm.DecTostring(invoice.shippingCost));
                        deliverydiv = deliverydiv.Replace("[[totaldeserved]]", repm.DecTostring(invoice.totalNet));
                        deliverydiv = deliverydiv.Replace("[[currencyh]]", AppSettings.Currency);
                        invbody = invbody.Replace("[[totalnet]]", repm.DecTostring(invoice.totalNet - invoice.shippingCost));
                        invbody = invbody.Replace("[[deliverydiv]]", deliverydiv);
                
                    }
                    else
                    {
                        invbody = invbody.Replace("[[deliverydiv]]", "");
                        invbody = invbody.Replace("[[totalnet]]", repm.DecTostring(invoice.totalNet));
                    }
                }
                else
                {
                    invbody = invbody.Replace("[[totalnet]]", repm.DecTostring(invoice.totalNet));
                    invbody = invbody.Replace("[[deliverydiv]]", "");

                }
                // end shippingcost

            }
            //  invoiceItems.trQuantity trQTR
            invitemtable = invitemtable.Replace("[[tritems]]", MainWindow.resourcemanagerreport.GetString("trItem").Trim());
            invitemtable = invitemtable.Replace("[[trunit]]", MainWindow.resourcemanagerreport.GetString("trUnit").Trim());
            invitemtable = invitemtable.Replace("[[trprice]]", MainWindow.resourcemanagerreport.GetString("trPrice").Trim());
            invitemtable = invitemtable.Replace("[[trquantity]]", MainWindow.resourcemanagerreport.GetString("trQTR").Trim());
            invitemtable = invitemtable.Replace("[[trtotalrow]]", MainWindow.resourcemanagerreport.GetString("trTotal").Trim());
            invbody = invbody.Replace("[[trinvoicecode]]", MainWindow.resourcemanagerreport.GetString("trInvoiceNumber").Trim() + ": ");
            invbody = invbody.Replace("[[trinvoicedate]]", MainWindow.resourcemanagerreport.GetString("trDate").Trim() + ": ");

            // invbody = invbody.Replace("[[trinvoicetotal]]", MainWindow.resourcemanagerreport.GetString("trSum").Trim() + ": ");

            invbody = invbody.Replace("[[trinvoicetotal]]", MainWindow.resourcemanagerreport.GetString("trSum").Trim());
            invbody = invbody.Replace("[[currency]]", AppSettings.Currency);
            //
            invbody = invbody.Replace("[[trinvoicediscount]]", MainWindow.resourcemanagerreport.GetString("trDiscount").Trim());

            invbody = invbody.Replace("[[trtotalnet]]", MainWindow.resourcemanagerreport.GetString("trTotal").Trim());
            // string invoicenote = "Thank you for your cooperation. We have also enclosed our procurement specifications and conditions for your review <br/> Sincerely";
            string invoicenote = setvlist.Where(x => x.notes == "text2").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "text2").FirstOrDefault().value.ToString();
            invbody = invbody.Replace("[[invoicenote]]", invoicenote);
            string link1 = setvlist.Where(x => x.notes == "link1text").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "link1text").FirstOrDefault().value.ToString();

            string link2 = setvlist.Where(x => x.notes == "link2text").FirstOrDefault() is null ? ""
                 : setvlist.Where(x => x.notes == "link2text").FirstOrDefault().value.ToString();
            string link3 = setvlist.Where(x => x.notes == "link3text").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "link3text").FirstOrDefault().value.ToString();
            invfooter = invfooter.Replace("[[support]]", link1);
            invfooter = invfooter.Replace("[[returnpolicy]]", link2);
            invfooter = invfooter.Replace("[[aboutus]]", link3);
            string link1url = setvlist.Where(x => x.notes == "link1url").FirstOrDefault() is null ? ""
                       : setvlist.Where(x => x.notes == "link1url").FirstOrDefault().value.ToString();
            string link2url = setvlist.Where(x => x.notes == "link2url").FirstOrDefault() is null ? ""
                       : setvlist.Where(x => x.notes == "link2url").FirstOrDefault().value.ToString();
            string link3url = setvlist.Where(x => x.notes == "link3url").FirstOrDefault() is null ? ""
                       : setvlist.Where(x => x.notes == "link3url").FirstOrDefault().value.ToString();
            invfooter = invfooter.Replace("[[supporturl]]", link1url);
            invfooter = invfooter.Replace("[[returnpolicyurl]]", link2url);
            invfooter = invfooter.Replace("[[aboutusurl]]", link3url);
            invfooter = invfooter.Replace("[[year]]", DateTime.Now.Year.ToString());
            //  invitemtable
            // foreach
            string datarows = "";
            foreach (ItemTransfer row in invoiceItems)
            {
                string rowhtml = invitemrow;
                row.price = decimal.Parse(SectionData.DecTostring(row.price));
                rowhtml = rowhtml.Replace("[[col1]]", row.itemName.Trim());
                rowhtml = rowhtml.Replace("[[col2]]", row.unitName.Trim());
                rowhtml = rowhtml.Replace("[[col3]]", row.price.ToString());
                rowhtml = rowhtml.Replace("[[col4]]", row.quantity.ToString());

                rowhtml = rowhtml.Replace("[[col5]]", (row.quantity * row.price).ToString());
                //     rowhtml = rowhtml.Replace("[[col4]]", (row.quantity * row.price).ToString());

                datarows += rowhtml;

            }
            invitemtable = invitemtable.Replace("[[invitemrow]]", datarows);
            // end foreach
            invbody = invbody.Replace("[[invitemtable]]", invitemtable);
            string mailbody = invheader + invbody + invfooter;
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailbody, null, "text/html");
            string testpath = repm.GetPath(@"EmailTemplates\mail.html");
            //
            if (!File.Exists(testpath))
            {
                // Create a file to write to.
                string createText = mailbody;
                File.WriteAllText(testpath, createText);
            }
            else
            {
                File.Delete(testpath);
                // Create a file to write to.
                string createText = mailbody;
                File.WriteAllText(testpath, createText);
            }
            img.path = repm.GetLogoImagePath();
            img.imageId = "logo";
            imgs.Add(img);
            img = new MailimageClass();
            img.path = repm.GetPath(@"EmailTemplates\images\image-2.gif");

            img.imageId = "image-2";
            imgs.Add(img);

            foreach (MailimageClass row in imgs)
            {
                htmlView.LinkedResources.Add(mailtosend.Linkimage(@row.path, row.imageId));
            }
            //
            mailtosend.htmlView = htmlView;
            return mailtosend;
        }


        public EmailClass fillRequirdTempData(string total, string emailto, SysEmails email, List<SetValues> setvlist)
        {// 
            string invheader = "";
            string invfooter = "";
            string invbody = "";
            EmailClass mailtosend = new EmailClass();
            mailtosend.from = email.email;
            mailtosend.smtpclient = email.smtpClient;
            mailtosend.port = (int)email.port;
            mailtosend.password = Encoding.UTF8.GetString(Convert.FromBase64String(email.password));
            mailtosend.isSSl = (bool)email.isSSL;
            mailtosend.AddTolist(emailto);
            mailtosend.subject = "Reqierment" + DateTime.Now.ToString();
            // data
            ReportCls repm = new ReportCls();
            List<MailimageClass> imgs = new List<MailimageClass>();
            MailimageClass img = new MailimageClass();
            string isArabic = ReportCls.checkInvLang();
            if (isArabic == "ar")
            {
                invheader = repm.ReadFile(@"EmailTemplates\ordertemplate\ar\invheader.tmp");
                invfooter = repm.ReadFile(@"EmailTemplates\ordertemplate\ar\invfooter.tmp");
                invbody = repm.ReadFile(@"EmailTemplates\reqtemplate\ar\invbody.tmp");
            }
            else
            { // en
                invheader = repm.ReadFile(@"EmailTemplates\ordertemplate\en\invheader.tmp");
                invfooter = repm.ReadFile(@"EmailTemplates\ordertemplate\en\invfooter.tmp");
                invbody = repm.ReadFile(@"EmailTemplates\reqtemplate\en\invbody.tmp");
            }
            //header info
            invheader = invheader.Replace("[[companyname]]", AppSettings.companyName.Trim());
            invheader = invheader.Replace("[[phone]]", AppSettings.Phone.Trim());
            invheader = invheader.Replace("[[Email]]", AppSettings.Email.Trim());
            invheader = invheader.Replace("[[fax]]", AppSettings.Fax.Trim());
            invheader = invheader.Replace("[[address]]", AppSettings.Address.Trim());
            invheader = invheader.Replace("[[trphone]]", MainWindow.resourcemanagerreport.GetString("trPhone").Trim() + ": ");
            invheader = invheader.Replace("[[trfax]]", MainWindow.resourcemanagerreport.GetString("trFax").Trim() + ": ");
            invheader = invheader.Replace("[[traddress]]", MainWindow.resourcemanagerreport.GetString("trAddress").Trim() + ": ");
            // string title = "Purchase Order";Required Amount: [[trreqamount]] [[reqamount]]
            //BODY
            if (isArabic == "ar")
            {
                invbody = invbody.Replace("[[trreqamount]]", " " + ":" + MainWindow.resourcemanagerreport.GetString("trRequired").Trim());

            }
            else
            {
                invbody = invbody.Replace("[[trreqamount]]", MainWindow.resourcemanagerreport.GetString("trRequired").Trim() + ": ");

            }
            invbody = invbody.Replace("[[reqamount]]", total.Trim() + " " + AppSettings.Currency);
            string title = setvlist.Where(x => x.notes == "title").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "title").FirstOrDefault().value.ToString();
            invheader = invheader.Replace("[[title]]", title.Trim());
            invbody = invbody.Replace("[[thankstitle]]", title);
            //   string thankstext = "Please provide to us,with a price list,along with your terms and conditions of sale, applicable discounts, shipping dates and additional sales and corporate policies. Should the information you provide be acceptable and competitive. ";
            string thankstext = setvlist.Where(x => x.notes == "text1").FirstOrDefault() is null ? ""
                  : setvlist.Where(x => x.notes == "text1").FirstOrDefault().value.ToString();
            invbody = invbody.Replace("[[thankstext]]", thankstext);
            // string invoicenote = "Thank you for your cooperation. We have also enclosed our procurement specifications and conditions for your review <br/> Sincerely";
            string invoicenote = setvlist.Where(x => x.notes == "text2").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "text2").FirstOrDefault().value.ToString();
            invbody = invbody.Replace("[[invoicenote]]", invoicenote);
            string link1 = setvlist.Where(x => x.notes == "link1text").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "link1text").FirstOrDefault().value.ToString();

            string link2 = setvlist.Where(x => x.notes == "link2text").FirstOrDefault() is null ? ""
                 : setvlist.Where(x => x.notes == "link2text").FirstOrDefault().value.ToString();
            string link3 = setvlist.Where(x => x.notes == "link3text").FirstOrDefault() is null ? ""
                : setvlist.Where(x => x.notes == "link3text").FirstOrDefault().value.ToString();

            invfooter = invfooter.Replace("[[support]]", link1);
            invfooter = invfooter.Replace("[[returnpolicy]]", link2);
            invfooter = invfooter.Replace("[[aboutus]]", link3);
            string link1url = setvlist.Where(x => x.notes == "link1url").FirstOrDefault() is null ? ""
                       : setvlist.Where(x => x.notes == "link1url").FirstOrDefault().value.ToString();
            string link2url = setvlist.Where(x => x.notes == "link2url").FirstOrDefault() is null ? ""
                       : setvlist.Where(x => x.notes == "link2url").FirstOrDefault().value.ToString();
            string link3url = setvlist.Where(x => x.notes == "link3url").FirstOrDefault() is null ? ""
                       : setvlist.Where(x => x.notes == "link3url").FirstOrDefault().value.ToString();

            invfooter = invfooter.Replace("[[supporturl]]", link1url);
            invfooter = invfooter.Replace("[[returnpolicyurl]]", link2url);
            invfooter = invfooter.Replace("[[aboutusurl]]", link3url);

            //invfooter = invfooter.Replace("[[year]]", DateTime.Now.Year.ToString());
            invfooter = invfooter.Replace("[[year]]", repm.DecTostring(DateTime.Now.Year));
            //  invitemtable
            // foreach

            // end foreach
            string mailbody = invheader + invbody + invfooter;
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailbody, null, "text/html");
            string testpath = repm.GetPath(@"EmailTemplates\mail.html");
            //
            if (!File.Exists(testpath))
            {
                // Create a file to write to.
                string createText = mailbody;
                File.WriteAllText(testpath, createText);
            }
            else
            {
                File.Delete(testpath);
                // Create a file to write to.
                string createText = mailbody;
                File.WriteAllText(testpath, createText);
            }
            img.path = repm.GetLogoImagePath();
            img.imageId = "logo";
            imgs.Add(img);
            img = new MailimageClass();
            img.path = repm.GetPath(@"EmailTemplates\images\req2.gif");

            img.imageId = "image-2";
            imgs.Add(img);
            img = new MailimageClass();
            img.path = repm.GetPath(@"EmailTemplates\images\req1.png");

            img.imageId = "image-3";
            imgs.Add(img);

            foreach (MailimageClass row in imgs)
            {
                htmlView.LinkedResources.Add(mailtosend.Linkimage(@row.path, row.imageId));
            }
            mailtosend.htmlView = htmlView;
            return mailtosend;
        }

    }
}





