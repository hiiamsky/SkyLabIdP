namespace SkyLabIdP.Domain.Settings
{
    public class MailSettings
    {
        public string EmailFrom { get; set; } = "";
        public string SmtpHost { get; set; } = "";
        public int SmtpPort { get; set; } = 25;
        public string SmtpUser { get; set; } = "";
        public string SmtpPass { get; set; } = "";
        public string DisplayName { get; set; } = "";

        public bool SmtpUseSSL { get; set; } = false;

        public string PgEmail { get; set; } = "";

        public string AssigneeEmail { get; set; } = "";
    }
}

