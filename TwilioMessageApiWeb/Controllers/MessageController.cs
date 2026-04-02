using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using Twilio;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Studio.V1;
using TwilioMessageApiWeb.Models;

namespace TwilioMessageApiWeb.Controllers
{
    public class MessageController : Controller
    {
        static SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TwilioConnection"].ToString());
        public string OneDriveApiRoot { get; set; } = "https:/demo.com/v1.0/";
        string accountSid = System.Configuration.ConfigurationManager.AppSettings["SID"].ToString();
        string authToken = System.Configuration.ConfigurationManager.AppSettings["Token"].ToString();
        // GET: Message
        public void GetMessage()
        {
            TwilioClient.Init(accountSid, authToken);
            var messages = MessageResource.Read();
            DataTable tblmessage = new DataTable();
            tblmessage.Columns.Add("body");
            tblmessage.Columns.Add("num_segments");
            tblmessage.Columns.Add("direction");
            tblmessage.Columns.Add("from");
            tblmessage.Columns.Add("to");
            tblmessage.Columns.Add("date_updated");
            tblmessage.Columns.Add("price");
            tblmessage.Columns.Add("error_message");
            tblmessage.Columns.Add("uri");
            tblmessage.Columns.Add("account_sid");
            tblmessage.Columns.Add("num_media");
            tblmessage.Columns.Add("status");
            tblmessage.Columns.Add("messaging_service_sid");
            tblmessage.Columns.Add("sid");
            tblmessage.Columns.Add("date_sent");
            tblmessage.Columns.Add("date_created");
            tblmessage.Columns.Add("error_code");
            tblmessage.Columns.Add("price_unit");
            tblmessage.Columns.Add("api_version");
            tblmessage.Columns.Add("record_id");
            DataRow row;
            int i = 0;
            foreach (var message in messages)
            {
                row = tblmessage.NewRow();
                row["body"] = message.Body;
                row["num_segments"] = message.NumSegments;
                row["direction"] = message.Direction;
                row["from"] = message.From;
                row["to"] = message.To;
                row["date_updated"] = message.DateUpdated != null ? message.DateUpdated.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                row["price"] = message.Price;
                row["error_message"] = message.ErrorMessage;
                row["uri"] = message.Uri;
                row["account_sid"] = message.AccountSid;
                row["num_media"] = message.NumMedia;
                row["status"] = message.Status;
                row["messaging_service_sid"] = message.MessagingServiceSid;
                row["sid"] = message.Sid;
                row["date_sent"] = message.DateSent != null ? message.DateSent.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                row["date_created"] = message.DateCreated != null ? message.DateCreated.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                row["error_code"] = message.ErrorCode;
                row["price_unit"] = message.PriceUnit;
                row["api_version"] = message.ApiVersion;
                row["record_id"] = i;
                tblmessage.Rows.Add(row);
                i++;
            }

            using (SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TwilioConnection"].ToString()))
            {
                dbConnection.Open();
                using (SqlBulkCopy s = new SqlBulkCopy(ConfigurationManager.ConnectionStrings["TwilioConnection"].ToString(), SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.UseInternalTransaction))
                {
                    s.DestinationTableName = "[dbo].[TwillioMessages]";
                    s.BulkCopyTimeout = 2000;
                    foreach (var column in tblmessage.Columns)
                        s.ColumnMappings.Add(column.ToString(), column.ToString());

                    s.WriteToServer(tblmessage);
                }
            }
            
        }
        public static void DatatableToCSV(DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers  
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        public ActionResult Downloadfile()
        {
            return View();
        }

        public JsonResult Login(string U, string P)
        {
            ResponseDetail responseDetail = new ResponseDetail();
            string curFile = string.Format(System.Configuration.ConfigurationManager.AppSettings["basepath"].ToString() + "Message/Messages.csv");
            string Username = System.Configuration.ConfigurationManager.AppSettings["Username"].ToString();
            string Password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();
            if (U == Username && P == Password)
            {
                responseDetail.Success = true;
                responseDetail.Data = curFile;
            }
            return Json(responseDetail, JsonRequestBehavior.AllowGet);
        }

        public class ResponseDetail
        {
            public string Message { get; set; }

            public bool Success { get; set; }

            public object Data { get; set; }
        }

        public ActionResult GetMessageLastDay()
        {
            try
            {
                UpdateQueuedJobRunDaily();
                TwilioClient.Init(accountSid, authToken);
                DateTime Today = DateTime.Now;
                DateTime Yesterday = Today.AddDays(-1);

                //DateTime Yesterday = new DateTime(2021, 10, 15);//DateTime.Now.AddMonths(-5).AddDays(-3);
                //DateTime Today = new DateTime(2021, 10, 16);//DateTime.Now.AddMonths(-4).AddDays(-3);

                var messages = MessageResource.Read(dateSentAfter: new DateTime(Yesterday.Year, Yesterday.Month, Yesterday.Day, 0, 0, 0), dateSentBefore: new DateTime(Today.Year, Today.Month, Today.Day, 0, 0, 0));
                //Get today messages 
                //var messages = MessageResource.Read(dateSent : new DateTime(Today.Year, Today.Month, Today.Day, 0, 0, 0));
                DataTable tblmessage = new DataTable();
                tblmessage.Columns.Add("body");
                tblmessage.Columns.Add("num_segments");
                tblmessage.Columns.Add("direction");
                tblmessage.Columns.Add("from");
                tblmessage.Columns.Add("to");
                tblmessage.Columns.Add("date_updated");
                tblmessage.Columns.Add("price");
                tblmessage.Columns.Add("error_message");
                tblmessage.Columns.Add("uri");
                tblmessage.Columns.Add("account_sid");
                tblmessage.Columns.Add("num_media");
                tblmessage.Columns.Add("status");
                tblmessage.Columns.Add("messaging_service_sid");
                tblmessage.Columns.Add("sid");
                tblmessage.Columns.Add("date_sent");
                tblmessage.Columns.Add("date_created");
                tblmessage.Columns.Add("error_code");
                tblmessage.Columns.Add("price_unit");
                tblmessage.Columns.Add("api_version");
                tblmessage.Columns.Add("record_id");
                tblmessage.Columns.Add("FlowID");
                tblmessage.Columns.Add("FlowName");
                tblmessage.Columns.Add("WidgetName");
                DataRow row;
                int i = 0;
                foreach (var message in messages)
                {
                    row = tblmessage.NewRow();
                    row["body"] = message.Body;
                    row["num_segments"] = message.NumSegments;
                    row["direction"] = message.Direction;
                    row["from"] = message.From;
                    row["to"] = message.To;
                    row["date_updated"] = message.DateUpdated != null ? message.DateUpdated.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                    row["price"] = message.Price;
                    row["error_message"] = message.ErrorMessage;
                    row["uri"] = message.Uri;
                    row["account_sid"] = message.AccountSid;
                    row["num_media"] = message.NumMedia;
                    row["status"] = message.Status;
                    row["messaging_service_sid"] = message.MessagingServiceSid;
                    row["sid"] = message.Sid;
                    row["date_sent"] = message.DateSent != null ? message.DateSent.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                    row["date_created"] = message.DateCreated != null ? message.DateCreated.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                    row["error_code"] = message.ErrorCode;
                    row["price_unit"] = message.PriceUnit;
                    row["api_version"] = message.ApiVersion;
                    row["record_id"] = i;
                    row["FlowID"] = "";
                    row["FlowName"] = "";
                    row["WidgetName"] = "";
                    tblmessage.Rows.Add(row);
                    i++;
                }
                //save in file 
                string curFile = System.Web.Hosting.HostingEnvironment.MapPath("~/Message/LastDayMessages.csv");
                if (System.IO.File.Exists(curFile))
                {
                    System.IO.File.Delete(curFile);
                }
                DatatableToCSV(tblmessage, curFile);
                LogError("save messages start");
               
                DataSet ds = new DataSet();
                using (var cmd = new SqlCommand("Update_TwillioMessages", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tblMessages", tblmessage).SqlDbType = SqlDbType.Structured;
                    cmd.CommandTimeout = Int32.MaxValue;
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
                //if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null)
                //{
                //    //save in file 
                //    string curFile = System.Web.Hosting.HostingEnvironment.MapPath("~/Message/LastDayMessages.csv");
                //    if (System.IO.File.Exists(curFile))
                //    {
                //        System.IO.File.Delete(curFile);
                //    }
                //    DatatableToCSV(ds.Tables[0], curFile);
                //}
                LogError("save messages end");
                LogError("delete old messages start");
                using (var cmd = new SqlCommand("DeleteBeforesevenDaysRecords", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = Int32.MaxValue;
                    try
                    {
                        DataSet ds1 = new DataSet();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds1);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
                LogError("delete old messages end");
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            return View();
        }
        public void UpdateQueuedJobRunDaily()
        {
            using (var cmd = new SqlCommand("UpdateQueuedJobRunDaily", sqlConnection))
            {
                sqlConnection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = Int32.MaxValue;
                try
                {
                    DataSet ds = new DataSet();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.Message);
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
        }
        public JsonResult GetFileForDownload(string FN)
        {
            ResponseDetail responseDetail = new ResponseDetail();
            string curFile = null;
            if (FN.ToUpper() == "MESSAGEFILE")
            {
                curFile = string.Format(System.Configuration.ConfigurationManager.AppSettings["basepath"].ToString() + "Message/Messages.csv");
            }
            else if (FN.ToUpper() == "LASTDAYMESSAGEFILE")
            {
                curFile = string.Format(System.Configuration.ConfigurationManager.AppSettings["basepath"].ToString() + "Message/LastDayMessages.csv");
            }
            responseDetail.Success = true;
            responseDetail.Data = curFile;
            return Json(responseDetail, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetFilesForDownload(string FN)
        {
            ResponseDetail responseDetail = new ResponseDetail();
            string curFile = null;
            if (FN.ToUpper() == "7DAYSMESSAGEFILE")
            {
                using (var cmd = new SqlCommand("GetMessages", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.Parameters.AddWithValue("@TypeToGet", FN).SqlDbType = SqlDbType.NVarChar;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = Int32.MaxValue;
                    try
                    {
                        DataSet ds = new DataSet();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null)
                        {
                            string file = FN.ToUpper() == "7DAYSMESSAGEFILE" ? System.Web.Hosting.HostingEnvironment.MapPath("~/Message/Messages.csv") : System.Web.Hosting.HostingEnvironment.MapPath("~/Message/LastDayMessages.csv");
                            if (System.IO.File.Exists(file))
                            {
                                System.IO.File.Delete(file);
                            }
                            DatatableToCSV(ds.Tables[0], file);
                            responseDetail.Success = true;
                            responseDetail.Data = file;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
                curFile = string.Format(System.Configuration.ConfigurationManager.AppSettings["basepath"].ToString() + "Message/Messages.csv");
            }
            else if (FN.ToUpper() == "24HRMESSAGEFILE")
            {
                curFile = string.Format(System.Configuration.ConfigurationManager.AppSettings["basepath"].ToString() + "Message/LastDayMessages.csv");
            }
            responseDetail.Success = true;
            responseDetail.Data = curFile;
            return Json(responseDetail, JsonRequestBehavior.AllowGet);
        }
        public void Get24hoursMessage()
        {
            try
            {
                TwilioClient.Init(accountSid, authToken);
                DateTime Today = DateTime.Now;
                DateTime Yesterday = GetLastRecordDateFromDB();

                
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public DateTime GetLastRecordDateFromDB()
        {
            DateTime lastrecorddate = DateTime.Now;

            using (var cmd = new SqlCommand("GetLastRecordDateFromDB", sqlConnection))
            {
                sqlConnection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = Int32.MaxValue;
                try
                {
                    DataSet ds = new DataSet();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null)
                    {
                        lastrecorddate = Convert.ToDateTime(ds.Tables[0].Rows[0]["date_created"]);
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            return lastrecorddate;
        }
        private void LogError(string MessageToAppend)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", MessageToAppend);
            //message += Environment.NewLine;
            //message += string.Format("StackTrace: {0}", ex.StackTrace);
            //message += Environment.NewLine;
            //message += string.Format("Source: {0}", ex.Source);
            //message += Environment.NewLine;
            //message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            //message += Environment.NewLine;
            //message += "-----------------------------------------------------------";
            //message += Environment.NewLine;
            string path = Server.MapPath("~/exception.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
        public static bool SendEmail(string toemail, string subject, string body, string[] Cc = null)
        {
            try
            {
                System.Net.Mail.MailMessage NetMail = new System.Net.Mail.MailMessage();
                SmtpClient MailClient = new SmtpClient();
                using (MailClient = new SmtpClient())
                {
                    NetworkCredential TheseCredentials = new NetworkCredential(Convert.ToString(ConfigurationManager.AppSettings["ForMailGmailID"]), Convert.ToString(ConfigurationManager.AppSettings["FormMailGmailPassword"]));
                    NetMail.To.Add(toemail);
                    NetMail.From = new MailAddress(Convert.ToString(ConfigurationManager.AppSettings["ForMailGmailID"]));
                    NetMail.IsBodyHtml = true;
                    NetMail.Priority = System.Net.Mail.MailPriority.High;
                    NetMail.Subject = subject;
                    NetMail.Body = body;
                    MailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    MailClient.EnableSsl = true;
                    MailClient.Host = Convert.ToString(ConfigurationManager.AppSettings["MailHostAddress"]);
                    MailClient.Port = Convert.ToInt16(ConfigurationManager.AppSettings["MailPort"]);
                    MailClient.UseDefaultCredentials = false;
                    MailClient.Credentials = TheseCredentials;
                    MailClient.Send(NetMail);
                    NetMail.Dispose();
                }
                return true;
            }
            catch (SmtpException ex)
            {
                return false;
            }
        }
        public static bool SendBulkEmail(List<EmailIds> EmailList, string Subject, string body)
        {
            bool result = false;

            if (EmailList.Count > 0)
            {
                try
                {
                    Parallel.ForEach(EmailList, row =>
                    {
                        if (!string.IsNullOrEmpty(row.EmailId.Trim()))
                        {
                            MailMessage NetMail = new MailMessage();
                            NetworkCredential TheseCredentials = new NetworkCredential(Convert.ToString(ConfigurationManager.AppSettings["ForMailGmailID"]), Convert.ToString(ConfigurationManager.AppSettings["FormMailGmailPassword"]));
                            NetMail.To.Add(row.EmailId.Trim());
                            NetMail.From = new MailAddress(Convert.ToString(ConfigurationManager.AppSettings["ForMailGmailID"]));
                            NetMail.IsBodyHtml = true;
                            NetMail.Priority = MailPriority.High;
                            NetMail.Subject = Subject;
                            NetMail.Body = body;
                            SmtpClient MailClient = new SmtpClient(Convert.ToString(ConfigurationManager.AppSettings["MailHostAddress"]), 587);
                            MailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                            MailClient.EnableSsl = true;
                            MailClient.Port = Convert.ToInt16(ConfigurationManager.AppSettings["MailPort"]);
                            MailClient.UseDefaultCredentials = false;
                            MailClient.Credentials = TheseCredentials;
                            MailClient.Send(NetMail);
                            NetMail.Dispose();
                        }
                    });
                }
                catch (Exception ex)
                {
                    result = false;
                }

            }
            return result;
        }
        public ActionResult GetQueuedMessages()
        {
            try
            {
                bool IsEmailSent = false;
                using (var cmd = new SqlCommand("GetIsEmailSentValue", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = Int32.MaxValue;
                    try
                    {
                        DataSet ds = new DataSet();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            IsEmailSent = Convert.ToBoolean(ds.Tables[0].Rows[0]["IsEmailSent"]);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
                if (!IsEmailSent)
                {
                    TwilioClient.Init(accountSid, authToken);
                    DateTime Today = DateTime.Now;
                    DateTime Yesterday = Today.AddDays(-1);

                    var messages = MessageResource.Read(dateSentAfter: new DateTime(Yesterday.Year, Yesterday.Month, Yesterday.Day, Yesterday.Hour, Yesterday.Minute, Yesterday.Second), dateSentBefore: new DateTime(Today.Year, Today.Month, Today.Day, Today.Hour, Today.Minute, Today.Second));
                    string emailhtmlstr = "";

                    foreach (var message in messages)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(message.Status)) && Convert.ToString(message.Status) == "queued" && message.DateSent != null)
                        {
                            DateTime datesent = Convert.ToDateTime(message.DateSent.Value);
                            TimeSpan span = DateTime.Now.Subtract(datesent);
                            if (span.TotalMinutes > Convert.ToDouble(30))
                            {
                                emailhtmlstr += "<div><p></p>To : " + message.To + "<br/> From : " + message.From + " <br/> Account sid : " + message.AccountSid + "<br/>Sid : " + message.Sid + "<br/> Status : " + message.Status + "<br/> Date Sent : " + message.DateSent.Value.ToString("MM/dd/yyyy HH:mm:ss") + "<br/> Time in Que : " + span.ToString(@"mm\:ss") + "</div><br/>";
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(emailhtmlstr))
                    {
                        List<EmailIds> emailIdList = new List<EmailIds>();
                        EmailIds emailIds = new EmailIds();
                        emailIds.EmailId = "robert@kaptea.io";
                        emailIdList.Add(emailIds);
                        EmailIds emailId = new EmailIds();
                        emailId.EmailId = "sean.cody@kaptea.io";
                        emailIdList.Add(emailId);
                        //bool success = SendEmail("robert@kaptea.io", "Twilio Queued Messages Status Alert", emailhtmlstr);
                        bool success = SendBulkEmail(emailIdList, "Twilio Queued Messages Status Alert", emailhtmlstr);
                        if (success)
                        {
                            using (var cmd = new SqlCommand("UpdateIsEmailSentValue", sqlConnection))
                            {
                                sqlConnection.Open();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandTimeout = Int32.MaxValue;
                                try
                                {
                                    DataSet ds = new DataSet();
                                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                    {
                                        da.Fill(ds);
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                finally
                                {
                                    sqlConnection.Close();
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View();
        }

        public void GetFlowDetail()
        {
            TwilioClient.Init(accountSid, authToken);
            var Flows = FlowResource.Read();
            DataTable tblmessage = new DataTable();
            tblmessage.Columns.Add("AccountSid");
            tblmessage.Columns.Add("DateCreated");
            tblmessage.Columns.Add("DateUpdated");
            tblmessage.Columns.Add("FriendlyName");
            tblmessage.Columns.Add("Sid");
            tblmessage.Columns.Add("Status");
            tblmessage.Columns.Add("Url");
            tblmessage.Columns.Add("Version");
            tblmessage.Columns.Add("record_id");
            DataRow row;
            int i = 0;
            foreach (var message in Flows)
            {
                row = tblmessage.NewRow();
                row["AccountSid"] = message.AccountSid;
                row["DateCreated"] = message.DateCreated != null ? message.DateCreated.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                row["DateUpdated"] = message.DateUpdated != null ? message.DateUpdated.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                row["FriendlyName"] = message.FriendlyName;
                row["Sid"] = message.Sid;
                row["Status"] = message.Status;
                row["Url"] = message.Url;
                row["Version"] = message.Version;
                row["record_id"] = i;
                tblmessage.Rows.Add(row);
                i++;
            }
            //save in file 
            string curFile = System.Web.Hosting.HostingEnvironment.MapPath("~/Message/FlowLogs.csv");
            if (System.IO.File.Exists(curFile))
            {
                System.IO.File.Delete(curFile);
            }
            DatatableToCSV(tblmessage, curFile);
            using (SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TwilioConnection"].ToString()))
            {
                dbConnection.Open();
                using (SqlBulkCopy s = new SqlBulkCopy(ConfigurationManager.ConnectionStrings["TwilioConnection"].ToString(), SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.UseInternalTransaction))
                {
                    s.DestinationTableName = "[dbo].[FlowDetail]";
                    s.BulkCopyTimeout = 2000;
                    foreach (var column in tblmessage.Columns)
                        s.ColumnMappings.Add(column.ToString(), column.ToString());

                    s.WriteToServer(tblmessage);
                }
            }
        }
        [HttpPost]
        public JsonResult updateflowdetails()
        {
            string result = string.Empty;
            List<FlowDetail> jsonRequest = new List<FlowDetail>();
            System.IO.Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new System.IO.StreamReader(req).ReadToEnd();
            try
            {
                Insertlastmessages();
                jsonRequest = JsonConvert.DeserializeObject<List<FlowDetail>>(json);
                DataTable Dt = DatatableConvert.ToDataTable<FlowDetail>(jsonRequest);
                using (var cmd = new SqlCommand("Update_FlowdetailsByMessageID", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tblFlowDetails", Dt).SqlDbType = SqlDbType.Structured;
                    cmd.CommandTimeout = Int32.MaxValue;
                    try
                    {
                        DataSet ds = new DataSet();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                        result = "success";
                    }
                    catch (Exception ex)
                    {
                        result = "failed";
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                result = "failed";
            }
            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public void Insertlastmessages()
        {
            try
            {
                DateTime LastMessageDate = DateTime.Now;
                using (var cmd = new SqlCommand("GetLastMessageDate", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = Int32.MaxValue;
                    try
                    {
                        DataSet ds = new DataSet();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            LastMessageDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["LastMessageDate"]);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
                TwilioClient.Init(accountSid, authToken);
                DateTime currentdatetime = DateTime.Now;
                var messages = MessageResource.Read(dateSentAfter: new DateTime(LastMessageDate.Year, LastMessageDate.Month, LastMessageDate.Day, LastMessageDate.Hour, LastMessageDate.Minute, LastMessageDate.Second), dateSentBefore: new DateTime(currentdatetime.Year, currentdatetime.Month, currentdatetime.Day, currentdatetime.Hour, currentdatetime.Minute, currentdatetime.Second));
                DataTable tblmessage = new DataTable();
                tblmessage.Columns.Add("body");
                tblmessage.Columns.Add("num_segments");
                tblmessage.Columns.Add("direction");
                tblmessage.Columns.Add("from");
                tblmessage.Columns.Add("to");
                tblmessage.Columns.Add("date_updated");
                tblmessage.Columns.Add("price");
                tblmessage.Columns.Add("error_message");
                tblmessage.Columns.Add("uri");
                tblmessage.Columns.Add("account_sid");
                tblmessage.Columns.Add("num_media");
                tblmessage.Columns.Add("status");
                tblmessage.Columns.Add("messaging_service_sid");
                tblmessage.Columns.Add("sid");
                tblmessage.Columns.Add("date_sent");
                tblmessage.Columns.Add("date_created");
                tblmessage.Columns.Add("error_code");
                tblmessage.Columns.Add("price_unit");
                tblmessage.Columns.Add("api_version");
                tblmessage.Columns.Add("record_id");
                tblmessage.Columns.Add("FlowID");
                tblmessage.Columns.Add("FlowName");
                tblmessage.Columns.Add("WidgetName");
                DataRow row;
                int i = 0;
                foreach (var message in messages)
                {
                    row = tblmessage.NewRow();
                    row["body"] = message.Body;
                    row["num_segments"] = message.NumSegments;
                    row["direction"] = message.Direction;
                    row["from"] = message.From;
                    row["to"] = message.To;
                    row["date_updated"] = message.DateUpdated != null ? message.DateUpdated.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                    row["price"] = message.Price;
                    row["error_message"] = message.ErrorMessage;
                    row["uri"] = message.Uri;
                    row["account_sid"] = message.AccountSid;
                    row["num_media"] = message.NumMedia;
                    row["status"] = message.Status;
                    row["messaging_service_sid"] = message.MessagingServiceSid;
                    row["sid"] = message.Sid;
                    row["date_sent"] = message.DateSent != null ? message.DateSent.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                    row["date_created"] = message.DateCreated != null ? message.DateCreated.Value.ToString("MM/dd/yyyy HH:mm:ss") : "";
                    row["error_code"] = message.ErrorCode;
                    row["price_unit"] = message.PriceUnit;
                    row["api_version"] = message.ApiVersion;
                    row["record_id"] = i;
                    row["FlowID"] = "";
                    row["FlowName"] = "";
                    row["WidgetName"] = "";
                    tblmessage.Rows.Add(row);
                    i++;
                }
                using (var cmd = new SqlCommand("Update_TwillioMessages", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tblMessages", tblmessage).SqlDbType = SqlDbType.Structured;
                    cmd.CommandTimeout = Int32.MaxValue;
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        DataSet ds = new DataSet();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public JsonResult InsertFlowDetail()
        {
            string result = string.Empty;
            List<FlowDetail> jsonRequest = new List<FlowDetail>();
            System.IO.Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new System.IO.StreamReader(req).ReadToEnd();
            try
            {
                jsonRequest = JsonConvert.DeserializeObject<List<FlowDetail>>(json);
                DataTable Dt = DatatableConvert.ToDataTable<FlowDetail>(jsonRequest);
                using (var cmd = new SqlCommand("Insert_Flowdetails", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tblFlowDetails", Dt).SqlDbType = SqlDbType.Structured;
                    cmd.CommandTimeout = Int32.MaxValue;
                    try
                    {
                        DataSet ds = new DataSet();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                        result = "success";
                    }
                    catch (Exception ex)
                    {
                        result = "failed";
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                result = "failed";
            }
            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpPost]
        public JsonResult GetDateWiseFile(string date)
        {
            ResponseDetail responseDetail = new ResponseDetail();
            try
            {
                DateTime Date = Convert.ToDateTime(date);
                LogError("selected date : " + date);
                DataSet ds = new DataSet();
                using (var cmd = new SqlCommand("GetMessages_ByDate", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Date", Date).SqlDbType = SqlDbType.DateTime;
                    cmd.CommandTimeout = Int32.MaxValue;
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("ex: sp :" + ex.Message);
                        responseDetail.Success = false;
                        responseDetail.Message = ex.Message;
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null)
                {
                    LogError("save into file");
                    //save in file 
                    string curFile = System.Web.Hosting.HostingEnvironment.MapPath("~/Message/DateWiseMessages.csv");
                    if (System.IO.File.Exists(curFile))
                    {
                        System.IO.File.Delete(curFile);
                    }
                    LogError("DatatableToCSV called");
                    DatatableToCSV(ds.Tables[0], curFile);
                    LogError("set  responseDetail.Data");
                    responseDetail.Data = string.Format(System.Configuration.ConfigurationManager.AppSettings["basepath"].ToString() + "Message/DateWiseMessages.csv"); ;
                }
                responseDetail.Success = true;
            }
            catch (Exception ex)
            {
                LogError("outer ex :" + ex.Message);
                responseDetail.Success = false;
                responseDetail.Message = ex.Message;
            }
            return Json(responseDetail, JsonRequestBehavior.AllowGet);
        }
    }
}