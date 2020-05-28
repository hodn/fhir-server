namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DoctorView")]
    public partial class DoctorView
    {
        public int? doctorId { get; set; }

        [StringLength(20)]
        public string nameTitle { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string firstName { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string lastName { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(50)]
        public string email { get; set; }

        [StringLength(200)]
        public string fhirId { get; set; }

        public int? workingPlaceCity { get; set; }

        [StringLength(100)]
        public string workingPlaceAddress { get; set; }

        public long? evidenceNumber { get; set; }

        [StringLength(3)]
        public string countryCode { get; set; }

        [StringLength(50)]
        public string cityName { get; set; }

        [StringLength(11)]
        public string zipCode { get; set; }
    }
}
