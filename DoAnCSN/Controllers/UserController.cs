//using DoAnCSN.Models;
//using System;
//using System.Data.Entity.Infrastructure;
//using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
//using System.Text;
//using System.Web.Mvc;
//using System.Web;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DoAnCSN.Models;

namespace DoAnCSN.Controllers
{
    public class UserController : Controller
    {
        QLSpaEntities db = new QLSpaEntities();

        private string convertNgay(string Ngay)
        {
            string[] arr = Ngay.Split('-');
            string s = arr[2] + "/" + arr[1] + "/" + arr[0];
            return s;
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            return View(); 
        }

        [HttpPost]
        public ActionResult DangKy(FormCollection f)
        {
            string hoTen = f["HO_TEN"];
            string userName = f["USER_NAME"];
            string ngaySinh = convertNgay(f["NGAY_SINH"]);
            string email = f["EMAIL"];
            string phoneNumber = f["PHONE_NUMBER"];
            AccountCustomer acc = db.AccountCustomers.FirstOrDefault(u => u.EMAIL == email || u.USER_NAME == userName || u.PHONE_NUMBER == phoneNumber);
            if (acc == null)
            {
                acc = new AccountCustomer();
                var matKhauNgauNhien = GenerateRandomPassword(8);
                var matKhauMaHoa = HashPassword(matKhauNgauNhien);
                acc.EMAIL = email;
                acc.PHONE_NUMBER = phoneNumber;
                acc.NGAY_SINH = ngaySinh;
                acc.USER_NAME = userName;
                acc.PASSWORD = matKhauMaHoa;
                acc.STATUS = 1;
                acc.NAME = hoTen;

                db.AccountCustomers.Add(acc);
                db.SaveChanges();
                GuiEmailXacNhan(acc.EMAIL, acc, matKhauNgauNhien);

                int MaTK = db.AccountCustomers
                             .Where(w => w.PHONE_NUMBER.Equals(phoneNumber))
                             .Select(s => s.ID).FirstOrDefault();

                CUSTOMER c = new CUSTOMER();
                c.HO_TEN = hoTen;
                c.NGAY_SINH = ngaySinh;
                c.PHONE_NUMBER = phoneNumber;
                c.MaTT = 1;
                c.MaTK = MaTK;
                db.CUSTOMERs.Add(c);
                db.SaveChanges();
                //TempData["dangkythanhcong"] = true;
                return RedirectToAction("DangNhap");
            }
            else
            {

                if (acc.EMAIL == email)
                {
                    ViewBag.thongbao = "Email đã được đăng ký trước đó.";
                }
                else if (acc.USER_NAME == userName)
                {
                    ViewBag.thongbao = "Tên đăng nhập đã được đăng ký trước đó.";
                }
                else if (acc.PHONE_NUMBER == phoneNumber)
                {
                    ViewBag.thongbao = "Số điện thoại đã được đăng ký trước đó.";
                }
                return View();
            }
        }
        private void GuiEmailXacNhan(string email, AccountCustomer kh, string matKhauNgauNhien)
        {
            string username = "phamhuutien004@gmail.com";
            string password = "arih lxoe frtv wfjq";

            try
            {
                string body = $@"
         <table style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd;'>
             <tr>
                 <td style='background-color: #4CAF50; padding: 25px; text-align: center; color: white;'>
                     <h2 style='font-size: 28px; margin: 0;'>Xác nhận đăng ký thành công</h2>
                     <p style='font-size: 16px; margin: 0;'>Cảm ơn bạn đã đăng ký thành viên tại <strong>Spa Phạm Hữu Tiến</strong>!</p>
                 </td>
             </tr>
             <tr>
                 <td style='padding: 30px; background-color: #fafafa;'>
                     <h3 style='font-size: 22px; color: #333; margin-bottom: 15px;'>Thông tin đăng ký của bạn:</h3>
                     <p><strong>Tên đăng nhập:</strong> {kh.USER_NAME}</p>
                     <p><strong>Số điện thoại:</strong> {kh.PHONE_NUMBER}</p>
                     <p><strong>Email:</strong> {kh.EMAIL}</p>
                     <p style='color: #FF5733; font-size: 18px; margin-top: 20px;'><strong>MẬT KHẨU CỦA BẠN LÀ:</strong> {matKhauNgauNhien}</p>
                     
                     <p style='margin-top: 20px; font-size: 14px; color: #666;'>Nếu có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email: 
                         <a href='mailto:phamhuutien004@gmail.com' style='color: #4CAF50; text-decoration: none;'>phamhuutien004@gmail.com</a>.
                     </p>
                     <p style='margin-top: 15px; font-size: 14px; color: #666;'>Trân trọng,<br/><strong>Đội ngũ Spa Phạm Hữu Tiến</strong></p>
                 </td>
             </tr>
             <tr>
                 <td style='background-color: #4CAF50; padding: 15px; text-align: center; color: white;'>
                     <p style='margin: 0; font-size: 14px;'>&copy; 2024 Spa Phạm Hữu Tiến. All rights reserved.</p>
                 </td>
             </tr>
         </table>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(username);
                mail.To.Add(email);
                mail.Subject = "Xác Nhận Đơn Hàng";
                mail.Body = body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(username, password);
                smtp.EnableSsl = true;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gửi email thất bại: " + ex.Message);
            }

        }
        private string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            Random random = new Random();
            char[] chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }

