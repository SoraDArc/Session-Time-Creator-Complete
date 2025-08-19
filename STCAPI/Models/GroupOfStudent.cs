using System;
using System.Collections.Generic;

namespace STCAPI.Models;

public partial class GroupOfStudent
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int InstitutesId { get; set; }

    public virtual Institute Institutes { get; set; } = null!;
}
