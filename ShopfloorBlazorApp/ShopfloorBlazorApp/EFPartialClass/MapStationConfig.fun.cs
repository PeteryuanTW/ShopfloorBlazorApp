using ShopfloorBlazorApp.RuntimeClass;

namespace ShopfloorBlazorApp.EFModels
{
    public partial class MapStationConfig
    {
        private StationBase? _stationBase;
        public StationBase? StationBase => _stationBase;
        public bool stationBindingSuccess => _stationBase != null;
        public void SetStationBase(StationBase stationBase)
        {
            _stationBase = stationBase;
        }
    }
}
