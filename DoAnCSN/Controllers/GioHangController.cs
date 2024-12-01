using DoAnCSN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebGrease.Css.Ast.Animation;
using System.Text;
using System.Threading.Tasks;

namespace DoAnCSN.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        QLSpaEntities db = new QLSpaEntities();
        private readonly sepay model = new sepay();

        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang == null)
            {
                // Khởi tạo Giỏ hàng (giỏ hàng chưa tồn tại)
                lstGioHang = new List<GioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }

        public ActionResult ThemGioHang(int ms, string url)
        {
            List<GioHang> lstCart = LayGioHang();
            GioHang sp = lstCart.Find(n => n.iID == ms);
            if (sp == null)
            {
                sp = new GioHang(ms);
                lstCart.Add(sp);
            }
            else
            {
                sp.iSoLuong++;
            }
            return Redirect(url);
        }

        private int TongSoLuong()
        {
            int iTongSoLuong = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongSoLuong = lstGioHang.Sum(n => n.iSoLuong);
            }
            return iTongSoLuong;
        }

        private double TongTien()
        {
            double dTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                dTongTien = lstGioHang.Sum(n => n.dThanhTien);
            }
            return dTongTien;
        }

        public ActionResult GioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();

            if (lstGioHang.Count == 0)
            {
                return RedirectToAction("TrangChu", "Home");
            }

            if (Session["SoHoaDon"] == null)
            {
                string soHoaDon = TaoSoHoaDon();
                Session["SoHoaDon"] = soHoaDon;
            }
            string soHoaDonSession = Session["SoHoaDon"].ToString();

            ViewBag.SoHoaDon = soHoaDonSession;

            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();

            return View(lstGioHang);
        }

        public ActionResult GioHangPartial()
        {
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return PartialView();
        }

        public ActionResult XoaSPKhoiGioHang(int sTENSP)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.SingleOrDefault(n => n.iID == sTENSP);
            if (sp != null)
            {
                lstGioHang.RemoveAll(n => n.iID == sTENSP);
                if (lstGioHang.Count == 0)
                {
                    return RedirectToAction("Product", "PhamHuuTien");
                }
            }
            return RedirectToAction("GioHang");
        }

        public ActionResult CapNhatGioHang(int? iID, string operation)
        {
            if (!iID.HasValue)
            {

                return RedirectToAction("GioHang");
            }

            var cart = Session["GioHang"] as List<GioHang>;
            if (cart == null)
            {
                cart = new List<GioHang>();
            }

            var item = cart.FirstOrDefault(i => i.iID == iID.Value);
            if (item != null)
            {
                if (operation == "increase")
                {
                    item.iSoLuong++;
                }
                else if (operation == "decrease")
                {

                    if (item.iSoLuong > 1)
                    {
                        item.iSoLuong--;
                    }
                }
            }
            Session["GioHang"] = cart;
            return RedirectToAction("GioHang");
        }

        public ActionResult XoaGioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            lstGioHang.Clear();
            return RedirectToAction("PhamHuuTien", "PhamHuuTien");
        }

        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["USER_NAME"] == null || Session["USER_NAME"].ToString() == "")
            {
                return RedirectToAction("DangNhap", "User");

            }

            if (Session["GioHang"] == null)
            {
                return RedirectToAction("PhamHuuTien", "PhamHuuTien");
            }

            List<GioHang> lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return View(lstGioHang);
        }

        [HttpPost]
        public async Task<ActionResult> DatHang(FormCollection f)
        {
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang == null || !lstGioHang.Any())
            {
                return RedirectToAction("GioHang", "GioHang");
            }

            AccountCustomer kh = (AccountCustomer)Session["ID"];
            if (kh == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            string phuongThucThanhToan = f["PhuongThucThanhToan"];
            string soHoaDon = (string)Session["SoHoaDon"];
            DonDatHang ddh = new DonDatHang
            {
                MAKH = kh.ID,
                NgayDat = DateTime.Now,
                SoHoaDon = soHoaDon,
                NgayGiao = DateTime.Now.AddDays(5),
                TinhTrangGiaoHang = 0,
                DaThanhToan = false
            };


            decimal totalAmount = lstGioHang.Sum(item => item.iSoLuong * (decimal)item.dGIA);
            GuiEmailXacNhan(kh.EMAIL, ddh);
            db.DonDatHangs.Add(ddh);
            db.SaveChanges();

            var latestOrderId = ddh.ID;

            foreach (var item in lstGioHang)
            {
                CHITIETDATHANG ctdh = new CHITIETDATHANG
                {
                    MAKH = kh.ID,
                    MASP = item.iID,
                    SOLUONG = item.iSoLuong,
                    DONGIA = (decimal)item.dGIA,
                    IDDonHang = latestOrderId
                };

                db.CHITIETDATHANGs.Add(ctdh);

                var product = db.SANPHAMs.SingleOrDefault(p => p.ID == item.iID);
                if (product != null)
                {
                    product.SO_LUONG -= item.iSoLuong;
                }
            }

            db.SaveChanges();

            Session["GioHang"] = null;
            Session["Xacnhanthanhtoan"] = true;

            return RedirectToAction("XacNhanDonHang", "GioHang");
        }

        public async Task<ActionResult> XacNhanThanhToan(string phuongThucThanhToan, string content, int amount)
        {
            if (phuongThucThanhToan == "online")
            {
                bool result = false;
                var transactionResult = await model.FetchTransactionsAsync();
                var danhsachgiaodich = transactionResult.transactions;
                if (transactionResult != null && transactionResult.status == "200")
                {
                    foreach (var item in danhsachgiaodich)
                    {
                        decimal transactionAmount = item.amount_in;
                        string transactionContentFirst5 = item.transaction_content.ToString().Substring(0, 10);

                        if (transactionContentFirst5.Trim() == content.Trim() && transactionAmount >= amount)
                        {
                            result = true;
                        }
                    }
                    Session["alertPayment"] = result ? "Thành công, đơn hàng đã được thanh toán" : "Thất bại, không tồn tại giao dịch";

                    if (result)
                    {
                        List<GioHang> lstGioHang = LayGioHang();
                        if (lstGioHang == null || !lstGioHang.Any())
                        {
                            return RedirectToAction("GioHang", "GioHang");
                        }
                        AccountCustomer kh = (AccountCustomer)Session["ID"];
                        if (kh == null)
                        {
                            return RedirectToAction("DangNhap", "User");
                        }

                        string soHoaDon = (string)Session["SoHoaDon"];
                        DonDatHang ddh = new DonDatHang
                        {
                            MAKH = kh.ID,
                            NgayDat = DateTime.Now,
                            SoHoaDon = soHoaDon,
                            NgayGiao = DateTime.Now.AddDays(5),
                            TinhTrangGiaoHang = 0,
                            DaThanhToan = true
                        };

                        db.DonDatHangs.Add(ddh);
                        db.SaveChanges();

                        var latestOrderId = ddh.ID;

                        foreach (var item in lstGioHang)
                        {
                            CHITIETDATHANG ctdh = new CHITIETDATHANG
                            {
                                MAKH = kh.ID,
                                MASP = item.iID,
                                SOLUONG = item.iSoLuong,
                                DONGIA = (decimal)item.dGIA,
                                IDDonHang = latestOrderId
                            };

                            db.CHITIETDATHANGs.Add(ctdh);

                            var product = db.SANPHAMs.SingleOrDefault(p => p.ID == item.iID);
                            if (product != null)
                            {
                                product.SO_LUONG -= item.iSoLuong;
                            }
                        }

                        GuiEmailXacNhan(kh.EMAIL, ddh);
                        db.SaveChanges();
                        Session["GioHang"] = null;
                        return RedirectToAction("XacNhanDonHang", "GioHang");
                    }
                }
            }

            return RedirectToAction("GioHang", "GioHang");
        }

        public ActionResult XacNhanDonHang()
        {
            return View();
        }
        public ActionResult ThongTinKhachHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            return View(lstGioHang);
        }
        private void GuiEmailXacNhan(string email, DonDatHang order)
        {
            string username = "phamhuutien004@gmail.com";
            string password = "arih lxoe frtv wfjq";

            try
            {
                string body = $@"
<div style='font-family:Arial, sans-serif; max-width:600px; margin:auto; border:1px solid #ddd; border-radius:8px; overflow:hidden; box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);'>
    <div style='background-color:#4CAF50; padding:25px; text-align:center; color:white;'>
        <h2 style='font-size:28px; margin-bottom:10px;'>Xác nhận đơn hàng</h2>
        <p style='font-size:16px; margin:0;'>Cảm ơn bạn đã đặt hàng tại <b>Spa Thu Hương</b>!</p>
    </div>
    <div style='padding:30px; background-color:#fafafa;'>
        <h3 style='font-size:22px; color:#333; margin-bottom:15px;'>Chi tiết đơn hàng</h3>
        <table style='width:100%; border-collapse:collapse;'>
            <thead>
                <tr style='background-color:#FFB591; color:#555;'>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>STT</th>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>Tên Sản Phẩm</th>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>Số Lượng</th>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>Đơn Giá</th>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>Thành Tiền</th>
                </tr>
            </thead>
            <tbody>";
                int stt = 1;
                foreach (var item in LayGioHang())
                {

                    body += $@"
                <tr style='background-color:#fff; border-bottom:1px solid #eee;'>
                    <td style='padding:10px; text-align:center; color:#333;'>{stt++}</td>
                    <td style='padding:10px; color:#333;'>{item.sTENSP}</td>
                    <td style='padding:10px; text-align:center; color:#333;'>{item.iSoLuong}</td>
                    <td style='padding:10px; text-align:right; color:#333;'>{item.dGIA:#,##0} VNĐ</td>
                    <td style='padding:10px; text-align:right; color:#333;'>{item.dThanhTien:#,##0} VNĐ</td>
                </tr>";
                }

                body += $@"
            </tbody>
        </table>
        <div style='text-align:right; margin-top:20px; font-size:16px; color:#333;'>
            <p><b>Tổng tiền:</b> {TongTien():#,##0} VNĐ</p>
        </div>
        <p style='margin-top:20px; font-size:14px; color:#666;'> Cảm ơn bạn đã mua sản phẩm, nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email: 
            <a href='mailto:phamhuutien004@gmail.com' style='color:#4CAF50; text-decoration:none;'>spathuhuong@student.tdmu.edu.vn</a>.
        </p>
        <p style='margin-top:15px; font-size:14px; color:#666;'>Trân trọng,<br/><b>Đội ngũ Spa Thu Hương</b></p>
    </div>
    <div style='background-color:#4CAF50; padding:15px; text-align:center; color:white;'>
        <p style='margin:0; font-size:14px;'>&copy; 2024 Spa Thu Hương. All rights reserved.</p>
    </div>
</div>";


                // Cấu hình email
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(username);
                mail.To.Add(email);
                mail.Subject = "Xác Nhận Đơn Hàng";
                mail.Body = body;
                mail.IsBodyHtml = true;
                // Cấu hình SMTP
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(username, password);
                smtp.EnableSsl = true;

                // Gửi email
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gửi email thất bại: " + ex.Message);
            }
        }

        public ActionResult XacNhanChuyenTien()
        {
            return View();
        }
        private static Random random = new Random();

        public string TaoSoHoaDon()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string soHoaDon;

            do
            {
                soHoaDon = new string(Enumerable.Repeat(chars, 10)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (db.DonDatHangs.Any(h => h.SoHoaDon == soHoaDon));

            return soHoaDon;
        }
    }
}