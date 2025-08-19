using System;
using System.Collections.Generic;

namespace STCAPI.Models;

public partial class Subject
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int InstitutesId { get; set; }

    public virtual Institute Institutes { get; set; } = null!;
}
