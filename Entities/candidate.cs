using System;
using System.Collections.Generic;

namespace Ochakovo.Entities;

public partial class candidate
{
    public int CandidateId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Gender { get; set; }

    public DateOnly BirthDate { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? EducationType { get; set; }

    public string InstitutionName { get; set; } = null!;

    public string? WorkReadiness { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<candidatevacancy> candidatevacancies { get; set; } = new List<candidatevacancy>();
}
