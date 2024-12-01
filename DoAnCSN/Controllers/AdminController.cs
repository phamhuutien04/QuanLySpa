using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DoAnCSN.Models;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using PagedList;

namespace DoAnCSN.Controllers
{
    public class AdminController : Controller
    {
        private static QLSpaEntities db = new QLSpaEntities();
        private static  int MaKHGlobal = 0;
        private static int MaDVGlobal = 0;

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
                    gioConvert = hour - 12 + " giờ " + minutes + " chiều";
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

        
        private static int tinhTongNgay(string Ngay)
        {
            String[] arr = Ngay.Trim().Split('/');
            int soNgay = int.Parse(arr[0]) + int.Parse(arr[1])*30 + int.Parse(arr[2])*365;
            return soNgay;
        }


        private static int tinhTongThoiGian(string thoiGian)
        {
            String[] arr = thoiGian.Trim().Split(' ');
            int time = 0;
            if(arr.Length == 3) // 9 giờ sáng
            {
                if (arr[2].Equals("sáng"))
                {
                    time = int.Parse(arr[0]) * 60;
                }
                else
                {
                    time = int.Parse(arr[0]) * 12 * 60;
                }
            }

            else // 5 giờ 30 chiều
            {
                if (arr[3].Equals("sáng"))
                {
                    time = int.Parse(arr[0]) * 60 + int.Parse(arr[2]);
                }
                else
                {
                    time = int.Parse(arr[0]) * 12 * 60 + int.Parse(arr[2]);
                }
            }
            return time;
        }

        
        public static string TaoSoHoaDon()
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
        public ActionResult Admin()
        {
            return View();
        }
        // GET: Admin
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
            //c.STATUS = 1;
            //c.CHECKED = 0;
            c.MaTT = 1;
            db.CUSTOMERs.Add(c);
            db.SaveChanges();

            return View();
        }

        [HttpGet]
        public ActionResult SuaTTKhachHang(int MaKH)
        {
            var kh = db.CUSTOMERs.Where(w => w.ID.Equals(MaKH)).FirstOrDefault();
            return View(kh);
        }

