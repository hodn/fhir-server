namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Insurances
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Insurances()
        {
            Patients = new HashSet<Patients>();
        }

        [Key]
        public int insuranceId { get; set; }

        [Required]
        [StringLength(50)]
        public string insuranceName { get; set; }

        [Required]
        [StringLength(3)]
        public string insuranceCode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Patients> Patients { get; set; }
    }
}
