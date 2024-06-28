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
    public abstract class StationBase
    {
        public StationConfig StationConfig { get; set; }
        private List<StationCustomAttribute> _stationCustomAttributes = new();
        public List<StationCustomAttribute> StationCustomAttributes => _stationCustomAttributes;
        public string name => StationConfig.Name;
        public int type => StationConfig.Type;
        public string procedureName => StationConfig.ProcessName;

        public StationState stationState;

        public string errorMsg = String.Empty;

        public bool hasRunned { get; set; } = false;
        public StationBase(StationConfig StationConfig)
        {
            this.StationConfig = StationConfig;
            stationState = StationState.Uninit;
        }

        public void SetCustomAttributes(List<StationCustomAttribute> stationCustomAttributes)
        {
            _stationCustomAttributes = stationCustomAttributes;
        }

        public abstract void SetWorkorder(string workorder);
        public abstract void SetTask(StationWorkOrderPartDetail stationWorkOrderPartDetail);
        public abstract  void StationWorkOrderPartDetailUpdate(StationWorkOrderPartDetail stationWorkOrderPartDetail);
        public abstract  void ClearWorkOrder();
        public abstract void Reset();
        public abstract void Run();
        public abstract void Pause();
        public abstract void Stop();

        public void Error(string errormsg)
        {
            errorMsg = errormsg;
            stationState = StationState.Error;
        }

        public Action? UpdateUIAct;
        public void UpdateUI() => UpdateUIAct?.Invoke();

        public Action? UpdateParameterListsAct;
        public void UpdateParameterLists() => UpdateParameterListsAct?.Invoke();
    }
}
