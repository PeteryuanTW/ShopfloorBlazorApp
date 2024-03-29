using ShopfloorBlazorApp.EFModels;

namespace ShopfloorBlazorApp.RuntimeClass
{
    public enum StationState
    {
        Uninit,
        Running,
        Pause,
        Error,
        Stop,
    }
    public class StationBase
    {
        public StationConfig StationConfig { get; set; }
        public string name=> StationConfig.Name;
        public int type => StationConfig.Type;
        public string procedureName => StationConfig.ProcessName;
        public WorkOrder? workOrder { get; set; }
        public bool hasWorkOrder => workOrder != null;
        public StationState stationState;

        public string errorMsg = String.Empty;

        public bool hasRunned = false;
        public StationBase(StationConfig StationConfig)
        {
            this.StationConfig = StationConfig;
            stationState = StationState.Uninit;
        }
        public void SetWorkOrder(WorkOrder workOrder)
        {
            this.workOrder = workOrder;
        }
        public void ClearWorkOrder()
        {
            this.workOrder = null;
        }
        public virtual void Reset()
        {
            workOrder = null;
            stationState = StationState.Uninit;
        }
        public virtual void Run()
        {
            if (workOrder != null)
            {
                hasRunned = true;
                stationState = StationState.Running;
            }
        }
        public virtual void Pause()
        {
            stationState = StationState.Pause;
        }
        public virtual void Stop()
        {
            hasRunned = false;
            stationState = StationState.Stop;
        }

        public void Error(string errormsg)
        {
            errorMsg = errormsg;
            stationState = StationState.Error;
        }
    }
}
