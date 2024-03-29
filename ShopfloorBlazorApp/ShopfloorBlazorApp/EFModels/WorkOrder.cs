using System;
using System.Collections.Generic;

namespace ShopfloorBlazorApp.EFModels;

public partial class WorkOrder
{
    public string WorkerOrderNo { get; set; } = null!;

    public string ProcessName { get; set; } = null!;

    public int AmountInProcess { get; set; }

    public int Okamount { get; set; }

    public int Ngamount { get; set; }

    public int TargetAmount { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    public int Status { get; set; }
}
