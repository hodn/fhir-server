namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Doctors
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int doctorId { get; set; }

        [Required]
        [StringLength(20)]
        public string nameTitle { get; set; }

        public int workingPlaceCity { get; set; }

        [Required]
        [StringLength(100)]
        public string workingPlaceAddress { get; set; }

        public long evidenceNumber { get; set; }

        public virtual Cities Cities { get; set; }

        public virtual Users Users { get; set; }
    }
}
