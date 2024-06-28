using CommonLibrary;
using Microsoft.Data.SqlClient;
using ShopfloorBlazorApp.EFModels;

namespace ShopfloorBlazorApp.RuntimeClass
{
    public class StationSingleOrderWithoutPartNO : StationBase
    {
        public StationSingleOrderWithoutPartNO(StationConfig StationConfig) : base(StationConfig)
        {
        }
        public string stationName => StationConfig.Name;
        
        public string workorder  = String.Empty;
        public bool hasWorkOrder => workorder != String.Empty;
        public StationWorkOrderPartDetail? stationWorkOrderPartDetail { get; set; }
        public bool hasTask => stationWorkOrderPartDetail != null;
        public int currentAmount => hasTask ? stationWorkOrderPartDetail.WIP : 0;
        public int OKAmount => hasTask ? stationWorkOrderPartDetail.OKAmount : 0;
        public int NGAmount => hasTask ? stationWorkOrderPartDetail.NGAmount : 0;
        public int targetAmount => hasTask ? stationWorkOrderPartDetail.TargetAmount : 0;

        public override void Reset()
        {
            if (stationState == StationState.Pause)
            {
                if (hasRunned)
                {
                    stationState = StationState.Running;
                }

            }
            else if (stationState == StationState.Error)
            {
                if (hasRunned)
                {
                    errorMsg = String.Empty;
                    stationState = StationState.Running;
                }
                else
                {
                    errorMsg = String.Empty;
                    stationState = StationState.Uninit;
                }
            }
            else if (stationState == StationState.Stop)
            {
                ClearWorkOrder();
                errorMsg = String.Empty;
                stationState = StationState.Uninit;
            }
        }
        public override void SetWorkorder(string workorder)
        {
            if (this.workorder == String.Empty)
            {
                this.workorder = workorder;
            }
            else
            {
                Error("Already has workorder");
            }
        }
        public override void SetTask(StationWorkOrderPartDetail stationWorkOrderPartDetail)
        {
            if (!hasWorkOrder)
            {
                SetWorkorder(stationWorkOrderPartDetail.WorkOrderNo);
                this.stationWorkOrderPartDetail = stationWorkOrderPartDetail;
            }
            else
            {
                if (!hasTask)
                {
                    if (stationWorkOrderPartDetail.WorkOrderNo == this.workorder)
                    {
                        this.stationWorkOrderPartDetail = stationWorkOrderPartDetail;
                    }
                }
                else
                {
                    Error($"Already has workorder {this.workorder}, not match new task workorder {this.stationWorkOrderPartDetail.WorkOrderNo}");
                }
            }
        }

        public override void ClearWorkOrder()
        {
            workorder = String.Empty;
            stationWorkOrderPartDetail = null;
        }
        public override void Run()
        {
            if (!hasWorkOrder)
            {
                Error("workorder is null");
                return;
            }
            hasRunned = true;
            stationState = StationState.Running;
        }
        public void StationIn(int amount)
        {
            if (stationState == StationState.Running)
            {
                stationWorkOrderPartDetail.WIP += amount;
            }

        }
        public void StationOut(int ok, int ng)
        {
            if (stationState == StationState.Running)
            {
                stationWorkOrderPartDetail.OKAmount += ok;
                stationWorkOrderPartDetail.NGAmount += ng;
                stationWorkOrderPartDetail.WIP -= (ok + ng);
            }
        }
        public override void Pause()
        {
            stationState = StationState.Pause;
        }

        public override void Stop()
        {
            if (!hasWorkOrder)
            {
                Error("workorder is null");
                return;
            }
            stationWorkOrderPartDetail.FinishedTime = DateTime.Now;
            hasRunned = false;
            stationState = StationState.Stop;
        }

        public override void StationWorkOrderPartDetailUpdate(StationWorkOrderPartDetail stationWorkOrderPartDetail)
        {
            if (hasWorkOrder)
            {
                if (this.stationWorkOrderPartDetail.StationName == stationWorkOrderPartDetail.StationName && this.stationWorkOrderPartDetail.WorkOrderNo == stationWorkOrderPartDetail.WorkOrderNo && this.stationWorkOrderPartDetail.PartName == stationWorkOrderPartDetail.PartName)
                {
                    this.stationWorkOrderPartDetail.TargetAmount = stationWorkOrderPartDetail.TargetAmount;
                    UpdateUI();
                }
            }
        }
    }
}
