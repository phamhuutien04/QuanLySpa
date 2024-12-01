using DoAnCSN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static System.Net.Mime.MediaTypeNames.Text;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FontSize = DocumentFormat.OpenXml.Wordprocessing.FontSize;
using System.IO;
using System.Globalization;

namespace DoAnCSN.Controllers
{
    public class StaffController : Controller
    {
        QLSpaEntities db = new QLSpaEntities();

        private static int MaNVGoldbal = 0;

        private string convertNgay(string Ngay)
        {
            string[] arr = Ngay.Split('-');
            string s = arr[2] + "/" + arr[1] + "/" + arr[0];
            return s;
        }
        private string convertGio(string Gio)
        {
            string gioConvert = "";
            string[] arr = Gio.Split(':');
            int hour = int.Parse(arr[0]);
            int minutes = int.Parse(arr[1]);
            if (minutes != 0)
            {
                if (hour < 13)
                {
                    gioConvert = hour + " giờ " + minutes + " sáng";
                }
                else
                {
                    gioConvert = hour + " giờ " + minutes + " chiều";
                }
            }
            else
            {
                if (hour < 13)
                {
                    gioConvert = hour + " giờ sáng";
                }
                else
                {
                    gioConvert = hour + " giờ chiều";
                }
            }
            return gioConvert;
        }
        public ActionResult Staff()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DangKiTuVan()
        {
            return View();
        }


        [HttpPost]
        public ActionResult DangKiTuVan(FormCollection f)
        {
            string TenKH = f["TenKH"];
            string NgaySinh = convertNgay(f["NgaySinh"]);
            string SDT = f["SDT"];
            string GioTuVan = convertGio(f["GioTuVan"]);
            string NgayTuVan = convertNgay(f["NgayTuVan"]);


            CUSTOMER c = new CUSTOMER();
            c.HO_TEN = TenKH;
            c.NGAY_SINH = NgaySinh;
            c.PHONE_NUMBER = SDT;
            c.GIO = GioTuVan;
            c.NGAY_TU_VAN = NgayTuVan;
            db.CUSTOMERs.Add(c);
            db.SaveChanges();

            return View();
        }
        // GET: Staff
        public ActionResult ThoiGianBieu(int MaNV)
        {
            MaNVGoldbal = MaNV;
            var khs = (from c in db.CUSTOMERs
                      join assign in db.AssignmentCustomers on c.ID equals assign.MaKH
                      where assign.MaNV.Equals(MaNV)
                      select c)
                      .ToList();
            return View(khs);
        }

        public ActionResult Checked(int MaKH)
        {
            var customer = db.CUSTOMERs.SingleOrDefault(w => w.ID == MaKH);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng.";
                return RedirectToAction("ThoiGianBieu", "Staff", new { MaNV = MaNVGoldbal });
            }

            DateTime ngayTuVan;
            string format = "dd/MM/yyyy";
            if (!DateTime.TryParseExact(customer.NGAY_TU_VAN, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out ngayTuVan))
            {
                TempData["Error"] = "Ngày tư vấn không hợp lệ.";
                return RedirectToAction("ThoiGianBieu", "Staff", new { MaNV = MaNVGoldbal });
            }

            if (DateTime.Now < ngayTuVan)
            {
                TempData["Error"] = "Chưa đến thời gian tư vấn. Không thể thực hiện thao tác.";
                return RedirectToAction("ThoiGianBieu", "Staff", new { MaNV = MaNVGoldbal });
            }

            customer.MaTT = 3;
            db.SaveChanges();

            TempData["Success"] = "Cập nhật trạng thái thành công.";
            return RedirectToAction("ThoiGianBieu", "Staff", new { MaNV = MaNVGoldbal });
        }

        public ActionResult LichSuChamSoc()
        {
            return View();
        }
        public ActionResult ThucHienDichVu()
        {
            return View();
        }

        public ActionResult QuanLiDangKyDV()
        {
            return View();
        }
        public ActionResult QuanLiSanPham()
        {
            return View();
        }
        public ActionResult Sidebar_Menu()
        {
            return PartialView("Sidebar_Menu");
        }
        public ActionResult Footer()
        {
            return PartialView("Footer");
        }
        public ActionResult Navbar_Custom()
        {
            return PartialView("Navbar_Custom");
        }

