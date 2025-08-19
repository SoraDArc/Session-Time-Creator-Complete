namespace STCAPI.Models
{
    public class ReservedInformation
    {
        public int AuditoryReserve_id { get; set; }
        public List<string> Auditories { get; set; } = null!;
        public ReservedInformation(int id, List<string> aud) 
        {
            AuditoryReserve_id = id;
            Auditories = aud;
        }
    }
}
