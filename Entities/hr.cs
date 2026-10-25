using System;
using System.Collections.Generic;

namespace Ochakovo.Entities;

public partial class hr
{
    public int HrId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;
}
