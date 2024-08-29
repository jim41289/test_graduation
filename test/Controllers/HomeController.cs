using graduate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Graduation.Models;
using System.Data.Entity;
using System.Net;
using Microsoft.Ajax.Utilities;

namespace graduate.Controllers
{
    public class YourDbContext : DbContext
    {
        // 替換成你的資料庫連接字串名稱
        public YourDbContext() : base("Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Encrypt=False")
        {
        }

        // 定義資料表與模型的對應
        public DbSet<GraduationReview> GraduationReviews { get; set; }
    }
    public class HomeController : Controller
    {



        public HomeController()
        {
            // 設置 EPPlus 許可上下文
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;



        }
        public ActionResult index()
        {
            validnum validNumGenerator = new validnum();
            string validateNum = validNumGenerator.createrandnum(5); // 生成5位的驗證碼
            Session["validateNum"] = validateNum; // 使用統一的鍵名保存驗證碼到Session
            return View();
        }


        public ActionResult DoLogin(student_login_in student)
        {
            string sessionValiCode = Session["validateNum"] as string;
            if (sessionValiCode != student.ValiCode)
            {
                TempData["Errmsg"] = "驗證碼錯誤";
                return RedirectToAction("index");
            }


            if (string.IsNullOrEmpty(student.account) || string.IsNullOrEmpty(student.password))
            {
                TempData["Errmsg"] = "帳號密碼為空";
                return RedirectToAction("index");
            }
            else
            {
                try
                {
                    string checkpwd = student.hashi(student);
                    string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
                    SqlConnection conn = new SqlConnection(connstr);
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select * from student_login where account=@account and  Password=@Password");
                    cmd.Connection = conn;
                    cmd.Parameters.AddWithValue("@account", student.account);
                    cmd.Parameters.AddWithValue("@Password", checkpwd);
                    SqlDataAdapter adpt = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    adpt.SelectCommand = cmd;
                    adpt.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        Session["Account"] = student.account;
                        SqlCommand stu_name = new SqlCommand("select student_name,student_system from student_base_data where student_No=@student_No");
                        stu_name.Parameters.AddWithValue("@student_No", student.account);
                        stu_name.Connection = conn;
                        SqlDataReader reader = stu_name.ExecuteReader();
                        if (reader.Read()) // 在讀取資料之前先檢查是否有資料可以讀取
                        {
                            string name = reader["student_name"].ToString();
                            string system = reader["student_system"].ToString(); // 修改 "system" 為 "student_system"
                            Session["name"] = name;
                            TempData["name"] = name;
                            Session["system"] = system;
                        }
                        reader.Close();
                        cmd = null;
                        SqlCommand question = new SqlCommand("select * from student_login where account=@account and  Password=@Password");
                        question.Connection = conn;
                        question.Parameters.AddWithValue("@account", student.account);
                        question.Parameters.AddWithValue("@Password", checkpwd);
                        SqlDataReader reader1 = question.ExecuteReader();
                        if (reader1.Read())
                        {
                            if (string.IsNullOrEmpty(reader1["like_food"].ToString()) || string.IsNullOrEmpty(reader1["like_people"].ToString()))
                            {
                                return RedirectToAction("questions");
                            }
                            else
                            {
                                return RedirectToAction("Student_index");
                            }
                        }

                    }
                    else
                    {
                        TempData["Errmsg"] = "帳號或密碼錯誤";
                        return RedirectToAction("index");
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }


            return null;
        }
        public ActionResult questions()
        {
            return View();
        }
        public ActionResult Doquestions(string like_food, string like_people)
        {
            string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
            SqlConnection sqlConnection = new SqlConnection(connstr);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand("update student_login set like_food=@like_food,like_people=@like_people where account=@account");
            sqlCommand.Connection = sqlConnection;
            sqlCommand.Parameters.AddWithValue("@account", Session["account"] as string);
            sqlCommand.Parameters.AddWithValue("@like_food", like_food);
            sqlCommand.Parameters.AddWithValue("@like_people", like_people);
            sqlCommand.ExecuteNonQuery();
            return RedirectToAction("Student_index");
        }
        public ActionResult forget()
        {
            validnum validNumGenerator = new validnum();
            string validateNum = validNumGenerator.createrandnum(5); // 生成5位的驗證碼
            Session["validateNum"] = validateNum; // 使用統一的鍵名保存驗證碼到Session
            return View();
        }
        public ActionResult GetValidateCode()
        {
            string validateNum = Session["validateNum"] as string; // 從Session中讀取驗證碼
            if (string.IsNullOrEmpty(validateNum))
            {
                return new HttpStatusCodeResult(400, "驗證碼生成錯誤");
            }

            validnum validNumGenerator = new validnum();
            CreateImage(validateNum); // 生成驗證碼圖片
            return null;
        }
        public ActionResult Doforget(forget_password_info info)
        {
            string sessionValiCode = Session["validateNum"] as string;
            if (sessionValiCode != info.ValiCode)
            {
                TempData["Errmsg"] = "驗證碼錯誤";
                return RedirectToAction("forget");
            }
            try
            {

                string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
                SqlConnection conn = new SqlConnection(connstr);
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from student_login where account=@account and like_food=@like_food and like_people=@like_people");
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@account", info.account);
                cmd.Parameters.AddWithValue("@like_food", info.like_food);
                cmd.Parameters.AddWithValue("@like_people", info.like_people);
                SqlDataAdapter adpt = new SqlDataAdapter();
                adpt.SelectCommand = cmd;
                DataSet ds = new DataSet();
                adpt.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                {
                    TempData["Errmsg"] = "帳號或問題回答錯誤";
                    return RedirectToAction("forget");
                }
                else
                {
                    TempData["Account"] = info.account;
                    return RedirectToAction("Revise");
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return View();
        }
        public ActionResult Revise()
        {

            return View();
        }
        public ActionResult Dochange(changepwd checkpwd)
        {
            student_login_out response = new student_login_out();
            student_login_in hasi = new student_login_in();

            if (checkpwd.newpassword != checkpwd.checkpassword)
            {
                TempData["Account"] = checkpwd.account;
                TempData["Errmsg"] = "密碼不一致";
                return RedirectToAction("Revise");
            }
            try
            {
                string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
                SqlConnection conn = new SqlConnection(connstr);
                conn.Open();
                SqlCommand cmd = new SqlCommand("update student_login set Password=@Password where account=@account");
                cmd.Connection = conn;
                student_login_in student = new student_login_in();
                student.account = checkpwd.account;
                student.password = checkpwd.newpassword;
                cmd.Parameters.AddWithValue("@Password", hasi.hashi(student));
                cmd.Parameters.AddWithValue("@account", checkpwd.account);
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }


            return RedirectToAction("Index");
        }
        public void CreateImage(string validateNum)
        {
            if (validateNum == null || validateNum.Trim() == String.Empty)
                return;
            //生成bitmap圖像
            Bitmap image = new Bitmap(validateNum.Length * 12 + 10, 22);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成隨機生成器
                Random random = new Random();
                g.Clear(Color.White);
                //畫圖片背景噪音線
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 12, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateNum, font, brush, 2, 2);
                //畫圖片的前景噪音點
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //畫圖片的邊框線
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                //將圖像保存到指定的流
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                Response.ClearContent();
                Response.ContentType = "image/Gif";
                Response.BinaryWrite(ms.ToArray());
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
        [HttpPost]
        public ActionResult ImportExcel(HttpPostedFileBase file)
        {
            student_login_in info = new student_login_in();
            if (file != null && file.ContentLength > 0)
            {
                var package = new ExcelPackage(file.InputStream);
                var workSheet = package.Workbook.Worksheets.First();
                var start = workSheet.Dimension.Start;//行
                var end = workSheet.Dimension.End;
                string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
                SqlConnection sqlConnection = new SqlConnection(connstr);
                sqlConnection.Open();

                for (int row = start.Row + 1; row <= end.Row + 1; row++)
                {
                    string student_No = workSheet.Cells[row, 1].Text;
                    string student_sex = workSheet.Cells[row, 2].Text;
                    string student_Name = workSheet.Cells[row, 3].Text;
                    string student_phone = workSheet.Cells[row, 4].Text;
                    string student_bron = workSheet.Cells[row, 5].Text;
                    string student_system = workSheet.Cells[row, 6].Text;
                    string student_schoolyear = workSheet.Cells[row, 7].Text;
                    string student_status = workSheet.Cells[row, 8].Text;
                    string student_email = workSheet.Cells[row, 9].Text;
                    string student_class = workSheet.Cells[row, 10].Text;

                    // 讀取每個儲存格的值
                    // var cellValue = workSheet.Cells[row, col].Text;
                    // 處理儲存格的值，例如保存到資料庫
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM student_base_data WHERE student_No = @student_No", sqlConnection);
                    checkCmd.Parameters.AddWithValue("@student_No", student_No);
                    int userExists = (int)checkCmd.ExecuteScalar();

                    if (userExists == 0 && !string.IsNullOrEmpty(student_No))
                    {
                        SqlCommand cmd = new SqlCommand("INSERT INTO student_base_data (student_No, student_sex,  student_name,  student_phone, student_bron, student_system, student_schoolyear, student_status, student_email,student_class) VALUES (@student_No, @student_sex,  @student_name,  @student_phone, @student_bron, @student_system, @student_schoolyear, @student_status, @student_email,@student_class)", sqlConnection);
                        cmd.Parameters.AddWithValue("@student_No", student_No);
                        cmd.Parameters.AddWithValue("@student_sex", student_sex);
                        cmd.Parameters.AddWithValue("@student_name", student_Name);
                        cmd.Parameters.AddWithValue("@student_phone", student_phone);
                        cmd.Parameters.AddWithValue("@student_bron", student_bron);
                        cmd.Parameters.AddWithValue("@student_system", student_system);
                        cmd.Parameters.AddWithValue("@student_schoolyear", student_schoolyear);
                        cmd.Parameters.AddWithValue("@student_status", student_status);
                        cmd.Parameters.AddWithValue("@student_email", student_email);
                        cmd.Parameters.AddWithValue("@student_class", student_class);
                        cmd.ExecuteNonQuery();
                        SqlCommand login = new SqlCommand("INSERT INTO student_login(account,Password) VALUES (@account,@Password)", sqlConnection);
                        login.Parameters.AddWithValue("@account", student_No);
                        string bron = student_bron.Replace("/", "");
                        info.account = student_No;
                        info.password = bron;
                        login.Parameters.AddWithValue("@Password", info.hashi(info));
                        login.ExecuteNonQuery();
                        return RedirectToAction("Teacher_Student_data");
                    }
                }
            }
            return RedirectToAction("Teacher_Student_data");
        }
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase CPE, HttpPostedFileBase Certificate, HttpPostedFileBase race, HttpPostedFileBase race_two, HttpPostedFileBase race_three)
        {
            // 確保資料夾存在
            string rootPath = @"C:\Users\Administrator\Desktop\test\test\test\images\";

            // 資料夾路徑
            string cpePath = Path.Combine(rootPath, "CPE");
            string certificatePath = Path.Combine(rootPath, "Certificates");
            string racePath = Path.Combine(rootPath, "Races");

            string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
            SqlConnection sqlConnection = new SqlConnection(connstr);
            sqlConnection.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = sqlConnection;

            // 處理 CPE 檔案
            if (CPE != null && CPE.ContentLength > 0)
            {
                string cpeFileName = Session["Account"] as string + "1_1" + System.IO.Path.GetExtension(CPE.FileName);
                string cpeFilePath = Path.Combine(cpePath, cpeFileName);
                CPE.SaveAs(cpeFilePath);
                cmd.CommandText = "INSERT INTO GraduationReview (StudentID, UploadDate, FileID, FilePath, FileStatus, academic_system) VALUES (@StudentID, @UploadDate, @FileID, @FilePath, @FileStatus, @academic_system)";
                cmd.Parameters.AddWithValue("@StudentID", Session["Account"] as string);
                cmd.Parameters.AddWithValue("@UploadDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@FileID", cpeFileName);
                cmd.Parameters.AddWithValue("@FilePath", cpeFilePath);
                cmd.Parameters.AddWithValue("@FileStatus", "待審核");
                cmd.Parameters.AddWithValue("@academic_system", Session["system"] as string);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }

            // 處理 Certificate 檔案
            if (Certificate != null && Certificate.ContentLength > 0)
            {
                string certificateFileName = Session["Account"] as string + "2_1" + System.IO.Path.GetExtension(Certificate.FileName);
                string certificateFilePath = Path.Combine(certificatePath, certificateFileName);
                Certificate.SaveAs(certificateFilePath);
                cmd.CommandText = "INSERT INTO GraduationReview (StudentID, UploadDate, FileID, FilePath, FileStatus, academic_system) VALUES (@StudentID, @UploadDate, @FileID, @FilePath, @FileStatus, @academic_system)";
                cmd.Parameters.AddWithValue("@StudentID", Session["Account"] as string);
                cmd.Parameters.AddWithValue("@UploadDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@FileID", certificateFileName);
                cmd.Parameters.AddWithValue("@FilePath", certificateFilePath);
                cmd.Parameters.AddWithValue("@FileStatus", "待審核");
                cmd.Parameters.AddWithValue("@academic_system", Session["system"] as string);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }

            // 處理 race 檔案
            if (race != null && race.ContentLength > 0)
            {
                string raceFileName = Session["Account"] as string + "3_1" + System.IO.Path.GetExtension(race.FileName);
                string raceFilePath = Path.Combine(racePath, raceFileName);
                race.SaveAs(raceFilePath);
                cmd.CommandText = "INSERT INTO GraduationReview (StudentID, UploadDate, FileID, FilePath, FileStatus, academic_system) VALUES (@StudentID, @UploadDate, @FileID, @FilePath, @FileStatus, @academic_system)";
                cmd.Parameters.AddWithValue("@StudentID", Session["Account"] as string);
                cmd.Parameters.AddWithValue("@UploadDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@FileID", raceFileName);
                cmd.Parameters.AddWithValue("@FilePath", raceFilePath);
                cmd.Parameters.AddWithValue("@FileStatus", "待審核");
                cmd.Parameters.AddWithValue("@academic_system", Session["system"] as string);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }

            // 處理 race_two 檔案
            if (race_two != null && race_two.ContentLength > 0)
            {
                string raceTwoFileName = Session["Account"] as string + "3_2" + System.IO.Path.GetExtension(race_two.FileName);
                string raceTwoFilePath = Path.Combine(racePath, raceTwoFileName);
                race_two.SaveAs(raceTwoFilePath);
                cmd.CommandText = "INSERT INTO GraduationReview (StudentID, UploadDate, FileID, FilePath, FileStatus, academic_system) VALUES (@StudentID, @UploadDate, @FileID, @FilePath, @FileStatus, @academic_system)";
                cmd.Parameters.AddWithValue("@StudentID", Session["Account"] as string);
                cmd.Parameters.AddWithValue("@UploadDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@FileID", raceTwoFileName);
                cmd.Parameters.AddWithValue("@FilePath", raceTwoFilePath);
                cmd.Parameters.AddWithValue("@FileStatus", "待審核");
                cmd.Parameters.AddWithValue("@academic_system", Session["system"] as string);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }

            // 處理 race_three 檔案
            if (race_three != null && race_three.ContentLength > 0)
            {
                string raceThreeFileName = Session["Account"] as string + "3_3" + System.IO.Path.GetExtension(race_three.FileName);
                string raceThreeFilePath = Path.Combine(racePath, raceThreeFileName);
                race_three.SaveAs(raceThreeFilePath);
                cmd.CommandText = "INSERT INTO GraduationReview (StudentID, UploadDate, FileID, FilePath, FileStatus, academic_system) VALUES (@StudentID, @UploadDate, @FileID, @FilePath, @FileStatus, @academic_system)";
                cmd.Parameters.AddWithValue("@StudentID", Session["Account"] as string);
                cmd.Parameters.AddWithValue("@UploadDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@FileID", raceThreeFileName);
                cmd.Parameters.AddWithValue("@FilePath", raceThreeFilePath);
                cmd.Parameters.AddWithValue("@FileStatus", "待審核");
                cmd.Parameters.AddWithValue("@academic_system", Session["system"] as string);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }

            // 返回一個確認訊息或重定向到其他頁面
            return RedirectToAction("Lmport_data");

        }


        public ActionResult Logout()
        {
            Session["Account"] = null;
            return RedirectToAction("index");
        }
        public ActionResult Teacher_DoLogin(string teacher_account, string teacher_password)
        {

            string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            SqlCommand cmd = new SqlCommand("select * from teacher_login where account=@account and  Password=@Password");
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@account", teacher_account);
            cmd.Parameters.AddWithValue("@Password", teacher_password);
            SqlDataAdapter adpt = new SqlDataAdapter();
            DataSet ds = new DataSet();
            adpt.SelectCommand = cmd;
            adpt.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return RedirectToAction("Teacher_index");
            }
            else
            {
                TempData["Errmsg"] = "帳號或密碼錯誤";
                return RedirectToAction("Teacher_login");
            }

        }
        public ActionResult Teacher_Index()
        {
            return View();
        }
        public ActionResult Lmport_data()
        {
            string name = Session["name"] as string;
            TempData["name"] = name;
            return View();
        }
        public ActionResult Student_Index()
        {
            return View();
        }
        public ActionResult Lmport_data_teacher()
        {

            return View();
        }
        public ActionResult Teacher_login()
        {
            return View();
        }
        public ActionResult Teacher_Student_data()
        {
            List<student_basedata> students = new List<student_basedata> { };
            string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
            SqlConnection sqlConnection = new SqlConnection(connstr);
            sqlConnection.Open();
            SqlCommand cmd = new SqlCommand("select * from student_base_data", sqlConnection);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                student_basedata student = new student_basedata
                {
                    student_No = reader["student_No"].ToString(),
                    student_sex = reader["student_sex"].ToString(),
                    student_name = reader["student_name"].ToString(),
                    student_phone = reader["student_phone"].ToString(),
                    student_bron = reader["student_bron"].ToString(),
                    student_system = reader["student_system"].ToString(),
                    student_schoolyear = reader["student_schoolyear"].ToString(),
                    student_status = reader["student_status"].ToString(),
                    student_email = reader["student_email"].ToString()
                };
                students.Add(student);
            }
            return View(students);
        }
        public ActionResult Account()
        {
            return View();
        }

