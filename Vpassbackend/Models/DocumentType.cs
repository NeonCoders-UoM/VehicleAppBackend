namespace Vpassbackend.Models
{
    public enum DocumentType
    {
        VehicleRegistrationCertificate,
        DriversLicense,
        InsuranceCertificate,
        EmissionTestReport,
        TaxReport,
        WarrantyDocument
    }

    public static class DocumentTypeHelper
    {
        public static bool HasExpiration(DocumentType docType)
        {
            return docType != DocumentType.VehicleRegistrationCertificate;
        }
    }
}
