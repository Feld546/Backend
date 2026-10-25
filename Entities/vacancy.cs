using System;
using System.Collections.Generic;

namespace Ochakovo.Entities;

public partial class vacancy
{
    public int VacancyId { get; set; }

    public string Title { get; set; } = null!;

    public string Department { get; set; } = null!;

    public string? Description { get; set; }

    public string? Requirement { get; set; }

    public decimal Salary { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<candidatevacancy> candidatevacancies { get; set; } = new List<candidatevacancy>();
}
