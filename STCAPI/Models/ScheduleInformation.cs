namespace STCAPI.Models
{
    public class ScheduleInformation
    {
        public int AuditoryReserveId { get; set; }
        public ScheduleTemplate ScTemplate { get; set; }
        public override string ToString()
        {
            return ScTemplate?.Name ?? "Без названия";
        }
    }
}
