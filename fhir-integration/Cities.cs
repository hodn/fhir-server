namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Cities
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Cities()
        {
            Doctors = new HashSet<Doctors>();
            Patients = new HashSet<Patients>();
        }

        [Key]
        public int cityId { get; set; }

        [Required]
        [StringLength(3)]
        public string countryCode { get; set; }

        [Required]
        [StringLength(50)]
        public string cityName { get; set; }

        [Required]
        [StringLength(11)]
        public string zipCode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Doctors> Doctors { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Patients> Patients { get; set; }
    }
}
