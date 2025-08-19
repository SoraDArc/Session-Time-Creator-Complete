using System.Drawing;

namespace STCAPI.ScheduleAlgo
{
    public class ConditionHandler
    {
        public List<string> ExtractDates(string dates)
        {
            string[] words = dates.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var result = words.ToList();
            for (int i = 0; i < result.Count; i++) 
            {
                result[i]= result[i].Trim(); 
            }
            return result;
        }

        public List<string> ExtractAuditories(string auditories)
        {
            string[] words = auditories.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var result = words.ToList();
            for (int i = 0; i < result.Count; i++)
            {
                result[i] = result[i].Trim();
            }
            return result;
        }
        // Работает с доп. условиями, заданные форматом: "10.06.2024 — 15.06.2024, 24.06.2024 - 29.06.2024" "124a, 120b, 1204"
    }
}
