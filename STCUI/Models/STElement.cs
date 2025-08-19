using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCUI.Models
{
    internal class STElement
    {
        public string Name;
        public string Course;
        public string Group;
        public string Lector;
        public string WishDate;
        public string WishAuditoria;
        public DateTime Consultation { get; set; }
        public DateTime Exam { get; set; }
        public string Auditoria { get; set; }
        public bool HasDate { get; set; }
        public bool Failed = false;
    }
}
