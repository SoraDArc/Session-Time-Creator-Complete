using System;
using System.Collections.Generic;

namespace STCAPI.Models;

public partial class Lecturer
{
    public int Id { get; set; }

    public string Shortname { get; set; } = null!;

    public string? Surname { get; set; }

    public string? Name { get; set; }

    public string? Patronomyc { get; set; } 

    public int InstitutesId { get; set; }

    public virtual Institute Institutes { get; set; } = null!;
}