            return new string(chars);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Chuyển byte thành chuỗi hex
                }
                return builder.ToString();
            }
        }

        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection collection) //monokai
        {
            var sTenDN = collection["USER_NAME"];
            var sMatkhau = collection["PASSWORD"];

            if (String.IsNullOrEmpty(sTenDN))
            {
                ViewBag.Err1 = "Bạn chưa nhập tên đăng nhập";
                return View();
            }

            if (string.IsNullOrEmpty(sMatkhau))
            {
                ViewBag.Err2 = "Phải nhập mật khẩu";
                return View();
            }
            string _hashPassword = HashPassword(sMatkhau);

            AccountCustomer kh = db.AccountCustomers.SingleOrDefault(n => n.USER_NAME == sTenDN && n.PASSWORD == _hashPassword && n.STATUS == 1); 

            var acc = (from u in db.USERS
                       join ur in db.USER_ROLE on u.ID equals ur.USER_ID
                       where u.STATUS == 1 &&
                             u.USER_NAME.Equals(sTenDN) &&
                             u.PASSWORD.Equals(sMatkhau)
                       select new
                       {
                           u.ID,
                           ur.ROLE_ID
                       }).SingleOrDefault();

            if (kh != null)
            {
                int MaTK = kh.ID;
                string tenKH = db.CUSTOMERs
                                 .Where(w => w.MaTK == MaTK)
                                 .Select(s => s.HO_TEN).SingleOrDefault();

                ViewBag.ThongBao = "Chúc mừng đăng nhập thành công";
                Session["USER_NAME"] = kh.USER_NAME;
                // 'user' là đối tượng người dùng đã đăng nhập

                Session["ID"] = kh;
                Session["MaKH"] = MaTK;
                Session["NAME"] = kh.NAME;

                if (collection["remember"] == "true")
                {
                    Response.Cookies["NAME"].Value = sTenDN;
                    Response.Cookies["PASSWORD"].Value = sMatkhau;
                    Response.Cookies["NAME"].Expires = DateTime.Now.AddDays(1);
                    Response.Cookies["PASSWORD"].Expires = DateTime.Now.AddDays(1);
                }
                else
                {
                    Response.Cookies["NAME"].Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies["PASSWORD"].Expires = DateTime.Now.AddDays(-1);
                }

                return RedirectToAction("TrangChu", "Home");
            }
            else if(acc != null)
            {
                if(acc.ROLE_ID == 1)
                {
                    return RedirectToAction("Admin", "Admin");
                }
                else if(acc.ROLE_ID == 2)
                {
                    Session["MaNV"] = acc.ID;
                    return RedirectToAction("ThoiGianBieu", "Staff", new {MaNV = acc.ID});
                }
                else if(acc.ROLE_ID == 3)
                {
                    Session["MaNV"] = acc.ID;
                    return RedirectToAction("ThoiGianBieuPV", "StaffPV", new {MaNV = acc.ID});
                }
            }
            else
            {
                ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
            }

            return View();
        }
        public ActionResult DangXuat()
        {
            Session["NAME"] = null;

            if (Request.Cookies["NAME"] != null)
            {
                var userNameCookie = new HttpCookie("NAME");
                userNameCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(userNameCookie);
            }

            if (Request.Cookies["PASSWORD"] != null)
            {
                var passwordCookie = new HttpCookie("PASSWORD");
                passwordCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(passwordCookie);
            }

            return RedirectToAction("TrangChu", "Home");
        }
        [HttpGet]
        public ActionResult ThongTinNhanHang()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ThongTinNhanHang(CUSTOMER kh)
        {
            return View();
        }

        public ActionResult Profile(int MaKH)
        {
            var user = db.AccountCustomers.Where(w => w.ID.Equals(MaKH)).SingleOrDefault();

            if (user == null)
            {
                return RedirectToAction("DangNhap", "User");
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Profile(FormCollection f)
        {
            int MaTK = int.Parse(f["MaTK"]);
            var dbuser = db.AccountCustomers.FirstOrDefault(u => u.ID == MaTK);

            if (dbuser != null)
            {
                string email = f["EMAIL"];
                string phoneNumber = f["PHONE_NUMBER"];
                string name = f["NAME"];
                string diaChi = f["DiaChi"];
                dbuser.NAME = name;
                dbuser.EMAIL = email;
                dbuser.PHONE_NUMBER = phoneNumber;
                dbuser.DIACHI = diaChi;

                db.SaveChanges();

                HttpContext.Cache.Remove("UserCacheKey");
                Session["ID"] = dbuser;

                TempData["thongbao"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile", new {MaKH = MaTK});
            }
            else
            {
                TempData["thongbao"] = "Không tìm thấy người dùng!";
                return RedirectToAction("Profile");
            }
        }
    }
}
