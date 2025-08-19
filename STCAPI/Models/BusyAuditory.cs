using System;
using System.Collections.Generic;

namespace STCAPI.Models;

public partial class BusyAuditory
{
    public int Id { get; set; }

    public int AuditoryReserveId { get; set; }

    public string Auditory { get; set; } = null!;

    public string? BusyDate { get; set; }

    public string? Subject { get; set; }

    public virtual AuditoryReserve AuditoryReserve { get; set; } = null!;
}
