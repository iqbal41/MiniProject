using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Data.SqlClient;
using System.Configuration;
using MiniProject.Models;

namespace MiniProject.Controllers
{
    public class HomeController : Controller 
    {
        //
        // GET: /Home/
        private SMSSendEntities db = new SMSSendEntities();

        public object DataObjectFactory { get; private set; }

        public ActionResult Index()
        {
            MessageContacts msgCon = new MessageContacts();
            foreach (var msg in db.Messages.ToList())
            {
                MessageContacts msgContact = new MessageContacts();
                msgContact.MessageText = msg.Text;
                var sendDate = db.MessagesByContacts.Where(x => x.MessageID == msg.MessageID).FirstOrDefault().SendDate;
                msgContact.SendDate = sendDate;
                msgContact.SendTime = sendDate.ToString("hh:mm tt");
                msgCon.MessageCollection.Add(msgContact);
            }

            return View(msgCon);
        }
        [HttpPost]
        public ActionResult UploadAndSave(HttpPostedFileBase file,MessageContacts message)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    DataSet ds = new DataSet();
                    if (Request.Files["file"].ContentLength > 0 && !string.IsNullOrEmpty(message.MessageText))
                    {
                        string fileExtension =
                                             System.IO.Path.GetExtension(Request.Files["file"].FileName);

                        if (fileExtension == ".xls" || fileExtension == ".xlsx")
                        {
                            string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                            if (System.IO.File.Exists(fileLocation))
                            {

                                System.IO.File.Delete(fileLocation);
                            }
                            Request.Files["file"].SaveAs(fileLocation);
                            string excelConnectionString = string.Empty;
                            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            //connection String for xls file format.
                            if (fileExtension == ".xls")
                            {
                                excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                            }
                            //connection String for xlsx file format.
                            else if (fileExtension == ".xlsx")
                            {

                                excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            }
                            //Create Connection to Excel work book and add oledb namespace
                            OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                            excelConnection.Open();
                            DataTable dt = new DataTable();

                            dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            if (dt == null)
                            {
                                return null;
                            }

                            String[] excelSheets = new String[dt.Rows.Count];
                            int t = 0;
                            //excel data saves in temp file here.
                            foreach (DataRow row in dt.Rows)
                            {
                                excelSheets[t] = row["TABLE_NAME"].ToString();
                                t++;
                            }
                            OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


                            string query = string.Format("Select * from [{0}]", excelSheets[0]);
                            using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                            {
                                dataAdapter.Fill(ds);
                            }
                        }
                        if (fileExtension.ToString().ToLower().Equals(".csv"))
                        {
                            var postedFile = Request.Files[0];
                            if (postedFile.ContentLength > 0)
                            {
                                //read data from input stream
                                using (var csvReader = new System.IO.StreamReader(postedFile.InputStream))
                                {
                                    string inputLine = "";
                                    DataTable dt = new DataTable();
                                    dt.Clear();
                                    dt.Columns.Add("Contact");
                                    //read each line
                                    while ((inputLine = csvReader.ReadLine()) != null)
                                    {
                                        //get lines values
                                        string[] values = inputLine.Split(new char[] { ',' });

                                        for (int x = 0; x < values.Length; x++)
                                        {
                                           
                                            
                                            DataRow row = dt.NewRow();
                                            row["Contact"] = values[x];
                                            dt.Rows.Add(row);
                                            
                                        }
                                    }
                                    ds.Tables.Add(dt);
                                    csvReader.Close();
                                }
                            }
                            
                        }

                        var count = db.Messages.Where(x => x.MessagesByContacts.Any(y =>
                        y.SendDate.Year == DateTime.Now.Year
                        && y.SendDate.Month == DateTime.Now.Month
                        && y.SendDate.Day == DateTime.Now.Day) && x.Text == message.MessageText).Count();

                        if (count <= 2)
                        {
                            Message msg = new Message();

                            msg.Text = message.MessageText;
                            db.Messages.Add(msg);

                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                Contact cont = new Contact() { ContactNumber = row[0].ToString() };
                                db.Contacts.Add(cont);
                                MessagesByContact msgContact = new MessagesByContact();
                                msgContact.Contact = cont;
                                msgContact.Message = msg;
                                msgContact.SendDate = DateTime.Now;
                                db.MessagesByContacts.Add(msgContact);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("keyName", "Same message cannot be saved more than two times in a day.");
                            ViewBag.IsMessageSaved = true;
                        }



                        db.SaveChanges();
                        TempData["Success"] = "Message has been saved successfully.";
                       
                    }


                    return RedirectToAction("Index", "Home");
                    //return View();
                }
                else
                {
                    ModelState.AddModelError("keyName", "Something went wrong. Cannot save.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("keyName", ex.Message);
                return View();
            }
        }



    }
}
