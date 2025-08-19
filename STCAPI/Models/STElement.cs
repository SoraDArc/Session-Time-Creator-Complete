using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace STCAPI.Models
{
    [Serializable]
    public class STElement
    {
        public string Name { get; set; }
        public string Course { get; set; }
        public string Group { get; set; }
        public string Lector { get; set; }
        public string WishDate { get; set; } 
        public string WishAuditoria { get; set; }


        public DateTime Consultation { get; set; }
        public DateTime Exam { get; set; }
        public string Auditoria { get; set; }
        public bool HasDate { get; set; }
        public bool Failed { get; set; }

        public void ToConsole()
        {
            Console.WriteLine($"{Name} {Course} {Group} {Lector} {WishDate} {WishAuditoria}");
        }
        public void ToConsoleWD()
        {
            Console.WriteLine($"{Name} {Course} {Group} {Lector} {WishDate} {WishAuditoria} {Consultation.ToString("dd.MM.yyyy")} {Exam.ToString("dd.MM.yyyy")} {HasDate}");
        }
    }
}
