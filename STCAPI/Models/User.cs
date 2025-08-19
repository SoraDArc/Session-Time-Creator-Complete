using System;
using System.Collections.Generic;

namespace STCAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int InstitutesId { get; set; }

    public virtual ICollection<AuditoryReserve> AuditoryReserves { get; set; } = new List<AuditoryReserve>();

    public virtual Institute Institutes { get; set; } = null!;

    public virtual ICollection<ScheduleTemplate> ScheduleTemplates { get; set; } = new List<ScheduleTemplate>();
}
