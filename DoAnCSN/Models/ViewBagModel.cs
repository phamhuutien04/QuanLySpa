using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnCSN.Models
{
    public class ViewBagModel
    {
        public int MaKH { get; set; }
        public int MaDV { get; set; }
        public int MaNV { get; set; }
        public string HO_TEN { get; set; }
        public string NGAY_SINH { get; set; }
        public string PHONE_NUMBER { get; set; }
        public int SoLuongDV { get; set; }
        public int ConLai { get; set; }

        public string TEN_DICH_VU { get; set; }

        public int SO_NGAY_THUC_HIEN { get; set; }
        public int SO_NGAY_CON_LAI { get; set; }

        public string TONG_TIEN { get; set; }

        public int Buoi { get; set; }

        public string THOI_GIAN_TU { get; set; }
        public string THOI_GIAN_DEN { get; set; }

        public string NGAY_THUC_HIEN { get; set; }

        public string TEN_QUY_TRINH { get; set; }

        public int Checked { get; set; }

        public string TEN_KHACH_HANG {  get; set; }
        public string TEN_NHAN_VIEN { get; set; }

        public string HINH_ANH {  get; set; }

    }
}