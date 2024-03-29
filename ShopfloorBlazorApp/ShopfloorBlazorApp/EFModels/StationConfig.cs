using System;
using System.Collections.Generic;

namespace ShopfloorBlazorApp.EFModels;

public partial class StationConfig
{
    public string Name { get; set; } = null!;

    public int Type { get; set; }

    public string ProcessName { get; set; } = null!;

    public int ProcessStep { get; set; }
}
