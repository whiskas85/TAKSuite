namespace TAKSuite.Data.Models
{
    public class TakSettings
    {
        public int Id { get; set; } = 1; // riga singleton

        // Identità client
        public string Callsign  { get; set; } = "TAKSuiteServer";
        public string ClientUid { get; set; } = $"TAKSUITE-{Guid.NewGuid():N}".ToUpper();

        // Connessione server
        public string TakServerIp     { get; set; } = "10.147.19.5";
        public int    HttpsPort        { get; set; } = 8443;
        public int    StreamingPort    { get; set; } = 8089;
        public int    EnrollmentPort   { get; set; } = 8446;

        // Credenziali enrollment
        public string EnrollUser     { get; set; } = "webadmin";
        public string EnrollPassword { get; set; } = "";

        // Certificato
        public string  CertificatePassword { get; set; } = "atakatak";
        public byte[]? CertificateP12       { get; set; }

        public bool HasCertificate => CertificateP12 is { Length: > 0 };

        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
