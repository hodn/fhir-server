namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PasswordResetRequests
    {
        [Key]
        public int requestId { get; set; }

        public int userId { get; set; }

        public int tokenCode { get; set; }

        public short attemps { get; set; }

        public DateTime expiration { get; set; }

        public virtual Users Users { get; set; }
    }
}
