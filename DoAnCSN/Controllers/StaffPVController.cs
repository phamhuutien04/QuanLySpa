using DoAnCSN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;


namespace DoAnCSN.Controllers
{
    public class StaffPVController : Controller
    {
        QLSpaEntities db = new QLSpaEntities();
        // GET: StaffPV
        
        private static int MaNVGoldbal = 0;


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sidebar_Menu()
        {
            return PartialView("Sidebar_Menu");
        }

        public ActionResult ThoiGianBieuPV(int MaNV)
        {

            var list = (from lst in db.ChiTietDVKHs
                        join dv in db.DICHVUs on lst.MaDV equals dv.ID
                        join c in db.CUSTOMERs on lst.MaKH equals c.ID
                        where lst.MaNV.Equals(MaNV)
                        select new ViewBagModel
                        {
                            MaNV = lst.MaNV,
                            MaDV = lst.MaDV,
                            MaKH = lst.MaKH,
                            HO_TEN = c.HO_TEN,
                            PHONE_NUMBER = c.PHONE_NUMBER,
                            Buoi = lst.Buoi,
                            TEN_DICH_VU = dv.TEN_DICH_VU,
                            TEN_QUY_TRINH = lst.TenQuyTrinh,
                            THOI_GIAN_TU = lst.ThoiGianTu,
                            THOI_GIAN_DEN = lst.ThoiGianTu,
                            NGAY_THUC_HIEN = lst.NgayThucHien,
                            Checked = lst.Checked,
                            HINH_ANH = lst.HINH_ANH
                        }).ToList();
            ViewBag.lstDVKH = list;


            return View();
        }

        public ActionResult LichSuChamSoc(int MaNV)
        {
            //int iMaNV = (MaNV ?? MaNVGoldbal);
            MaNVGoldbal = MaNV;
            //ViewBag.MaNV = iMaNV;

            ViewBag.lichSuChamSoc = (from chiTiet in db.ChiTietDVKHs
                                     join c in db.CUSTOMERs on chiTiet.MaKH equals c.ID
                                     join dv in db.DICHVUs on chiTiet.MaDV equals dv.ID
                                     join u in db.USERS on chiTiet.MaNV equals u.ID
                                     where chiTiet.Checked.Equals(1) && chiTiet.MaNV.Equals(MaNV)
                                     select new ViewBagModel
                                     {
                                         TEN_KHACH_HANG = c.HO_TEN,
                                         PHONE_NUMBER = c.PHONE_NUMBER,
                                         TEN_DICH_VU = dv.TEN_DICH_VU,
                                         TEN_QUY_TRINH = chiTiet.TenQuyTrinh,
                                         TEN_NHAN_VIEN = u.NAME,
                                         Buoi = chiTiet.Buoi,
                                         NGAY_THUC_HIEN = chiTiet.NgayThucHien
                                     }).ToList();
            ViewBag.listDV = db.DICHVUs.ToList();

            return View();
        }

        [HttpPost]
        public ActionResult XacNhanHoanThanh(FormCollection f, HttpPostedFileBase imageFile)
        {
            
            int MaKH = int.Parse(f["MaKH"]);
            int MaDV = int.Parse(f["MaDV"]);
            int MaNV = int.Parse(f["MaNV"]);
            int Buoi = int.Parse(f["Buoi"]);


            if(imageFile == null)
            {
                TempData["Message"] = "Quên upload hình ảnh";
                return RedirectToAction("ThoiGianBieuPV", "StaffPV", new { MaNV = MaNV });
            }
            else
            {
                var file = imageFile;
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                string extension = Path.GetExtension(file.FileName);
                string newFileName = fileName + "_" + Guid.NewGuid() + extension;

                // Đường dẫn lưu file
                string path = Path.Combine(Server.MapPath("~/Image/HoanThanhDV/"), newFileName);
                file.SaveAs(path);

                ChiTietDVKH dvkh = db.ChiTietDVKHs
                         .Where(w => w.MaDV.Equals(MaDV)
                                  && w.MaKH.Equals(MaKH)
                                  && w.MaNV.Equals(MaNV)
                                  && w.Buoi.Equals(Buoi))
                         .SingleOrDefault();
                dvkh.Checked = 1;
                dvkh.HINH_ANH = newFileName;
                db.SaveChanges();

                DanhSachDVKH dsdvkh = db.DanhSachDVKHs
                                    .Where(w => w.MaDV.Equals(MaDV)
                                             && w.MaKH.Equals(MaKH))
                                    .SingleOrDefault();

                dsdvkh.SoNgayConLai -= 1;
                db.SaveChanges();

                if (dsdvkh.SoNgayConLai == 0)
                {
                    dsdvkh.Status = 1;
                    db.SaveChanges();
                }

                return RedirectToAction("ThoiGianBieuPV", "StaffPV", new {MaNV = MaNV}); 
            }


            return View();

             

        }
    }
}