using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCUI.Models
{
    internal class Lecturer
    {
        public int Id { get; set; }

        public string Shortname { get; set; } = null;
        public string Surname { get; set; } = null;

        public string Name { get; set; } = null;
        public string Patronomyc { get; set; } = null;

        public int InstitutesId { get; set; }

        public virtual Institute Institutes { get; set; } = null;
    }
}
