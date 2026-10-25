using System;
using System.Collections.Generic;

namespace Ochakovo.Entities;

public partial class candidatevacancy
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public int VacancyId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual candidate Candidate { get; set; } = null!;

    public virtual vacancy Vacancy { get; set; } = null!;
}
