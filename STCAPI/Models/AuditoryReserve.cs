using System;
using System.Collections.Generic;

namespace STCAPI.Models;

public partial class AuditoryReserve
{
    public int Id { get; set; }

    public string StartDate { get; set; } = null!;

    public string EndDate { get; set; } = null!;

    public int UsersId { get; set; }

    public int? ScheduleTemplatesId { get; set; }

    public virtual ICollection<BusyAuditory> BusyAuditories { get; set; } = new List<BusyAuditory>();

    public virtual ScheduleTemplate? ScheduleTemplates { get; set; }

    public virtual User Users { get; set; } = null!;
}
