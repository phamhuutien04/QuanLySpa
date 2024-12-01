using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnCSN.Models
{
    public class GioHang
    {
        QLSpaEntities db = new QLSpaEntities();
        public int iID { get; set; }
        public String sTENSP { get; set; }
        public String sHINH_ANH { get; set; }
        public Double dGIA { get; set; }
        public int iSoLuong { get; set; }
        public double dThanhTien
        {
            get { return iSoLuong * dGIA; }
        }
        public GioHang(int ms)
        {
            iID = ms;
            SANPHAM s = db.SANPHAMs.Single(n => n.ID == iID);
            sTENSP = s.TENSP;
            sHINH_ANH = s.HINH_ANH;
            dGIA = double.Parse(s.GIA.ToString());
            iSoLuong = 1;
        }
    }
}