        public ActionResult Review()
        {
            List<studentModel>students=new List<studentModel> { };
            if (students.Count==0)
            {
                string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
                SqlConnection sqlConnection = new SqlConnection(connstr);
                sqlConnection.Open();

                SqlCommand cmd = new SqlCommand("select * from student_base_data", sqlConnection);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    studentModel student = new studentModel
                    {
                        student_No = reader["student_No"].ToString(),
                        student_name = reader["student_name"].ToString(),
                        student_system = reader["student_system"].ToString(),
                        student_status = reader["student_status"].ToString(),
                    };
                    students.Add(student);
                }
                  reader.Close();
                fileModel file=new fileModel();
                List <fileModel> files = file.files(students);
                ViewBag.Students = students;
                ViewBag.Files = files;
            }

            return View(students);
        }
        public ActionResult Search(string system, string student_class)
        {
            List<studentModel> students = new List<studentModel>();
            List<fileModel> files = new List<fileModel>();
            string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";

            using (SqlConnection sqlConnection = new SqlConnection(connstr))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;

                if (string.IsNullOrEmpty(system) && string.IsNullOrEmpty(student_class))
                {
                    return RedirectToAction("Review");
                }
                else
                {
                    string query = "select * from student_base_data where 1=1";

                    if (!string.IsNullOrEmpty(system))
                    {
                        query += " and student_system=@student_system";
                        sqlCommand.Parameters.AddWithValue("@student_system", system);
                    }

                    if (!string.IsNullOrEmpty(student_class))
                    {
                        query += " and student_class=@student_class";
                        sqlCommand.Parameters.AddWithValue("@student_class", student_class);
                    }

                    sqlCommand.CommandText = query;

                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        studentModel student = new studentModel
                        {
                            student_No = reader["student_No"].ToString(),
                            student_name = reader["student_name"].ToString(),
                            student_system = reader["student_system"].ToString(),
                            student_status = reader["student_status"].ToString(),
                        };
                        students.Add(student);
                    }
                    fileModel file = new fileModel();
                    files = file.files(students);
                }
            }
            ViewBag.Students = students;
            ViewBag.Files = files;

            return View("Review");
        }
        public ActionResult ViewPhoto(string studentNo)
        {
         
            ViewBag.StudentNo = studentNo;
            return View();
        }
    }
  
}
