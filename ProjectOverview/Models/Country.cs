using System;
using System.Collections.Generic;

namespace ProjectOverview.Models;

public partial class Country
{
    public int Id { get; set; }

    public string? CommonName { get; set; }

    public string? Capital { get; set; }

    public string? Borders { get; set; }
}
