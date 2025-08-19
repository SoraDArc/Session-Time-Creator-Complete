using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCUI.Models
{
    internal class ScheduleInformation
    {
        public int AuditoryReserveId { get; set; }
        public ScheduleTemplate ScTemplate { get; set; }
        public override string ToString()
        {
            return ScTemplate?.Name ?? "Без названия";
        }
    }
}
