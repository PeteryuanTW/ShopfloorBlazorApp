using ShopfloorBlazorApp.EFModels;
using ShopfloorBlazorApp.RuntimeClass;

namespace ShopfloorBlazorApp.Service
{
    public class ShopfloorServiceDataService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private List<StationBase> Stations;
        public List<StationBase> stations => Stations;
        public StationBase GetStationBaseByConfig(StationConfig stationConfigs)
        {
            return Stations.FirstOrDefault(x => x.StationConfig.Name == stationConfigs.Name);
        }
        public ShopfloorServiceDataService(IServiceScopeFactory scopeFactory)
        {
            this._scopeFactory = scopeFactory;
            InitStations();
        }

        private void InitStations()
        {
            Stations = new();
            List<StationConfig> stationConfigs = GetStationConfigs();
            foreach (StationConfig stationConfig in stationConfigs)
            {
                StationBase stationBase;
                switch (stationConfig.Type)
                {
                    case 0:
                        stationBase = new StationSingleOrderWithPartNO(stationConfig);
                        Stations.Add(stationBase);
                        StationChanged(stationBase);
                        break;
                    case 1:
                        stationBase = new StationSingleOrderWithoutPartNO(stationConfig);
                        Stations.Add(stationBase);
                        StationChanged(stationBase);
                        break;
                    case 2:
                        stationBase = new StationMultipleOrdersWithPartNO(stationConfig);
                        Stations.Add(stationBase);
                        StationChanged(stationBase);
                        break;
                    case 3:
                        stationBase = new StationMultipleOrdersWithoutPartNO(stationConfig);
                        Stations.Add(stationBase);
                        StationChanged(stationBase);
                        break;
                    default:
                        break;
                }
            }
        }

        public List<StationConfig> GetStationConfigs()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return context.StationConfigs.ToList();
            }
        }

        public Action<StationBase>? StationChangedAct;
        private void StationChanged(StationBase stationBase) => StationChangedAct?.Invoke(stationBase);

        public List<WorkOrder> GetWorkOrdersNotDoneByProcessName(string processName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return context.WorkOrders.Where(x=>x.ProcessName == processName && x.Status !=3).ToList();
            }
        }
        public async Task WorkOrderStart(string no)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                WorkOrder target = context.WorkOrders.FirstOrDefault(x => x.WorkerOrderNo == no && x.StartTime == null);
                if (target != null)
                {
                    target.Status = 1;
                    target.StartTime = DateTime.Now;
                    await context.SaveChangesAsync();
                }
                else
                {
                    
                }
            }
        }
        public async Task WorkOrderFinished(string no)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                WorkOrder target = context.WorkOrders.FirstOrDefault(x => x.WorkerOrderNo == no && x.FinishedTime == null);
                if (target != null)
                {
                    target.Status = 3;
                    target.FinishedTime = DateTime.Now;
                    await context.SaveChangesAsync();
                }
                else
                {

                }
            }
        }

    }
}
