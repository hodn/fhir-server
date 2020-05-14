namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AllUserData")]
    public partial class AllUserData
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int userId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string email { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(255)]
        public string password { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        public string firstName { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(20)]
        public string lastName { get; set; }

        [StringLength(200)]
        public string fhirId { get; set; }

        [Key]
        [Column(Order = 5)]
        public byte isDoctor { get; set; }

        [Key]
        [Column(Order = 6)]
        public byte isAdmin { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(50)]
        public string token { get; set; }

        [Key]
        [Column(Order = 8)]
        public DateTime registrationTs { get; set; }

        [Key]
        [Column(Order = 9)]
        public byte isActive { get; set; }

        [Key]
        [Column(Order = 10)]
        public byte isDeleted { get; set; }

        public int? patientId { get; set; }

        public long? telNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime? birthDate { get; set; }

        public byte? sex { get; set; }

        public int? cityId { get; set; }

        [StringLength(100)]
        public string address { get; set; }

        [StringLength(13)]
        public string nationalIdentificationNumber { get; set; }

        public int? insuranceId { get; set; }

        public int? assignedDoctor { get; set; }

        public DateTime? lastVisitDate { get; set; }

        public DateTime? plannedVisitDate { get; set; }

        public int? doctorId { get; set; }

        [StringLength(20)]
        public string nameTitle { get; set; }

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

        [StringLength(3)]
        public string workingPlaceCountry { get; set; }

        [StringLength(50)]
        public string workingPlaceCityName { get; set; }

        [StringLength(11)]
        public string workingPlaceZipCode { get; set; }

        [StringLength(50)]
        public string insuranceName { get; set; }

        [StringLength(3)]
        public string insuranceCode { get; set; }
    }
}
