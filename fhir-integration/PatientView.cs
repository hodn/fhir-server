namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PatientView")]
    public partial class PatientView
    {
        public int? patientId { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string email { get; set; }

        [StringLength(20)]
        public string nameTitle { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string firstName { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(20)]
        public string lastName { get; set; }

        [StringLength(200)]
        public string fhirId { get; set; }

        public long? telNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime? birthDate { get; set; }

        public byte? sex { get; set; }

        [StringLength(100)]
        public string address { get; set; }

        [StringLength(13)]
        public string nationalIdentificationNumber { get; set; }

        public int? assignedDoctor { get; set; }

        public DateTime? lastVisitDate { get; set; }

        public DateTime? plannedVisitDate { get; set; }

        [StringLength(3)]
        public string countryCode { get; set; }

        [StringLength(50)]
        public string cityName { get; set; }

        [StringLength(11)]
        public string zipCode { get; set; }

        [StringLength(50)]
        public string insuranceName { get; set; }

        [StringLength(3)]
        public string insuranceCode { get; set; }
    }
}
