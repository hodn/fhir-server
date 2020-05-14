namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Patients
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Patients()
        {
            BloodPressureMeasurements = new HashSet<BloodPressureMeasurements>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int patientId { get; set; }

        public long telNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime birthDate { get; set; }

        public byte? sex { get; set; }

        public int cityId { get; set; }

        [Required]
        [StringLength(100)]
        public string address { get; set; }

        [Required]
        [StringLength(13)]
        public string nationalIdentificationNumber { get; set; }

        public int insuranceId { get; set; }

        public int assignedDoctor { get; set; }

        public DateTime? lastVisitDate { get; set; }

        public DateTime? plannedVisitDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BloodPressureMeasurements> BloodPressureMeasurements { get; set; }

        public virtual Cities Cities { get; set; }

        public virtual Insurances Insurances { get; set; }

        public virtual Users Users { get; set; }
    }
}