        [HttpPost]
        public ActionResult SuaTTKhachHang(FormCollection f)
        {
            string TenKH = f["TenKH"];
            string NgaySinh = f["NgaySinh"];
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

        [HttpGet]
        public ActionResult QuanLiDangKiTuVan(int? MaTT)
        {
            int iMaTT = (MaTT ?? 1);
            ViewBag.CurrentMaTT = iMaTT;

            ViewBag.khs = (from c in db.CUSTOMERs
                           join tt in db.TRANGTHAIs on c.MaTT equals tt.ID
                           where c.MaTT.Equals(iMaTT)
                           orderby c.ID descending
                           select new CustomerModel
                           {
                               ID = c.ID,
                               HO_TEN = c.HO_TEN,
                               NGAY_SINH = c.NGAY_SINH,
                               PHONE_NUMBER = c.PHONE_NUMBER,
                               NGAY_TU_VAN = c.NGAY_TU_VAN,
                               GIO = c.GIO,
                               TEN_TRANG_THAI = tt.Ten_Trang_Thai
                           }).ToList();
            return View();
        }


        [HttpGet]
        public JsonResult DVKH(int MaKH)
        {
            var dvkh = (from dsdvkh in db.DanhSachDVKHs
                        join dv in db.DICHVUs on dsdvkh.MaDV equals dv.ID
                        where dsdvkh.MaKH.Equals(MaKH)
                        select new
                        {
                            dsdvkh.MaKH,
                            dsdvkh.MaDV,
                            dv.TEN_DICH_VU,
                            dv.SO_NGAY_THUC_HIEN,
                            giaTien = dv.SO_NGAY_THUC_HIEN * dv.GIA_DICH_VU,
                            TINH_TRANG_THANH_TOAN = dsdvkh.DaThanhToan == 1 ? "Đã thanh toán" :
                            dsdvkh.THANHTOANBANG == 1 ? "Thanh toán online" : "Thanh toán tiền mặt"

                        }).ToList();
            return Json(dvkh, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AssignEmployee(int MaKH)
        {
            var employees = (from u in db.USERS
                             join ur in db.USER_ROLE on u.ID equals ur.USER_ID
                             where ur.ROLE_ID == 2
                             select new
                             {
                                 u.ID,
                                 u.NAME,
                                 Assigned = db.AssignmentCustomers.Any(a => a.MaKH == MaKH && a.MaNV == u.ID)
                             }).ToList();

            return Json(employees, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AssignEmployee(int customerId, int employeeId)
        {
            var khs = db.AssignmentCustomers
                        .Where(w => w.MaKH.Equals(customerId)).SingleOrDefault();

            List<CUSTOMER> list = (from c in db.CUSTOMERs
                                   join assign in db.AssignmentCustomers on c.ID equals assign.MaKH
                                   where c.MaTT.Equals(2) && assign.MaNV.Equals(employeeId)
                                   select c).ToList(); //lấy danh sách khách hàng chờ tư vấn của nhân viên đó
            if (khs != null)
            {
                if(list !=  null)
                {
                    //lấy ngày giờ của khách hàng chuẩn bị phân công
                    CUSTOMER customer = db.CUSTOMERs.Where(w => w.ID.Equals(customerId)).SingleOrDefault();
                    foreach (CUSTOMER item in list)
                    {
                        // kiểm tra trùng lịch
                        if ((tinhTongNgay(item.NGAY_TU_VAN) == tinhTongNgay(customer.NGAY_TU_VAN))
                            && (tinhTongThoiGian(customer.GIO) >= tinhTongThoiGian(item.GIO)) && tinhTongThoiGian(customer.GIO) <= (tinhTongThoiGian(item.GIO)) + 15)
                        {
                            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                
                AssignmentCustomer assign = db.AssignmentCustomers.Where(w => w.MaKH.Equals(customerId)).SingleOrDefault();
                assign.MaKH = customerId;
                assign.MaNV = employeeId;
                db.SaveChanges();
            }

            else
            {
                if (list != null)
                {
                    //lấy ngày giờ của khách hàng chuẩn bị phân công
                    CUSTOMER customer = db.CUSTOMERs.Where(w => w.ID.Equals(customerId)).SingleOrDefault();
                    foreach (CUSTOMER item in list)
                    {
                        // kiểm tra trùng lịch
                        if ((tinhTongNgay(item.NGAY_TU_VAN) == tinhTongNgay(customer.NGAY_TU_VAN))
                            && (tinhTongThoiGian(customer.GIO) >= tinhTongThoiGian(item.GIO)) && tinhTongThoiGian(customer.GIO) <= (tinhTongThoiGian(item.GIO)) + 15)
                        {
                            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                AssignmentCustomer assign = new AssignmentCustomer();
                assign.MaKH = customerId;
                assign.MaNV = employeeId;
                db.AssignmentCustomers.Add(assign);
                db.SaveChanges();

                CUSTOMER c = db.CUSTOMERs.Where(w => w.ID.Equals(customerId)).SingleOrDefault();
                c.MaTT = 2;
                db.SaveChanges();

            }
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DangKiDV(int MaKH)
        {
            
            CUSTOMER c = db.CUSTOMERs.Where(w => w.ID == MaKH).SingleOrDefault();
            ViewBag.listDVKH = (from dsdvkh in db.DanhSachDVKHs
                              join dv in db.DICHVUs on dsdvkh.MaDV equals dv.ID
                              where dsdvkh.MaKH.Equals(MaKH)
                              select new ViewBagModel
                              {
                                  TEN_DICH_VU = dv.TEN_DICH_VU,
                                  SO_NGAY_THUC_HIEN = dsdvkh.SoNgayThucHien,
                                  TONG_TIEN = dsdvkh.TongTien
                              }).ToList();
            ViewBag.listDV = db.DICHVUs.ToList();
            
            return View(c);
        }

        
        [HttpPost]
        public ActionResult DangKiDV(FormCollection f)
        {
            int MaKH = int.Parse(f["MaKH"]);
            int MaDV = int.Parse(f["MaDV"]);
            int SoNgayThucHien = int.Parse(f["SoNgay"]);
            string TongTien = f["GiaTien"];
            int ThanhToanBang = int.Parse(f["ThanhToanBang"]);
            ViewBag.CurrectTT = ThanhToanBang;

            DanhSachDVKH dsdvkh = new DanhSachDVKH();
            dsdvkh.MaKH = MaKH;
            dsdvkh.MaDV = MaDV;
            dsdvkh.SoNgayThucHien = SoNgayThucHien;
            dsdvkh.SoNgayConLai = SoNgayThucHien;
            dsdvkh.TongTien = TongTien;
            dsdvkh.Status = 0;
            dsdvkh.THANHTOANBANG = ThanhToanBang;

            db.DanhSachDVKHs.Add(dsdvkh);
            db.SaveChanges();

            CUSTOMER c = db.CUSTOMERs.Where(w => w.ID.Equals(MaKH)).SingleOrDefault();
            c.MaTT = 4;
            db.SaveChanges();

            return RedirectToAction("DangKiDV", "Admin", new {MaKH = MaKH});
        }
        public ActionResult ThucHienDV(string search = null)
        {
            if(!string.IsNullOrEmpty(search))
            {
                ViewBag.listDVKH = from c in db.CUSTOMERs
                                   join dsdvkh in db.DanhSachDVKHs on c.ID equals dsdvkh.MaKH
                                   group dsdvkh by new { dsdvkh.MaKH, c.HO_TEN, c.NGAY_SINH, c.PHONE_NUMBER } into g
                                   where g.All(x => x.DaThanhToan == 1) && g.Key.HO_TEN.Contains(search)
                                   select new ViewBagModel
                                   {
                                       MaKH = g.Key.MaKH,
                                       HO_TEN = g.Key.HO_TEN,
                                       NGAY_SINH = g.Key.NGAY_SINH,
                                       PHONE_NUMBER = g.Key.PHONE_NUMBER,
                                       SoLuongDV = g.Count(),
                                       ConLai = g.Count(x => x.Status == 0)
                                   };
            }

            else
            {
                ViewBag.listDVKH = from c in db.CUSTOMERs
                                   join dsdvkh in db.DanhSachDVKHs on c.ID equals dsdvkh.MaKH
                                   group dsdvkh by new { dsdvkh.MaKH, c.HO_TEN, c.NGAY_SINH, c.PHONE_NUMBER } into g
                                   where g.All(x => x.DaThanhToan == 1)
                                   select new ViewBagModel
                                   {
                                       MaKH = g.Key.MaKH,
                                       HO_TEN = g.Key.HO_TEN,
                                       NGAY_SINH = g.Key.NGAY_SINH,
                                       PHONE_NUMBER = g.Key.PHONE_NUMBER,
                                       SoLuongDV = g.Count(),
                                       ConLai = g.Count(x => x.Status == 0)
                                   };
            }

            return View();
        }

        public ActionResult ChiTietDichVuKH(int MaKH)
        {
            var kh = db.CUSTOMERs.Where(w => w.ID.Equals(MaKH)).SingleOrDefault();
            ViewBag.listDVKH = from dsdvkh in db.DanhSachDVKHs
                               join dv in db.DICHVUs on dsdvkh.MaDV equals dv.ID
                               where dsdvkh.MaKH.Equals(MaKH)
                               select new ViewBagModel
                               {
                                   MaKH = dsdvkh.MaKH,
                                   MaDV = dsdvkh.MaDV,
                                   TEN_DICH_VU = dv.TEN_DICH_VU,
                                   SO_NGAY_THUC_HIEN = dsdvkh.SoNgayThucHien,
                                   SO_NGAY_CON_LAI = dsdvkh.SoNgayConLai
                               };
            return View(kh);
        }

        [HttpGet]
        public ActionResult ChiTietDichVu(int MaKH, int MaDV)
        {
            MaKHGlobal = MaKH;
            MaDVGlobal = MaDV;
            ViewBag.TEN_DICH_VU = db.DICHVUs.Where(w => w.ID.Equals(MaDV)).Select(s => s.TEN_DICH_VU).FirstOrDefault();
            ViewBag.ListNV = (from u in db.USERS
                              join ur in db.USER_ROLE on u.ID equals ur.USER_ID
                              where ur.ROLE_ID == 3
                              select u)
                              .ToList();
            ViewBag.So_Buoi_Thuc_Hien = (from dv in db.DICHVUs
                                         join dsdvkh in db.DanhSachDVKHs on dv.ID equals dsdvkh.MaDV
                                         where dsdvkh.MaDV.Equals(MaDV) && dsdvkh.MaKH.Equals(MaKH)
                                         select dv.SO_NGAY_THUC_HIEN).SingleOrDefault();
            //var listDV = db.ChiTietDVKHs.Where(w => w.MaDV.Equals(MaDV) && w.MaKH.Equals(MaKH)).ToList();                            
            ViewBag.listDV = (from chitiet in db.ChiTietDVKHs
                              join u in db.USERS on chitiet.MaNV equals u.ID
                              where chitiet.MaKH.Equals(MaKH) && chitiet.MaDV.Equals(MaDV)
                              select new ViewBagModel
                              {
                                  HO_TEN = u.NAME,
                                  TEN_QUY_TRINH = chitiet.TenQuyTrinh,
                                  Buoi = chitiet.Buoi,
                                  THOI_GIAN_TU = chitiet.ThoiGianTu,
                                  THOI_GIAN_DEN = chitiet.ThoiGianDen,
                                  NGAY_THUC_HIEN = chitiet.NgayThucHien
                              }).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult ChiTietDichVu(FormCollection f)
        {
            //int ok = 1;

            int MaNV = int.Parse(f["MaNV"]);
            ChiTietDVKH chitiet = new ChiTietDVKH();
            chitiet.MaKH = MaKHGlobal;
            chitiet.MaDV = MaDVGlobal;
            chitiet.MaNV = MaNV;
            chitiet.TenQuyTrinh = f["TEN_QUY_TRINH"];
            chitiet.Buoi = int.Parse(f["BUOI"]);
            chitiet.ThoiGianTu = convertGio(f["THOI_GIAN_TU"]);
            chitiet.ThoiGianDen = convertGio(f["THOI_GIAN_DEN"]);
            chitiet.NgayThucHien = convertNgay(f["NGAY_THUC_HIEN"]);

            List<ChiTietDVKH> list = db.ChiTietDVKHs
                                       .Where(w => w.MaNV.Equals(MaNV)
                                                && w.MaKH.Equals(MaKHGlobal)
                                                && w.Checked.Equals(0))
                                       .ToList();

            foreach(var item in list)
            {
                if(tinhTongNgay(item.NgayThucHien) == tinhTongNgay(chitiet.NgayThucHien))
                {
                    if(
                        (
                            (tinhTongThoiGian(chitiet.ThoiGianTu) >= tinhTongThoiGian(item.ThoiGianTu)) && (tinhTongThoiGian(chitiet.ThoiGianTu) <= tinhTongThoiGian(item.ThoiGianDen))
                         || (tinhTongThoiGian(chitiet.ThoiGianDen) >= tinhTongThoiGian(item.ThoiGianTu)) && (tinhTongThoiGian(chitiet.ThoiGianDen) <= tinhTongThoiGian(item.ThoiGianDen))
                        )
                      ) 
                    {
                        TempData["Error"] = "Nhân viên này đã trùng lịch";
                        return RedirectToAction("ChiTietDichVu", "Admin", new { MaKH = MaKHGlobal, MaDV = MaDVGlobal });
                    }

                }
            }

            db.ChiTietDVKHs.Add(chitiet);
            db.SaveChanges();

            



            return RedirectToAction("ChiTietDichVu", "Admin", new { MaKH = MaKHGlobal, MaDV = MaDVGlobal});
        }
        public ActionResult XacNhanThanhToan(int MaKH)
        {
            CUSTOMER c = db.CUSTOMERs.Where(w => w.ID.Equals(MaKH)).SingleOrDefault();
            c.MaTT = 5;
            db.SaveChanges();

            List<DanhSachDVKH> list = db.DanhSachDVKHs
                                        .Where(w => w.MaKH.Equals(MaKH))
                                        .ToList();
            foreach(var item in list)
            {
                item.DaThanhToan = 1;
            }
            db.SaveChanges();

            return RedirectToAction("QuanLiDangKiTuVan", "Admin");
        }

        public ActionResult ThanhToanDV(int MaKH, int MaDV)
        {
            DanhSachDVKH dsdvkh = db.DanhSachDVKHs
                                    .Where(w => w.MaKH == MaKH
                                             && w.MaDV == MaDV)
                                    .SingleOrDefault();
            dsdvkh.DaThanhToan = 1;
            db.SaveChanges();


            List<DanhSachDVKH> list = db.DanhSachDVKHs.Where(w => w.MaKH == MaKH).ToList();
            foreach(var item in list)
            {
                if(item.DaThanhToan == 0)
                {
                    return RedirectToAction("QuanLiDangKiTuVan", "Admin", new {MaTT = 4});
                }
            }

            CUSTOMER c = db.CUSTOMERs.Where(w => w.ID.Equals(MaKH)).SingleOrDefault();
            c.MaTT = 5;
            db.SaveChanges();

            return RedirectToAction("QuanLiDangKiTuVan", "Admin", new { MaTT = 4 });
        }

        public ActionResult ThanhToanDVOnline(int soTien)
        {
            if (Session["SoHoaDon"] == null)
            {
                string soHoaDon = TaoSoHoaDon();
                Session["SoHoaDon"] = soHoaDon;
            }

            string soHoaDonSession = Session["SoHoaDon"].ToString();
            ViewBag.SoHoaDon = soHoaDonSession;
            ViewBag.TongTien = soTien;

            return View();
        }
        public ActionResult LichSuChamSoc(string TenKH = null, int? MaNV = 0, int? MaDV = 0)
        {

            TenKH = TenKH ?? "";
            MaNV = MaNV ?? 0;
            MaDV = MaDV ?? 0;


            var lsChamSoc = (from chiTiet in db.ChiTietDVKHs
                                     join c in db.CUSTOMERs on chiTiet.MaKH equals c.ID
                                     join dv in db.DICHVUs on chiTiet.MaDV equals dv.ID
                                     join u in db.USERS on chiTiet.MaNV equals u.ID
                                     where chiTiet.Checked.Equals(1) 
                                        && (string.IsNullOrEmpty(TenKH) || c.HO_TEN.Contains(TenKH)) 
                                        && (MaNV == 0 || u.ID == MaNV) 
                                        && (MaDV == 0 || dv.ID == MaDV)
                                     select new ViewBagModel
                                     {
                                         TEN_KHACH_HANG = c.HO_TEN,
                                         PHONE_NUMBER = c.PHONE_NUMBER,
                                         TEN_DICH_VU = dv.TEN_DICH_VU,
                                         TEN_QUY_TRINH = chiTiet.TenQuyTrinh,
                                         TEN_NHAN_VIEN = u.NAME,
                                         Buoi = chiTiet.Buoi,
                                         NGAY_THUC_HIEN = chiTiet.NgayThucHien,
                                         HINH_ANH = chiTiet.HINH_ANH
                                     }).ToList();
            ViewBag.lichSuChamSoc = lsChamSoc;

            ViewBag.listDV = db.DICHVUs.ToList();
            ViewBag.ListNV = (from u in db.USERS
                              join ur in db.USER_ROLE on u.ID equals ur.USER_ID
                              where ur.ROLE_ID == 3
                              select u)
                              .ToList();

            return View();
        }

        public ActionResult QuanLiSanPham(int? page)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;
            var products = db.SANPHAMs.OrderBy(p => p.ID).Where(p => p.STATUS == 1)
                                      .Skip((pageNumber - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToList();
            int totalProducts = db.SANPHAMs.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = pageNumber;
            if (pageNumber == 0)
            {
                return RedirectToAction("QuanLiSanPham", new { page = 1 });
            }

            return View(products);
        }

        public ActionResult SuaSanPham(int? page)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;
            var products = db.SANPHAMs.OrderBy(p => p.ID)
                                  .Skip((pageNumber - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();

            int totalProducts = db.SANPHAMs.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = pageNumber;

            return View(products);
        }

        public ActionResult DoanhThuBanHang()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SuaSanPham(List<HttpPostedFileBase> HinhAnhFiles, List<SANPHAM> Products)
        {
            if (ModelState.IsValid && Products != null)
            {
                for (int i = 0; i < Products.Count; i++)
                {
                    var product = Products[i];
                    var existingProduct = db.SANPHAMs.FirstOrDefault(p => p.ID == product.ID);
                    if (existingProduct != null)
                    {
                        // Cập nhật thông tin sản phẩm
                        existingProduct.TENSP = product.TENSP;
                        existingProduct.TENSP = product.TENSP;
                        existingProduct.MOTA_SP = product.MOTA_SP;
                        existingProduct.NAM_SAN_XUAT = product.NAM_SAN_XUAT;
                        existingProduct.GIA = product.GIA;
                        existingProduct.STATUS = product.STATUS;

                        if (HinhAnhFiles != null && HinhAnhFiles[i] != null)
                        {
                            var file = HinhAnhFiles[i];


                            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                            string extension = Path.GetExtension(file.FileName);
                            string newFileName = fileName + "_" + Guid.NewGuid() + extension;

                            string path = Path.Combine(Server.MapPath("~/Image/SanPham/"), newFileName);

                            file.SaveAs(path);

                            existingProduct.HINH_ANH = newFileName;

                        }
                        else
                        {
                            TempData["Message"] = $"thay đổi thành công.";
                        }
                    }
                }

                db.SaveChanges();
                return RedirectToAction("SuaSanPham");
            }

            return View(Products);
        }


        public ActionResult Search(string productName = "", string productType = "", decimal? priceFrom = null, decimal? priceTo = null)
        {
            var results = from s in db.SANPHAMs
                          select s;

            if (!string.IsNullOrEmpty(productName))
            {
                results = results.Where(s => s.TENSP.Contains(productName));
            }

            if (!string.IsNullOrEmpty(productType))
            {
                results = results.Where(s => s.LOAISP == productType);
            }

            if (priceFrom.HasValue)
            {
                results = results.Where(s => s.GIA >= priceFrom.Value);
            }

            if (priceTo.HasValue)
            {
                results = results.Where(s => s.GIA <= priceTo.Value);
            }

            var productList = results.ToList();

            ViewBag.ProductName = productName;
            ViewBag.ProductType = productType;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;

            return View(productList);
        }




        public ActionResult Sidebar_Menu()
        {
            return PartialView("Sidebar_Menu");
        }

        public ActionResult Navbar_Custom()
        {
            return PartialView("Navbar_Custom");
        }

        public ActionResult Footer()
        {
            return PartialView("Footer");
        }

        public ActionResult ThemSanPham()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemSanPham(SANPHAM sanpham, HttpPostedFileBase HinhAnhFiles)
        {
            TempData["Message"] = $"Đã có lỗi xảy ra.";
            if (ModelState.IsValid)
            {
                if (HinhAnhFiles != null)
                {

                    var file = HinhAnhFiles;
                    TempData["Message"] = $"Đã tải lên hình ảnh thành công.";

                    // Tạo tên file mới để tránh trùng lặp
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    string newFileName = fileName + "_" + Guid.NewGuid() + extension;

                    // Đường dẫn lưu file
                    string path = Path.Combine(Server.MapPath("~/Image/SanPham/"), newFileName);

                    // Lưu file lên server
                    file.SaveAs(path);

                    // Lưu tên file mới vào thuộc tính của đối tượng SANPHAM
                    sanpham.HINH_ANH = newFileName;
                }
                else
                {
                    TempData["Message"] = $"Lỗi: Không có hình ảnh được tải lên.";
                }
                sanpham.STATUS = 1;
                db.SANPHAMs.Add(sanpham);
                db.SaveChanges();
                return RedirectToAction("ThemSanPham");
            }

            return View(sanpham);
        }


        [HttpPost]
        public ActionResult Xoa(int id)
        {
            var product = db.SANPHAMs.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            product.STATUS = 0;
            db.SaveChanges();

            return RedirectToAction("QuanLiSanPham", "Admin");
        }

        public ActionResult ImportExcel()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportExcel(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var worksheet = package.Workbook.Worksheets.First();
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var sanPham = new SANPHAM
                            {
                                TENSP = worksheet.Cells[row, 1].Text,
                                LOAISP = worksheet.Cells[row, 2].Text,
                                MOTA_SP = worksheet.Cells[row, 3].Text,
                                GIA = decimal.TryParse(worksheet.Cells[row, 4].Text, out decimal gia) ? gia : 0,
                                NAM_SAN_XUAT = DateTime.TryParse(worksheet.Cells[row, 5].Text, out DateTime namSX) ? namSX : DateTime.MinValue,
                                NAM_HET_HAN = DateTime.TryParse(worksheet.Cells[row, 6].Text, out DateTime namHH) ? namHH : DateTime.MinValue,
                                HINH_ANH = worksheet.Cells[row, 7].Text,
                                SO_LUONG = int.TryParse(worksheet.Cells[row, 8].Text, out int soLuong) ? soLuong : 0,
                                STATUS = int.TryParse(worksheet.Cells[row, 9].Text, out int status) ? (int?)status : null
                            };

                            db.SANPHAMs.Add(sanPham);
                        }

                        db.SaveChanges();
                    }

                    TempData["Message"] = "Nhập file Excel thành công!";
                }
                else
                {
                    TempData["Message"] = "Vui lòng tải lên file Excel hợp lệ (.xls hoặc .xlsx).";
                }
            }
            else
            {
                TempData["Message"] = "Vui lòng chọn file.";
            }

            return RedirectToAction("Index");
        }

        public ActionResult DoanhThuTheoThang()
        {
            var doanhThuTheoThang = db.CHITIETDATHANGs
                .Join(db.DonDatHangs,
                    ct => ct.IDDonHang,
                    dh => dh.ID,
                    (ct, dh) => new { dh.NgayDat, DoanhThu = ct.SOLUONG * ct.DONGIA })
                .GroupBy(x => new
                {
                    Year = System.Data.Entity.DbFunctions.TruncateTime(x.NgayDat).Value.Year,
                    Month = System.Data.Entity.DbFunctions.TruncateTime(x.NgayDat).Value.Month
                })
                .Select(g => new DoanhThu
                {
                    ThoiGian = g.Key.Month + "/" + g.Key.Year,
                    TongDoanhThu = g.Sum(x => (x.DoanhThu ?? 0))
                })
                .OrderBy(x => x.ThoiGian)
                .ToList();
            ViewBag.Labels = doanhThuTheoThang.Select(x => x.ThoiGian).ToList();
            ViewBag.Data = doanhThuTheoThang.Select(x => x.TongDoanhThu).ToList();

            return View();
        }


    }
}
