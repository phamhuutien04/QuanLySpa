//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DoAnCSN.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CUSTOMER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CUSTOMER()
        {
            this.AssignmentCustomers = new HashSet<AssignmentCustomer>();
            this.ChiTietDVKHs = new HashSet<ChiTietDVKH>();
            this.DanhSachDVKHs = new HashSet<DanhSachDVKH>();
        }
    
        public int ID { get; set; }
        public string HO_TEN { get; set; }
        public string NGAY_SINH { get; set; }
        public string PHONE_NUMBER { get; set; }
        public string NGAY_TU_VAN { get; set; }
        public string GIO { get; set; }
        public int MaTT { get; set; }
        public Nullable<int> MaTK { get; set; }
    
        public virtual AccountCustomer AccountCustomer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AssignmentCustomer> AssignmentCustomers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDVKH> ChiTietDVKHs { get; set; }
        public virtual TRANGTHAI TRANGTHAI { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DanhSachDVKH> DanhSachDVKHs { get; set; }
    }
}