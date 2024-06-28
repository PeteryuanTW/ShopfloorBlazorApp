using CommonLibrary;
using ShopfloorBlazorApp.EFModels;

namespace ShopfloorBlazorApp.RuntimeClass
{
    public class StationSingleOrderWithPartNO : StationBase
    {

        public StationSingleOrderWithPartNO(StationConfig StationConfig) : base(StationConfig)
        {
            StationWorkOrderPartDetails = new();
        }
        public string stationName => StationConfig.Name;
        public string workorder { get; set; } = String.Empty;
        public List<StationWorkOrderPartDetail> StationWorkOrderPartDetails { get; set; }
        public bool hasWorkOrder => workorder != String.Empty;
        public List<string> partNOs => StationWorkOrderPartDetails.Select(x => x.PartName).ToList();
        public bool hasPart => partNOs.Count > 0;

        public override void Reset()
        {
            if (stationState == StationState.Pause)
            {
                if (hasRunned)
                {
                    stationState = StationState.Running;
                }
                else
                {
                    errorMsg = String.Empty;
                    stationState = StationState.Uninit;
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
        public override void SetWorkorder(string workorderno)
        {
            if (!hasWorkOrder)
            {
                workorder = workorderno;
            }
            else
            {
                if (stationState != StationState.Uninit)
                {
                    Error("Workorder not match");
                }
                else
                {
                    workorder = workorderno;
                }
            }
        }
        public override void SetTask(StationWorkOrderPartDetail stationWorkOrderPartDetail)
        {
            StationIn(stationWorkOrderPartDetail);
        }
        public override void StationWorkOrderPartDetailUpdate(StationWorkOrderPartDetail stationWorkOrderPartDetail)
        {
            var target = StationWorkOrderPartDetails.FirstOrDefault(x=>x.ID == stationWorkOrderPartDetail.ID);
            if (target != null)
            {
                target = stationWorkOrderPartDetail;
                UpdateUI();
            }
        }
        public override void ClearWorkOrder()
        {
            workorder = String.Empty;
            StationWorkOrderPartDetails = new();
        }
        public override void Run()
        {
            if (!hasWorkOrder)
            {
                Error("workorder is invalid");
                return;
            }
            hasRunned = true;
            stationState = StationState.Running;
        }
        public void StationIn(StationWorkOrderPartDetail stationWorkOrderPartDetail)
        {
            if (stationState == StationState.Running)
            {
                StationWorkOrderPartDetails.Add(stationWorkOrderPartDetail);
            }
        }
        public void StationOut(StationWorkOrderPartDetail stationWorkOrderPartDetail)
        {
            var target = StationWorkOrderPartDetails.FirstOrDefault(x => x.WorkOrderNo == stationWorkOrderPartDetail.WorkOrderNo && x.PartName == stationWorkOrderPartDetail.PartName);
            if (target != null)
            {
                StationWorkOrderPartDetails.Remove(target);
            }
        }
        public override void Pause()
        {
            stationState = StationState.Pause;
        }

        public override void Stop()
        {
            stationState = StationState.Stop;
        }


    }
}
