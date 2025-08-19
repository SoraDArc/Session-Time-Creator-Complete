using System;
using System.Collections.Generic;

namespace STCAPI.Models;

public partial class ScheduleTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string JsonTemplate { get; set; } = null!;

    public int OwnerUserId { get; set; } 

    public int InstitutesId { get; set; }

    public virtual ICollection<AuditoryReserve> AuditoryReserves { get; set; } = new List<AuditoryReserve>();

    public virtual Institute Institutes { get; set; } = null!;

    public virtual User OwnerUser { get; set; } = null!;
}