        public ActionResult QuanLyDonHang(int? page)
        {
            int pageSize = 6; // Number of items per page
            int pageNumber = (page ?? 1); // Default to page 1 if no page is provided

            // Get all records
            var DonDatHang = (from d in db.DonDatHangs
                              join u in db.USERS on d.MAKH equals u.ID
                              orderby d.NgayDat descending
                              select new DDH
                              {
                                  ID = d.ID,
                                  MaKH = (int)d.MAKH,
                                  TenKH = u.NAME,
                                  Phone = u.PHONE_NUMBER,
                                  NgayDat = (DateTime)d.NgayDat
                              }).ToList();

            var totalCount = DonDatHang.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var pagedDonDatHang = DonDatHang.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(pagedDonDatHang);
        }


        [HttpPost]
        public ActionResult XuatFile(int id)
        {
            try
            {
                var orderEntity = db.DonDatHangs.FirstOrDefault(d => d.ID == id);
                if (orderEntity != null)
                {
                    orderEntity.TinhTrangGiaoHang = 0;
                    db.SaveChanges();
                }
                var orderData = db.DonDatHangs
                         .Where(d => d.ID == id)
                         .Select(d => new
                         {
                             d.ID,
                             d.MAKH,
                             d.NgayDat,
                             Customer = db.AccountCustomers.FirstOrDefault(u => u.ID == d.MAKH),
                             SanPham = db.CHITIETDATHANGs
                                          .Where(c => c.IDDonHang == id)
                                          .Select(c => new
                                          {
                                              TEN = c.SANPHAM.TENSP,
                                              SLuong = c.SOLUONG,
                                              DonGia = c.DONGIA
                                          }).ToList()
                         })
                         .FirstOrDefault();

                if (orderData == null) return HttpNotFound("Đơn hàng không tồn tại.");

                var customer = db.AccountCustomers.FirstOrDefault(u => u.ID == orderData.MAKH);
                var products = db.CHITIETDATHANGs
                                .Where(c => c.IDDonHang == id)
                                .ToList()
                                .Select(c => new
                                {
                                    c.SANPHAM.TENSP,
                                    c.SOLUONG,
                                    c.DONGIA
                                }).ToList();

                if (customer == null) return new HttpStatusCodeResult(500, "Thông tin khách hàng không tồn tại.");
                if (products == null || !products.Any()) return new HttpStatusCodeResult(500, "Không có sản phẩm trong đơn hàng.");

                string filePath = Server.MapPath("~/App_Data/ThongTinDonHang_" + orderData.ID + ".docx");
                string directoryPath = Server.MapPath("~/App_Data");
                if (!Directory.Exists(directoryPath)) return new HttpStatusCodeResult(500, "Thư mục App_Data không tồn tại.");

                try
                {
                    string tempFilePath = Path.Combine(directoryPath, "test_write_permission.txt");
                    System.IO.File.WriteAllText(tempFilePath, "Test write permission");
                    System.IO.File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(500, "Không có quyền ghi vào thư mục App_Data: " + ex.Message);
                }
                try
                {
                    using (var wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                    {
                        var mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.Document = new Document(new Body());
                        Body body = mainPart.Document.Body;

                        var titleParagraph = new Paragraph(
                            new Run(new Text("HÓA ĐƠN MUA HÀNG"))
                            {
                                RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "32" })
                            });
                        titleParagraph.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                        body.Append(titleParagraph);
                        body.Append(new Paragraph(new Run(new Text(""))));

                        AppendTextToBody(body, "Mã Đơn Hàng: " + orderData.ID);
                        AppendTextToBody(body, "Tên Khách Hàng: " + customer.USER_NAME);
                        AppendTextToBody(body, "Điện Thoại: " + customer.PHONE_NUMBER);
                        AppendTextToBody(body, "Ngày Đặt: " + orderData.NgayDat?.ToString("dd/MM/yyyy"));
                        body.Append(new Paragraph(new Run(new Text(""))));
                        body.AppendChild(new Paragraph(new Run(new Text("Danh Sách Sản Phẩm:"))));

                        foreach (var product in products)
                        {
                            AppendTextToBody(body, "Tên Sản Phẩm: " + product.TENSP);
                            AppendTextToBody(body, "Số Lượng: " + product.SOLUONG);
                            AppendTextToBody(body, "Đơn Giá: " + product.DONGIA + " VND");
                            decimal thanhTien = (decimal)(product.SOLUONG * product.DONGIA);
                            AppendTextToBody(body, "Thành Tiền: " + thanhTien + " VND");
                        }

                        var totalAmount = products.Sum(p => p.SOLUONG * p.DONGIA);
                        AppendTextToBody(body, "Tổng Cộng: " + totalAmount + " VND");
                        mainPart.Document.Save();
                        Send(customer.EMAIL, filePath, orderData.ID);

                    }

                    if (!System.IO.File.Exists(filePath)) return HttpNotFound("Tệp không tồn tại.");
                    Send(customer.EMAIL, filePath, orderData.ID);
                    return File(filePath, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"ThongTinDonHang_{orderData.ID}.docx");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi khi tạo tài liệu Word: " + ex.Message);
                    return new HttpStatusCodeResult(500, "Đã có lỗi khi tạo tài liệu: " + ex.Message);
                }
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi xử lý đơn hàng: " + ex.Message);
                return new HttpStatusCodeResult(500, "Đã xảy ra lỗi: " + ex.Message);
            }
        }

        private void AppendTextToBody(Body body, string text)
        {
            var paragraph = new Paragraph(new Run(new Text(text)));
            body.Append(paragraph);
        }

        private void AppendTextToBody(Body body, string text, bool isBold = false)
        {
            Paragraph paragraph = new Paragraph(new Run(new Text(text)));
            if (isBold)
            {
                Run run = paragraph.Elements<Run>().First();
                run.RunProperties = new RunProperties(new Bold());
            }
            body.Append(paragraph);
        }

        private void Send(string email, string attachmentPath, int orderId)
        {
            string username = "phamhuutien004@gmail.com";
            string password = "arih lxoe frtv wfjq";

            try
            {
                string body = $@"
        <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f4;
                        margin: 0;
                        padding: 0;
                        color: #333333;
                    }}
                    .email-container {{
                        width: 90%;
                        max-width: 600px;
                        margin: 20px auto;
                        background-color: #ffffff;
                        border-radius: 8px;
                        padding: 20px;
                        box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
                    }}
                    .header {{
                        text-align: center;
                        font-size: 24px;
                        font-weight: bold;
                        color: #4CAF50;
                        padding-bottom: 10px;
                    }}
                    .content {{
                        font-size: 16px;
                        line-height: 1.6;
                        color: #555555;
                    }}
                    .content p {{
                        margin: 0;
                    }}
                    .footer {{
                        margin-top: 20px;
                        font-size: 14px;
                        color: #777777;
                        text-align: center;
                    }}
                    .footer a {{
                        color: #4CAF50;
                        text-decoration: none;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 10px 20px;
                        font-size: 16px;
                        font-weight: bold;
                        color: #ffffff;
                        background-color: #4CAF50;
                        border-radius: 4px;
                        text-decoration: none;
                        margin-top: 10px;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='header'>Hóa Đơn Mua Hàng</div>
                    <div class='content'>
                        <p>Xin chào,</p>
                        <p>Cảm ơn bạn đã mua hàng. Vui lòng kiểm tra hóa đơn mua hàng của bạn trong tệp đính kèm.</p>
                        <p>Chúng tôi rất mong được phục vụ bạn trong những lần mua hàng tiếp theo!</p>
                        <p> vui lòng chú ý điện thoại của bản trong vào ngày tiếp theo để đảm bảo rằng đơn hàng chắc chắn đến tay của bạn.</p>
                    </div>
                    <div class='footer'>
                        <p>Trân trọng,</p>
                        <p>Nhóm Hỗ Trợ Khách Hàng của chúng tôi</p>
                        <p><a href='https://www.example.com'>Trang chủ</a></p>
                    </div>
                </div>
            </body>
        </html>";

                // Cấu hình email
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(username);
                mail.To.Add(email);
                mail.Subject = "Xác Nhận Đơn Hàng";
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(username, password);
                smtp.EnableSsl = true;

                if (!string.IsNullOrEmpty(attachmentPath) && System.IO.File.Exists(attachmentPath))
                {
                    Attachment attachment = new Attachment(attachmentPath);
                    mail.Attachments.Add(attachment);
                }


                smtp.Credentials = new NetworkCredential(username, password);
                smtp.EnableSsl = true;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gửi email thất bại: " + ex.Message);
            }
        }
        public ActionResult ThongTinCaNhan()
        {
            var user = Session["ID"] as USER;

            if (user == null)
            {
                return RedirectToAction("DangNhap", "User");
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult ThongTinCaNhan(USER update)
        {
            var user = Session["ID"] as USER;

            var dbuser = db.USERS.FirstOrDefault(u => u.ID == user.ID);

            if (dbuser != null)
            {
                dbuser.NAME = update.NAME;
                dbuser.EMAIL = update.EMAIL;
                dbuser.PHONE_NUMBER = update.PHONE_NUMBER;
                dbuser.DiaChi = update.DiaChi;

                db.SaveChanges();
                HttpContext.Cache.Remove("UserCacheKey");
                Session["ID"] = dbuser;

                TempData["thongbao"] = "Cập nhật thông tin thành công!";
                return View(dbuser);
            }
            else
            {
                TempData["thongbao"] = "Không tìm thấy người dùng!";
                return RedirectToAction("ThongTinCaNhan");
            }

        }
    }
}