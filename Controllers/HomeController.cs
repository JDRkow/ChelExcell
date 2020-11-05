using ChelExcell.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChelExcell.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            string connString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\database.mdf;Integrated Security=True";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                List<Area> currentArea = new List<Area>();
                using (var cmd = new SqlCommand("SELECT NameArea, AreaParameter FROM FileTable", conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        List<Area> dictList = new List<Area>();
                        while (reader.Read())
                        {
                            currentArea.Add(new Area()
                            {
                                //Id = Int32.Parse(reader["id"].ToString()),
                                NameArea = reader["NameArea"].ToString(),
                                AreaParameter = Int32.Parse(reader["AreaParameter"].ToString())
                            });
                        }
                    }
                    conn.Close();
                }
                conn.Open();
                SqlCommand comDelete = new SqlCommand("DELETE FROM FileTable", conn);
                comDelete.ExecuteNonQuery();
                conn.Close();
                return View(currentArea);
            }
        }
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            if (upload != null)
            {
                string fileName = System.IO.Path.GetFileName(upload.FileName);
                upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                byte[] file = new byte[upload.ContentLength];
                upload.InputStream.Read(file, 0, upload.ContentLength);
                using (MemoryStream stream = new MemoryStream(file))
                using (ExcelPackage excelPackage = new ExcelPackage(stream))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    var worksheet = excelPackage.Workbook.Worksheets.First();
                    for (int i = worksheet.Dimension.Start.Row+1; i <= worksheet.Dimension.End.Row; i++)
                    {
                        string connString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\database.mdf;Integrated Security=True";
                        using (SqlConnection conn = new SqlConnection(connString))
                        {
                            SqlCommand com = new SqlCommand("INSERT INTO FileTable(NameArea, AreaParameter) VALUES (@p_namearea, @p_valueparameter)", conn);
                            com.Parameters.Add("@p_namearea", SqlDbType.NVarChar).Value = worksheet.Cells[i, 1].Value.ToString();
                            com.Parameters.Add("@p_valueparameter", SqlDbType.Int).Value = Int32.Parse(worksheet.Cells[i, 2].Value.ToString());
                            conn.Open();
                            com.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                }
            }
            return RedirectToAction("Index");
        }
    }
}