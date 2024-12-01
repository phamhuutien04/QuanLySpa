using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnCSN.Models
{
    public class CustomerModel
    {
        public int ID { get; set; }
        public string HO_TEN { get; set; }
        public string NGAY_SINH { get; set; }
        public string PHONE_NUMBER { get; set; }
        public string NGAY_TU_VAN { get; set; }
        public string GIO { get; set; }

        public string TEN_TRANG_THAI { get; set; }

        public int THANH_TOAN_BANG { get; set; }
    }
}