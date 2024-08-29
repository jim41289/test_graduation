using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
namespace graduate.Models
{
    public class forget_password_info
    {
        public string account { get; set; }
        public string like_food { get; set; }
        public string like_people { get; set; }
        public string ValiCode { get; set; }
    }

    public class student_login_in
    {
        public string account { get; set; }
        public string password { get; set; }
        public string ValiCode { get; set; }
        public string hashi(student_login_in student)
        {
            SHA256 sha = SHA256.Create();
            string salt = student.account.Substring(0, 1).ToLower();
            byte[] bytes = Encoding.UTF8.GetBytes(salt + student.password);
            byte[] hash = sha.ComputeHash(bytes);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            String chechpwd = result.ToString();

            return chechpwd;
        }
    }
    public class student_login_out
    {

        public string result_msg { get; set; }
        public string Errmsg { get; set; }
    }
    public class changepwd
    {
        public string account { get; set; }
        public string newpassword { get; set; }
        public string checkpassword { get; set; }
    }
    public class student_basedata
    {
        public string student_No { get; set; }
        public string student_sex { get; set; }
        public string student_name {  get; set; }
        public string student_phone {  get; set; }
        public string student_email { get; set; }
        public string student_bron { get; set; }
        public string student_system { get; set; }
        public string student_schoolyear { get; set; }
        public string student_status { get; set; }
    }
    public class validnum : Controller
    {
        public string createrandnum(int numcount)
        {
            string allchar = "1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            string[] allchararray = allchar.Split(',');
            string randnum = "";
            int temp = -1;
            Random rand = new Random();
            for (int i = 0; i < numcount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(35);
                if (temp == t)
                {
                    return createrandnum(numcount);
                }
                temp = t;
                randnum += allchararray[temp];
            }



            return randnum;
        }

    }
}
namespace Graduation.Models
{
    public class GraduationReview
    {
        public int ReviewID { get; set; } // 主鍵
        public string StudentID { get; set; }
        public DateTime UploadDate { get; set; }
        public Guid FileID { get; set; }
        public string FilePath { get; set; }
        public string FileStatus { get; set; }
        public int academic_system { get; set; }
    }
}
public class studentModel
{
    public string student_No { get; set; }
    public string student_name { get; set; }
    public string student_system { get; set; }
    public string student_status { get; set; }
    
}
public class fileModel
{
    public string student_No { get; set; }
    public string file_CPE { get; set; }
    public string file_Certificate { get; set; }
    public string race_1 { get; set; }
    public string race_2 { get; set; }
    public string race_3 { get; set; }

    public List<fileModel> files( List<studentModel> students)
    {
        List <fileModel> files = new List<fileModel> { };
        string connstr = "Data Source=CSIE-TEST2;Initial Catalog=Student_data;User ID=TEST03;Password=1qaz@WSX;Encrypt=False";
        SqlConnection sqlConnection = new SqlConnection(connstr);
        sqlConnection.Open();
        SqlCommand cmd = new SqlCommand("select FileID,FilePath,FileStatus from GraduationReview where StudentID=@StudentID");
        cmd.Connection = sqlConnection;
        for (int i = 0;i<students.Count;i++)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@StudentID", students[i].student_No);
            SqlDataReader reader = cmd.ExecuteReader();
            string CPE = "未上傳";
            string file_Certificate = "未上傳";
            string race_1 = "未上傳";
            string race_2 = "未上傳";
            string race_3 = "未上傳";

            while (reader.Read())
            {
                if((!string.IsNullOrEmpty(reader["FileID"].ToString())))
                {
                if (reader["FileID"].ToString().Substring(10,3)=="1_1")
                {
                    CPE = reader["FileStatus"].ToString(); 
                }
                if (reader["FileID"].ToString().Substring(10, 3) == "2_1")
                {
                    file_Certificate = reader["FileStatus"].ToString();
                }
                if (reader["FileID"].ToString().Substring(10, 3) == "3_1")
                {
                    race_1 = reader["FileStatus"].ToString();
                }
                if (reader["FileID"].ToString().Substring(10, 3) == "3_2")
                {
                    race_2 = reader["FileStatus"].ToString();
                }
                if (reader["FileID"].ToString().Substring(10, 3) == "3_3")
                {
                    race_3 = reader["FileStatus"].ToString();
                }
                }

            };
            reader.Close();
            
            fileModel file = new fileModel
            {
                student_No = students[i].student_No,
                file_CPE = CPE,
                file_Certificate = file_Certificate,
                race_1 = race_1,
                race_2 = race_2,
                race_3 = race_3,
            };
            files.Add(file);

        }

        return files;
    }

}


