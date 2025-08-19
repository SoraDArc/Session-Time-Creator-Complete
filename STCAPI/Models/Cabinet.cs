using System;
using System.Collections.Generic;

namespace STCAPI.Models;

public partial class Cabinet
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int InstitutesId { get; set; }

    public virtual Institute Institutes { get; set; } = null!;
}
