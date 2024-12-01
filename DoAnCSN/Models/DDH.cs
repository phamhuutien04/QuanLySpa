using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnCSN.Models
{
    public class DDH
    {
        public int ID { get; set; }
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string Phone { get; set; }
        public int TinhTrangGiaoHang { get; set; }
        public DateTime NgayDat { get; set; }

    }
}