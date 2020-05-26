namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Users()
        {
            PasswordResetRequests = new HashSet<PasswordResetRequests>();
        }

        [Key]
        public int userId { get; set; }

        [Required]
        [StringLength(50)]
        public string email { get; set; }

        [Required]
        [StringLength(255)]
        public string password { get; set; }

        [Required]
        [StringLength(20)]
        public string firstName { get; set; }

        [Required]
        [StringLength(20)]
        public string lastName { get; set; }

        [StringLength(200)]
        public string fhirId { get; set; }

        public byte isDoctor { get; set; }

        public byte isAdmin { get; set; }

        [StringLength(50)]
        public string token { get; set; }

        public DateTime registrationTs { get; set; }

        public byte isActive { get; set; }

        public byte isDeleted { get; set; }

        public virtual Doctors Doctors { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PasswordResetRequests> PasswordResetRequests { get; set; }

        public virtual Patients Patients { get; set; }
    }
}
