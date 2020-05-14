namespace fhir_integration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BloodPressureMeasurements
    {
        [Key]
        public int measurementId { get; set; }

        public int patientId { get; set; }

        public short sysPressure { get; set; }

        public short diaPressure { get; set; }

        public short heartRate { get; set; }

        public DateTimeOffset measurementTs { get; set; }

        public byte fhirSynced { get; set; }

        public byte isDeleted { get; set; }

        public virtual Patients Patients { get; set; }
    }
}
