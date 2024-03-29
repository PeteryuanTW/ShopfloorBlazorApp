using ShopfloorBlazorApp.EFModels;

namespace ShopfloorBlazorApp.RuntimeClass
{
    public class StationSingleOrderWithPartNO : StationBase
    {
        
        public StationSingleOrderWithPartNO(StationConfig StationConfig) : base(StationConfig)
        {

        }

        public string partNO = String.Empty;

        public override void Reset()
        {
            ClearWorkOrder();
            stationState = StationState.Uninit;
        }

        public override void Run()
        {
            if (hasWorkOrder)
            {
                Error("workorder or partNO is invalid");
                return;
            }
            stationState = StationState.Running;
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
