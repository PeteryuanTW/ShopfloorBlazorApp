using Microsoft.Data.SqlClient;
using ShopfloorBlazorApp.EFModels;

namespace ShopfloorBlazorApp.RuntimeClass
{
    public class StationSingleOrderWithoutPartNO : StationBase
    {
        public StationSingleOrderWithoutPartNO(StationConfig StationConfig) : base(StationConfig)
        {
        }
        public int currentAmount => hasWorkOrder ? workOrder.AmountInProcess:0;
        public int OKAmount => hasWorkOrder ? workOrder.Okamount:0;
        public int NGAmount => hasWorkOrder ? workOrder.Ngamount : 0;
        public int targetAmount => hasWorkOrder ? workOrder.TargetAmount:0;

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

        public override void Run()
        {
            if (!hasWorkOrder)
            {
                Error("workorder is null");
                return;
            }
            workOrder.StartTime = DateTime.Now;
            hasRunned = true;
            stationState = StationState.Running;
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
            workOrder.FinishedTime = DateTime.Now;
            hasRunned = false;
            stationState = StationState.Stop;
        }
    }
}
