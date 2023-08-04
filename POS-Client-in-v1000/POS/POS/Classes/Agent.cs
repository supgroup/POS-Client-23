using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace POS.Classes
{
    public class Agent
    {
        public int agentId { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string company { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string image { get; set; }
        public string type { get; set; }
        public string accType { get; set; }
        public float balance { get; set; }
        public Nullable<byte> balanceType { get; set; }
        public bool isLimited { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string notes { get; set; }
        public int isActive { get; set; }
        public string fax { get; set; }
        public string payType { get; set; }
        public decimal maxDeserve { get; set; }
        public Boolean canDelete { get; set; }

        public async Task<List<Agent>> Get(string type)
        {
            List<Agent> items = new List<Agent>();

            //  to pass parameters (optional)
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string myContent = type;
            parameters.Add("type", myContent);
            // 
            IEnumerable<Claim> claims = await APIResult.getList("Agent/Get", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Agent>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<Agent>> GetAll()
        {
            List<Agent> items = new List<Agent>();
 
            IEnumerable<Claim> claims = await APIResult.getList("Agent/GetAll");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Agent>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<Agent>> GetAgentsActive(string type)
        {
            List<Agent> items = new List<Agent>();

            //  to pass parameters (optional)
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string myContent = type;
            parameters.Add("type", myContent);
            // 
            IEnumerable<Claim> claims = await APIResult.getList("Agent/GetActive", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Agent>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        public async Task<List<Agent>> GetActiveForAccount(string type , string payType)
        {
            List<Agent> items = new List<Agent>();

            //  to pass parameters (optional)
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type"   , type.ToString());
            parameters.Add("payType", payType.ToString());


            IEnumerable<Claim> claims = await APIResult.getList("Agent/GetActiveForAccount", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Agent>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        public async Task<Agent> getAgentById(int agentId)
        {
            Agent agent = new Agent();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("agentId", agentId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Agent/GetAgentByID", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    agent = JsonConvert.DeserializeObject<Agent>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return agent;
        }
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        /// 

        public async Task<decimal> save(Agent agent)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Agent/Save";

            var myContent = JsonConvert.SerializeObject(agent);
            parameters.Add("itemObject", myContent);
           return await APIResult.post(method, parameters);
        }
    
        /// ///////////////////////////////////////
        /// before
        /// //////////////////////////////////////

        public string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }
        // delete agent
        public async Task<decimal> delete(int agentId, int userId, Boolean final)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", agentId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());

            string method = "Agent/Delete";
           return await APIResult.post(method, parameters);
        }

         
        public async Task<string> uploadImage(string imagePath, string imageName, int agentId)
        {
            if (imagePath != "")
            {
                //string imageName = agentId.ToString();
                MultipartFormDataContent form = new MultipartFormDataContent();
                // get file extension
                var ext = imagePath.Substring(imagePath.LastIndexOf('.'));
                var extension = ext.ToLower();
                string fileName = imageName + extension;
                try
                {
                    // configure trmporery path
                    string dir = Directory.GetCurrentDirectory();
                    string tmpPath = Path.Combine(dir, Global.TMPAgentsFolder);
                    tmpPath = Path.Combine(tmpPath, imageName + extension);
                    if (System.IO.File.Exists(tmpPath))
                    {
                        System.IO.File.Delete(tmpPath);
                    }
                    if (imagePath != tmpPath) // edit mode
                    {
                        // resize image
                        ImageProcess imageP = new ImageProcess(150, imagePath);
                        imageP.ScaleImage(tmpPath);

                        // read image file
                        var stream = new FileStream(tmpPath, FileMode.Open, FileAccess.Read);

                        // create http client request
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(Global.APIUri);
                            client.Timeout = System.TimeSpan.FromSeconds(3600);
                            string boundary = string.Format("----WebKitFormBoundary{0}", DateTime.Now.Ticks.ToString("x"));
                            HttpContent content = new StreamContent(stream);
                            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                            content.Headers.Add("client", "true");

                            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = imageName,
                                FileName = fileName
                            };
                            form.Add(content, "fileToUpload");

                            var response = await client.PostAsync(@"agent/PostUserImage", form);
                            //if (response.IsSuccessStatusCode)
                            //{
                            //    // save image name in DB
                            //    Agent agent = new Agent();
                            //    agent.agentId = agentId;
                            //    agent.image = fileName;
                            //    await updateImage(agent);
                            //    //await save();
                            //    return fileName;
                            //}
                        }
                        stream.Dispose();
                    }
                    // save image name in DB
                    Agent agent = new Agent();
                    agent.agentId = agentId;
                    agent.image = fileName;
                    await updateImage(agent);

                    return fileName;
                    //delete tmp image
                    //if (System.IO.File.Exists(tmpPath))
                    //{
                    //    System.IO.File.Delete(tmpPath);
                    //}
                }
                catch
                { return ""; }
            }
            return "";
        }
        // update image field in DB
        public async Task<decimal> updateImage(Agent agent)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var myContent = JsonConvert.SerializeObject(agent);
            parameters.Add("itemObject", myContent);

            string method = "Agent/UpdateImage";
           return await APIResult.post(method, parameters);
        }

        //public async Task<Image> downloadImage(string imageName)
        //public async Task<Stream> downloadImage(string imageName)
        //public async Task<byte[]> downloadImage(string imageName)

        //{
        //    Stream jsonString = null;
        //    byte[] byteImg = null;
        //    Image img = null;
        //    // ... Use HttpClient.
        //    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        //    using (var client = new HttpClient())
        //    {
        //        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //        client.BaseAddress = new Uri(Global.APIUri);
        //        client.DefaultRequestHeaders.Clear();
        //        client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
        //        client.DefaultRequestHeaders.Add("Keep-Alive", "3600");
        //        HttpRequestMessage request = new HttpRequestMessage();
        //        request.RequestUri = new Uri(Global.APIUri + "Agent/GetImage?imageName=" + imageName);
        //        //request.Headers.Add("APIKey", Global.APIKey);
        //        request.Headers.Add("type", type);
        //        request.Method = HttpMethod.Get;
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        HttpResponseMessage response = await client.SendAsync(request);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            jsonString = await response.Content.ReadAsStreamAsync();
        //            img = Bitmap.FromStream(jsonString);
        //            byteImg = await response.Content.ReadAsByteArrayAsync();

        //            // configure trmporery path
        //            //string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        //            string dir = Directory.GetCurrentDirectory();
        //            string tmpPath = Path.Combine(dir, Global.TMPAgentsFolder);
        //            if (!Directory.Exists(tmpPath))
        //                Directory.CreateDirectory(tmpPath);
        //            tmpPath = Path.Combine(tmpPath, imageName);
        //            if (System.IO.File.Exists(tmpPath))
        //            {
        //                System.IO.File.Delete(tmpPath);
        //            }
        //            using (FileStream fs = new FileStream(tmpPath, FileMode.Create, FileAccess.ReadWrite))
        //            {
        //                fs.Write(byteImg, 0, byteImg.Length);
        //            }
        //        }
        //        return byteImg;
        //    }
        //}

        public async Task<byte[]> downloadImage(string imageName)
        {
            byte[] byteImg = null;
            if (imageName != "")
            {
                byteImg = await APIResult.getImage("Agent/GetImage", imageName);

                string dir = Directory.GetCurrentDirectory();
                string tmpPath = Path.Combine(dir, Global.TMPAgentsFolder);
                if (!Directory.Exists(tmpPath))
                    Directory.CreateDirectory(tmpPath);
                tmpPath = Path.Combine(tmpPath, imageName);
                if (System.IO.File.Exists(tmpPath))
                {
                    System.IO.File.Delete(tmpPath);
                }
                if (byteImg != null)
                {
                    using (FileStream fs = new FileStream(tmpPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        fs.Write(byteImg, 0, byteImg.Length);
                    }
                }

            }
            return byteImg;
        }
        public async Task<decimal> updateBalance(int agentId, decimal balance)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("agentId", agentId.ToString());
            parameters.Add("balance", balance.ToString());

            string method = "Agent/UpdateBalance";
           return await APIResult.post(method, parameters);
        }

         

        public async Task<string> generateCodeNumber(string type)
        {
            int sequence = (int)await GetLastNumOfCode(type);
            sequence++;
            string strSeq = sequence.ToString();
            if (sequence <= 999999)
                strSeq = sequence.ToString().PadLeft(6, '0');
            string transNum = type + "-" + strSeq;
            return transNum;
        }
        public async Task<decimal> GetLastNumOfCode(string type)
        {
            int value = 0 ;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type);
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Agent/GetLastNumOfCode", parameters);


            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    
                    value =int.Parse(JsonConvert.DeserializeObject<String>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                    break;
                }
            }
            return value;
        }
         

    }

}

