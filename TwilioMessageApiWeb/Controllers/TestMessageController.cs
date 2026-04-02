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
    public class TestMessageController : Controller
    {
        static SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TwilioConnection"].ToString());
        string accountSid = System.Configuration.ConfigurationManager.AppSettings["TestSID"].ToString();
        string authToken = System.Configuration.ConfigurationManager.AppSettings["TestToken"].ToString();
        public ActionResult DownloadTestfile()
        {
            return View();
        }

        public JsonResult Login(string U, string P)
        {
            MessageController.ResponseDetail responseDetail = new MessageController.ResponseDetail();
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
        [HttpPost]
        public JsonResult GetFilesForDownload(string FN)
        {
            MessageController.ResponseDetail responseDetail = new MessageController.ResponseDetail();
            string curFile = null;
            string filepathforupload = System.Web.Hosting.HostingEnvironment.MapPath("~/Message/Test_LastDayMessages.csv");
            if (FN.ToUpper() == "24HRMESSAGEFILE")
            {
                curFile = string.Format(System.Configuration.ConfigurationManager.AppSettings["basepath"].ToString() + "Message/Test_LastDayMessages.csv");
            }
            using (var cmd = new SqlCommand("Test_GetMessages", sqlConnection))
            {
                sqlConnection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TypeToGet", FN).SqlDbType = SqlDbType.NVarChar;
                cmd.CommandTimeout = Int32.MaxValue;
                try
                {
                    DataSet ds = new DataSet();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                    if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        //save in file 
                        if (System.IO.File.Exists(filepathforupload))
                        {
                            System.IO.File.Delete(filepathforupload);
                        }
                        MessageController.DatatableToCSV(ds.Tables[0], filepathforupload);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            responseDetail.Success = true;
            responseDetail.Data = curFile;
            return Json(responseDetail, JsonRequestBehavior.AllowGet);
        }
        public void GetMessages()
        {
            try
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
                DataSet ds = new DataSet();
                using (var cmd = new SqlCommand("Test_Update_TwillioMessages", sqlConnection))
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
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null)
                {
                    //save in file 
                    string curFile = System.Web.Hosting.HostingEnvironment.MapPath("~/Message/Test_LastDayMessages.csv");
                    if (System.IO.File.Exists(curFile))
                    {
                        System.IO.File.Delete(curFile);
                    }
                    MessageController.DatatableToCSV(ds.Tables[0], curFile);
                }
            }
            catch (Exception ex)
            {
                
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
                using (var cmd = new SqlCommand("Test_Update_FlowdetailsByMessageID", sqlConnection))
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
                using (var cmd = new SqlCommand("Test_GetLastMessageDate", sqlConnection))
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
                //var messages = MessageResource.Read(dateSentAfter: new DateTime(LastMessageDate.Year, LastMessageDate.Month, LastMessageDate.Day, LastMessageDate.Hour, LastMessageDate.Minute, LastMessageDate.Second), dateSentBefore: new DateTime(currentdatetime.Year, currentdatetime.Month, currentdatetime.Day, currentdatetime.Hour, currentdatetime.Minute, currentdatetime.Second));
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
                using (var cmd = new SqlCommand("Test_Update_TwillioMessages", sqlConnection))
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
                Insertlastmessages();
                jsonRequest = JsonConvert.DeserializeObject<List<FlowDetail>>(json);
                DataTable Dt = DatatableConvert.ToDataTable<FlowDetail>(jsonRequest);
                using (var cmd = new SqlCommand("Test_Insert_Flowdetails", sqlConnection))
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
                        //if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        //{
                        //    //save in file 
                        //    string curFile = System.Web.Hosting.HostingEnvironment.MapPath("~/Message/Test_LastDayMessages.csv");
                        //    if (System.IO.File.Exists(curFile))
                        //    {
                        //        System.IO.File.Delete(curFile);
                        //    }
                        //    MessageController.DatatableToCSV(ds.Tables[0], curFile);
                        //}
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
    }
}