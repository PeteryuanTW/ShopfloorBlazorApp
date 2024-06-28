using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopfloorBlazorApp.EFModels;

public partial class WorkOrder
{
    [Required]
    public string WorkOrderNo { get; set; } = null!;
    [Required]
    public string PartName { get; set; } = null!;
    public bool HasSerialNo { get; set; }
    [Required]
    public string ProcessName { get; set; } = null!;

    public int Okamount { get; set; }

    public int Ngamount { get; set; }

    public int TargetAmount { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    public int Status { get; set; }


    public WorkOrder()
    {

    }
    public WorkOrder(string WorkOrderNo, bool hasSerialNo, string ProcessName, int targetAmount)
    {
        this.WorkOrderNo = WorkOrderNo;
        this.HasSerialNo = hasSerialNo;
        this.ProcessName = ProcessName;
        Okamount = 0;
        Ngamount = 0;
        TargetAmount = targetAmount;
        StartTime = null;
        FinishedTime = null;
        Status = 0;
    }

    public WorkOrder(WorkOrder workorder)
    {
        this.WorkOrderNo = workorder.WorkOrderNo;
        this.HasSerialNo = workorder.HasSerialNo;
        this.ProcessName = workorder.ProcessName;
        this.Okamount = workorder.Okamount;
        this.Ngamount = workorder.Ngamount;
        this.TargetAmount = workorder.TargetAmount;
        this.StartTime = workorder.StartTime;
        this.FinishedTime = workorder.FinishedTime;
        this.Status = workorder.Status;
    }
}
