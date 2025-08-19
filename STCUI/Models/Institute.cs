using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCUI.Models
{
    internal class Institute
    {
        public int Id { get; set; }

        public string Name { get; set; } = null;
        public string ShortName { get; set; } = null;
        public virtual ICollection<Cabinet> Cabinets { get; set; } = new List<Cabinet>();

        public virtual ICollection<GroupOfStudent> GroupOfStudents { get; set; } = new List<GroupOfStudent>();

        public virtual ICollection<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

        public virtual ICollection<ScheduleTemplate> ScheduleTemplates { get; set; } = new List<ScheduleTemplate>();

        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
