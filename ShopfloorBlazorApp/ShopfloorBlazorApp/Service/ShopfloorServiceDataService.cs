using CommonLibrary;
using CommonLibrary.LangPack;
using CommonLibrary.SignalRPack;
using DevExpress.XtraPrinting.Shape.Native;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShopfloorBlazorApp.Components.Pages;
using ShopfloorBlazorApp.Data;
using ShopfloorBlazorApp.EFModels;
using ShopfloorBlazorApp.RuntimeClass;
using System.Diagnostics.Eventing.Reader;

namespace ShopfloorBlazorApp.Service
{
    public class ShopfloorServiceDataService : ILanguageService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private List<StationBase> Stations;
        public List<StationBase> stations => Stations;

        public ShopfloorServiceDataService(IServiceScopeFactory scopeFactory)
        {
            this._scopeFactory = scopeFactory;
            InitStations();
        }

        #region signalr client
        public Task<string> GetServerConnectionString()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                var connectionConfig = dbContext.SignalRserverConfigs.FirstOrDefault();
                return Task.FromResult($"{ProtocolCodeToString(connectionConfig.Protocol)}://{connectionConfig.Ip}:{connectionConfig.Port}/{connectionConfig.Route}?servicename=ShopfloorServiceTest");
            }
        }
        private string ProtocolCodeToString(int i)
        {
            switch (i)
            {
                case 1:
                    return "http";
                default:
                    return "";
            }
        }
        #endregion

        #region process
        public Task<List<ProcessConfig>> GetProcessConfigs()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.ProcessConfigs.ToList());
            }
        }
        #endregion

        #region station
        private async void InitStations()
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
                        //StationWorkOrderPartDetail? lastTask_0 = RetriveWorkOrderDetailByStationName(stationBase.name, true);
                        //if (lastTask_0 != null)
                        //{
                        //    stationBase.SetWorkOrder(lastTask_0);
                        //    stationBase.Run();
                        //}
                        stationBase.SetCustomAttributes(await GetStationCustomAttributesByName(stationBase.StationConfig.Name));

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

        private Task<List<StationCustomAttribute>> GetStationCustomAttributesByName(string stationName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.StationCustomAttributes.Where(x=>x.StationName == stationName).ToList());
            }
        }

        public Task<RequestResult> SetWorkorderByStationName(string stationName, string wo)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                switch (target.StationConfig.Type)
                {
                    case 0:
                        ((StationSingleOrderWithPartNO)target).SetWorkorder(wo);
                        StationChanged(target);
                        break;
                    case 1:
                        ((StationSingleOrderWithoutPartNO)target).SetWorkorder(wo);
                        StationWorkOrderPartDetail targetTask = GetStationWorkOrderWithoutPartDetail(stationName, wo);
                        ((StationSingleOrderWithoutPartNO)target).SetTask(targetTask);
                        StationChanged(target);
                        break;
                    case 2:
                        StationChanged(target);
                        break;
                    case 3:
                        StationChanged(target);
                        break;
                    default:
                        break;
                }
                return Task.FromResult(new RequestResult(0, $"station {target.name} set workorder {wo} success"));
            }
            else
            {
                return Task.FromResult(new RequestResult(3, $"station {stationName} not found"));
            }
        }
        public async Task<RequestResult> RunStationByName(string stationName)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                target.Run();
                switch (target.StationConfig.Type)
                {
                    case 0:
                        //await StationStartWorkOrder(stationName, ((StationSingleOrderWithPartNO)target).workorder);
                        StationChanged(target);
                        break;
                    case 1:
                        //await StationStartWorkOrder(stationName, ((StationSingleOrderWithoutPartNO)target).workorder);
                        StationChanged(target);
                        break;
                    case 2:
                        StationChanged(target);
                        break;
                    case 3:
                        StationChanged(target);
                        break;
                    default:
                        break;
                }
                return new RequestResult(0, $"run station {target.name} success");
            }
            else
            {
                return new RequestResult(3, $"station {stationName} not found");
            }
        }
        public async Task<RequestResult> LogCustomData(CustomDataParam customDataParam)
        {
            StationBase stationBase = Stations.FirstOrDefault(x => x.name == customDataParam.stationName);
            if (stationBase != null)
            {
                int stationType = stationBase.StationConfig.Type;
                switch (stationType)
                {
                    case 0:
                        StationSingleOrderWithPartNO stationSingleOrderWithPartNO = (StationSingleOrderWithPartNO)stationBase;
                        StationWorkOrderPartDetail? target = stationSingleOrderWithPartNO.StationWorkOrderPartDetails.FirstOrDefault(x => x.SerialNO == customDataParam.serialNo);
                        if (target != null)
                        {
                            bool res = target.SetCustomData(customDataParam.dataType, customDataParam.sequence, customDataParam.val);
                            await UpdateWorkOrderDetailsCustomData(target);
                            if (res)
                            {
                                return new RequestResult(1, $"set custom data of serial {customDataParam.serialNo} in station {customDataParam.stationName} success");

                            }
                            else
                            {
                                return new RequestResult(3, $"set custom data of serial {customDataParam.serialNo} in station {customDataParam.stationName} fail");
                            }
                        }
                        else
                        {
                            return new RequestResult(3, $"serial {customDataParam.serialNo} not found in station {customDataParam.stationName}");
                        }
                    case 1:
                        StationSingleOrderWithoutPartNO stationSingleOrderWithoutPartNO = (StationSingleOrderWithoutPartNO)stationBase;
                        if (stationSingleOrderWithoutPartNO.stationWorkOrderPartDetail.SerialNO == customDataParam.serialNo)
                        {
                            bool res = stationSingleOrderWithoutPartNO.stationWorkOrderPartDetail.SetCustomData(customDataParam.dataType, customDataParam.sequence, customDataParam.val);
                            await UpdateWorkOrderDetailsCustomData(stationSingleOrderWithoutPartNO.stationWorkOrderPartDetail);
                            if (res)
                            {
                                return new RequestResult(1, $"set custom data of serial {customDataParam.serialNo} in station {customDataParam.stationName} success");

                            }
                            else
                            {
                                return new RequestResult(3, $"set custom data of serial {customDataParam.serialNo} in station {customDataParam.stationName} fail");
                            }
                        }
                        else
                        {
                            return new RequestResult(3, $"serial {customDataParam.serialNo} not found in station {customDataParam.stationName}");
                        }
                    case 2:
                    case 3:
                    default:
                        return new RequestResult(3, $"station {customDataParam.stationName} type {stationType} not implement yet");
                }
            }
            else
            {
                return new RequestResult(3, $"station {customDataParam.stationName} not found");
            }
        }
        public async Task<RequestResult> PauseStationByName(string stationName)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                target.Pause();
                StationChanged(target);
                return new RequestResult(0, $"Pause station {target.name} success");
            }
            else
            {
                return new RequestResult(3, $"station {stationName} not found");
            }
        }
        public async Task<RequestResult> StopStationByName(string stationName, string wo)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                target.Stop();
                await StationFinishWorkOrder(target.name, wo);
                target.UpdateParameterLists();
                StationChanged(target);
                return new RequestResult(0, $"stop station {target.name} success");
            }
            else
            {
                return new RequestResult(3, $"station {stationName} not found");
            }
        }
        public Task<RequestResult> ResetStationByName(string stationName)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                target.Reset();
                target.UpdateParameterLists();
                StationChanged(target);
                return Task.FromResult(new RequestResult(0, $"reset station {target.name} success"));
            }
            else
            {
                return Task.FromResult(new RequestResult(3, $"station {stationName} not found"));
            }
        }
        public Task<RequestResult> SetTaskByStationName(string stationName, StationWorkOrderPartDetail StationWorkOrderPartDetail)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                switch (target.StationConfig.Type)
                {
                    case 0:
                        ((StationSingleOrderWithPartNO)target).SetTask(StationWorkOrderPartDetail);
                        StationChanged(target);
                        break;
                    case 1:
                        ((StationSingleOrderWithoutPartNO)target).SetTask(StationWorkOrderPartDetail);
                        StationChanged(target);
                        break;
                    case 2:
                        StationChanged(target);
                        break;
                    case 3:
                        StationChanged(target);
                        break;
                    default:
                        break;
                }
                return Task.FromResult(new RequestResult(0, $"station {target.name} set task {StationWorkOrderPartDetail.WorkOrderNo} success"));
            }
            else
            {
                return Task.FromResult(new RequestResult(3, $"station {stationName} not found"));
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
        public List<StationConfig> GetStationConfigsByProcess(string processName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return context.StationConfigs.Where(x => x.ProcessName == processName).OrderBy(x => x.ProcessStep).ToList();
            }
        }
        public StationBase GetStationBaseByConfig(StationConfig stationConfigs)
        {
            return Stations.FirstOrDefault(x => x.StationConfig.Name == stationConfigs.Name);
        }
        public StationBase? GetStationBaseByName(string stationName)
        {
            return Stations.FirstOrDefault(x => x.StationConfig.Name == stationName);
        }
        public List<string> GetConsecutiveStationsInSameProcess(string stationName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                List<string> res = new();
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                StationConfig stationConfig = context.StationConfigs.FirstOrDefault(x => x.Name == stationName);
                if (stationConfig != null)
                {
                    string process = stationConfig.ProcessName;
                    int step = stationConfig.ProcessStep;
                    res = context.StationConfigs.Where(x => x.ProcessName == process && x.ProcessStep > step).Select(x => x.Name).ToList();
                }
                return res;
            }
        }
        public async Task BroadcastToAllStationBaseParamChanged()
        {
            List<Task> tasks = new();
            foreach (StationBase station in Stations)
            {
                tasks.Add(Task.Run(() => station.UpdateParameterLists()));
            }
            await Task.WhenAll(tasks);
        }
        public Action<StationBase>? StationChangedAct;
        private void StationChanged(StationBase stationBase) => StationChangedAct?.Invoke(stationBase);
        private void StationChangedByName(string stationName)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                StationChangedAct?.Invoke(target);
            }
        }
        #endregion

        #region Map
        public Task<List<MapConfig>> GetAllMapConfigs()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.MapConfigs.ToList());
            }
        }

        public Task<MapConfig?> GetMapConfigByName(string name)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.MapConfigs.FirstOrDefault(x => x.MapName == name));
            }
        }

        public Task<List<MapStationConfig>> GetMapStationConfigs(string mapName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                List<MapStationConfig> res = context.MapStationConfigs.Where(x => x.MapName == mapName).ToList();
                foreach (MapStationConfig config in res)
                {
                    config.SetStationBase(GetStationBaseByName(config.StationName));
                }
                return Task.FromResult(res);
            }
        }
        #endregion

        #region workorder
        public Action? WorkOrderStatusChangedAct;
        private void WorkOrderStatusChanged() => WorkOrderStatusChangedAct?.Invoke();
        public Task<List<WorkOrder>> GetWorkOrders()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.WorkOrders.ToList());
            }
        }
        public Task<List<WorkOrder>> GetWorkOrdersByStatus(List<int> status)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.WorkOrders.Where(x => status.Contains(x.Status)).ToList());
            }
        }
        public Task<List<WorkOrder>> GetWorkOrdersByProcessAndStatus(string process, List<int> status)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.WorkOrders.Where(x => x.ProcessName == process && status.Contains(x.Status)).ToList());
            }
        }
        public Task<WorkOrder?> GetWorkOrderByNo(string no)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.WorkOrders.FirstOrDefault(x => x.WorkOrderNo == no));
            }
        }

        public async Task WorkorderAddngByno(string no, int ng)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                WorkOrder? wo = context.WorkOrders.FirstOrDefault(x => x.WorkOrderNo == no);
                if (wo != null)
                {
                    wo.Ngamount += ng;
                    await context.SaveChangesAsync();
                }
            }
        }
        public async Task<RequestResult> UpsertWorkOrder(WorkOrder workOrder)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                WorkOrder target = context.WorkOrders.FirstOrDefault(x => x.WorkOrderNo == workOrder.WorkOrderNo);
                if (target != null)
                {
                    //update
                    if (target.Status != 0)
                    {
                        return new RequestResult(3, $"Workorder {workOrder.WorkOrderNo} already started");
                    }
                    else
                    {
                        //switch haspartno attribute
                        //if (target.HasPartNo != workOrder.HasPartNo)
                        //{
                        //    if (!workOrder.HasPartNo)
                        //    {
                        //        if (await GetWorkorderHasPart(workOrder.WorkOrderNo))
                        //        {
                        //            return new RequestResult(3, $"remove all part in workorder {workOrder.WorkOrderNo} first");
                        //        }
                        //    }
                        //    await DeleteStationWorkOrderPartDetailByWorkorder(workOrder.WorkOrderNo, target.HasPartNo);
                        //    if (!workOrder.HasPartNo)
                        //    {
                        //        await InsertStationWorkOrderPartDetails(await GenerateStationWorkOrderPartDetails(workOrder.WorkOrderNo));
                        //    }
                        //    target.HasPartNo = workOrder.HasPartNo;
                        //}
                        target.HasSerialNo = workOrder.HasSerialNo;
                        target.ProcessName = workOrder.ProcessName;
                        target.TargetAmount = workOrder.TargetAmount;
                    }
                }
                else
                {
                    //insert
                    WorkOrder tmp = new WorkOrder(workOrder.WorkOrderNo, workOrder.HasSerialNo, workOrder.ProcessName, workOrder.TargetAmount);
                    await context.AddAsync(tmp);
                    await context.SaveChangesAsync();
                    if (!tmp.HasSerialNo)
                    {
                        await InsertStationWorkOrderPartDetails(await GenerateStationWorkOrderPartDetails(tmp.WorkOrderNo));
                    }
                }
                await context.SaveChangesAsync();
                return new RequestResult(1, $"upsert workorder {workOrder.WorkOrderNo} seccess");
            }
        }
        public async Task<RequestResult> DeleteWorkOrder(WorkOrder workOrder)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                WorkOrder target = context.WorkOrders.FirstOrDefault(x => x.WorkOrderNo == workOrder.WorkOrderNo);
                if (target != null)
                {
                    //find
                    if (target.Status != 0)
                    {
                        return new RequestResult(3, $"Workorder {workOrder.WorkOrderNo} already started");
                    }
                    else
                    {
                        context.Remove(target);

                        List<StationConfig> stationsInProcess = GetStationConfigsByProcess(workOrder.ProcessName);
                        foreach (StationConfig stationConfig in stationsInProcess)
                        {
                            await DeleteStationWorkOrderPartDetailsByStationAndWorkorder(stationConfig.Name, workOrder.WorkOrderNo);
                        }
                        await context.SaveChangesAsync();
                        return new RequestResult(3, $"delete workorder {workOrder.WorkOrderNo} seccess");
                    }
                }
                else
                {
                    //not found
                    return new RequestResult(3, $"workorder {workOrder.WorkOrderNo} not found");
                }
            }
        }

        #endregion

        #region workorder part
        public Task<List<WorkorderPart>> GetWorkorderPartsByWorkorderNo(string wo)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.WorkorderParts.Where(x => x.WorkorderNo == wo).ToList());
            }
        }
        public Task<bool> GetWorkorderHasPart(string wo)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.WorkorderParts.Any(x => x.WorkorderNo == wo));
            }
        }
        public async Task<RequestResult> InsertWorkorderPart(WorkorderPart workorderPart)
        {
            WorkOrder? targetOrder = await GetWorkOrderByNo(workorderPart.WorkorderNo);
            if (targetOrder == null)
            {
                return new RequestResult(3, $" Workorder {workorderPart.WorkorderNo} not found");
            }
            else
            {
                if (targetOrder.Status != 0)
                {
                    return new RequestResult(3, $" Workorder {workorderPart.WorkorderNo} already start");
                }
                else
                {
                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                            await context.WorkorderParts.AddAsync(workorderPart);
                            RequestResult res = await InsertStationWorkOrderPartDetails(await GenerateStationWorkOrderPartDetails(workorderPart.WorkorderNo, workorderPart.PartNo));
                            if (res.success == 3)
                            {
                                return res;
                            }
                            await context.SaveChangesAsync();
                        }
                        return new RequestResult(1, $"Add {workorderPart.PartNo} to {workorderPart.WorkorderNo} success");
                    }
                    catch (Exception e)
                    {
                        return new RequestResult(3, $"Add {workorderPart.PartNo} to {workorderPart.WorkorderNo} fail({e})");
                    }

                }
            }
        }

        public async Task<RequestResult> DeleteWorkorderPart(WorkorderPart workorderPart)
        {
            List<StationWorkOrderPartDetail> res = await GetStationWorkOrderPartDetailByWorkorderAndPartNo(workorderPart);
            if (res.Exists(x => x.Status != 0))
            {
                return new RequestResult(3, $" {workorderPart.PartNo} in {workorderPart.WorkorderNo} already start");
            }
            else
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                    WorkorderPart target = context.WorkorderParts.FirstOrDefault(x => x.WorkorderNo == workorderPart.WorkorderNo && x.PartNo == workorderPart.PartNo);
                    if (target != null)
                    {
                        context.WorkorderParts.Remove(target);
                        await context.SaveChangesAsync();
                    }
                }
                await DeleteStationWorkOrderPartDetailsByWorkorderAndPartNo(workorderPart);
                return new RequestResult(1, $"remove {workorderPart.PartNo} in {workorderPart.WorkorderNo} success");
            }
        }

        #endregion

        #region workorder part station details
        public Action? WorkOrderDetailsAmountChangedAct;
        private void WorkOrderDetailsAmountChanged() => WorkOrderDetailsAmountChangedAct?.Invoke();
        public Action<StationWorkOrderPartDetail>? SingleWorkOrderDetailsAmountChangedAct;
        private void SingleWorkOrderDetailsAmountChanged(StationWorkOrderPartDetail stationWorkOrderPartDetail) => SingleWorkOrderDetailsAmountChangedAct?.Invoke(stationWorkOrderPartDetail);
        public List<StationWorkOrderPartDetail> GetStationWorkOrderPartDetail()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return context.StationWorkOrderPartDetails.ToList();
            }
        }
        public StationWorkOrderPartDetail GetStationWorkOrderWithoutPartDetail(string station, string wo)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return context.StationWorkOrderPartDetails.FirstOrDefault(x => x.StationName == station && x.WorkOrderNo == wo);
            }
        }
        public Task<List<StationWorkOrderPartDetail>> GetStationWorkOrderPartDetailByWorkorder(string wo)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.StationWorkOrderPartDetails.Where(x => x.WorkOrderNo == wo).ToList());
            }
        }
        public Task<List<StationWorkOrderPartDetail>> GetStationWorkOrderPartDetailFinishedByWorkorder(string wo)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.StationWorkOrderPartDetails.Where(x => x.WorkOrderNo == wo && x.Status == 3).ToList());
            }
        }
        public async Task DeleteStationWorkOrderPartDetailByWorkorder(string wo, bool haspart)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                var tmp = context.StationWorkOrderPartDetails.Where(x => x.WorkOrderNo == wo);
                IQueryable<StationWorkOrderPartDetail> target;
                if (haspart)
                {
                    target = tmp.Where(x => x.PartName != "");
                }
                else
                {
                    target = tmp.Where(x => x.PartName == "");
                }
                context.StationWorkOrderPartDetails.RemoveRange(target);
                await context.SaveChangesAsync();
            }
        }
        public Task<List<StationWorkOrderPartDetail>> GetStationWorkOrderPartDetailByWorkorderAndPartNo(WorkorderPart workorderPart)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.StationWorkOrderPartDetails.Where(x => x.WorkOrderNo == workorderPart.WorkorderNo && x.PartName == workorderPart.PartNo).ToList());
            }
        }
        public async Task<List<StationWorkOrderPartDetail>> GenerateStationWorkOrderPartDetails(string workorderno)
        {
            List<StationWorkOrderPartDetail> res = new();
            WorkOrder? workorder = await GetWorkOrderByNo(workorderno);
            if (workorder != null)
            {
                if (!workorder.HasSerialNo)
                {
                    List<StationConfig> stationsInProcess = GetStationConfigsByProcess(workorder.ProcessName);
                    foreach (StationConfig stationConfig in stationsInProcess)
                    {
                        res.Add(new StationWorkOrderPartDetail()
                        {
                            StationName = stationConfig.Name,
                            WorkOrderNo = workorder.WorkOrderNo,
                            PartName = "",
                            WIP = 0,
                            TargetAmount = workorder.TargetAmount,
                            OKAmount = 0,
                            NGAmount = 0,
                            StartTime = null,
                            FinishedTime = null,
                            Status = 0,
                        });
                    }
                }

            }
            return res;
        }
        public async Task<List<StationWorkOrderPartDetail>> GenerateStationWorkOrderPartDetails(string workorderno, string partno)
        {
            List<StationWorkOrderPartDetail> res = new();
            WorkOrder? workorder = await GetWorkOrderByNo(workorderno);
            if (workorder != null)
            {
                if (workorder.HasSerialNo)
                {
                    List<StationConfig> stationsInProcess = GetStationConfigsByProcess(workorder.ProcessName);
                    foreach (StationConfig stationConfig in stationsInProcess)
                    {
                        res.Add(new StationWorkOrderPartDetail()
                        {
                            StationName = stationConfig.Name,
                            WorkOrderNo = workorder.WorkOrderNo,
                            PartName = partno,
                            WIP = 0,
                            TargetAmount = 1,
                            OKAmount = 0,
                            NGAmount = 0,
                            StartTime = null,
                            FinishedTime = null,
                            Status = 0,
                        });
                    }
                }

            }
            return res;
        }
        public async Task<RequestResult> InsertStationWorkOrderPartDetails(List<StationWorkOrderPartDetail> stationWorkOrderPartDetails)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                    await context.StationWorkOrderPartDetails.AddRangeAsync(stationWorkOrderPartDetails);
                    await context.SaveChangesAsync();
                }
                return new RequestResult(1, "Insert workorder Part Detail success");
            }
            catch (Exception e)
            {
                return new RequestResult(3, $"Insert workorder Part Detail fail({e.Message})");
            }

        }
        public async Task DeleteStationWorkOrderPartDetailsByStationAndWorkorder(string stationName, string workorderno)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                context.StationWorkOrderPartDetails.RemoveRange(context.StationWorkOrderPartDetails.Where(x => x.StationName == stationName && x.WorkOrderNo == workorderno));
                await context.SaveChangesAsync();
            }
        }
        public async Task DeleteStationWorkOrderPartDetailsByWorkorderAndPartNo(WorkorderPart workorderPart)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                var target = context.StationWorkOrderPartDetails.Where(x => x.WorkOrderNo == workorderPart.WorkorderNo && x.PartName == workorderPart.PartNo);
                context.StationWorkOrderPartDetails.RemoveRange(target);
                await context.SaveChangesAsync();
            }
        }
        public async Task<List<StationWorkOrderPartDetail>> GetStatusStartedWorkOrderDetailsByStationName(string stationName, List<int> status)
        {
            string process = GetStationBaseByName(stationName).StationConfig.ProcessName;
            List<string> startedWorkorders = (await GetWorkOrdersByProcessAndStatus(process, new List<int> { 1 })).Select(x => x.WorkOrderNo).ToList();
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return context.StationWorkOrderPartDetails.Where(x => startedWorkorders.Contains(x.WorkOrderNo) && x.StationName == stationName && status.Contains(x.Status)).ToList();
            }
        }
        public Task<List<StationWorkOrderPartDetail>> GetStatusWorkOrderDetailsStationAndWorkorder(string stationName, string wo, List<int> status)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.StationWorkOrderPartDetails.Where(x => x.StationName == stationName && x.WorkOrderNo == wo && status.Contains(x.Status)).ToList());
            }
        }
        public Task<StationWorkOrderPartDetail?> GetWorkOrderDetailsStatus(string stationName, string wo, string serialNo)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                StationWorkOrderPartDetail target = context.StationWorkOrderPartDetails.FirstOrDefault(x => x.StationName == stationName && x.WorkOrderNo == wo && x.SerialNO == serialNo);
                return Task.FromResult(target);
            }
        }
        public Task<StationWorkOrderPartDetail?> GetWorkOrderDetailByID(int id)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                StationWorkOrderPartDetail target = context.StationWorkOrderPartDetails.FirstOrDefault(x => x.ID == id);
                return Task.FromResult(target);
            }
        }
        public StationWorkOrderPartDetail? RetriveWorkOrderDetailByStationName(string stationName, bool withPartNO)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                if (withPartNO)
                {
                    return context.StationWorkOrderPartDetails.FirstOrDefault(x => x.StationName == stationName && x.PartName != "" && x.Status != 0 && x.Status != 3);
                }
                else
                {
                    return context.StationWorkOrderPartDetails.FirstOrDefault(x => x.StationName == stationName && x.PartName == "" && x.Status != 0 && x.Status != 3);
                }
            }
        }
        public List<StationWorkOrderPartDetail> RetriveWorkOrderDetailsByStationName(string stationName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return context.StationWorkOrderPartDetails.Where(x => x.StationName == stationName && (x.Status != 3 || x.Status != 0)).ToList();
            }
        }
        public async Task StationFinishWorkOrder(string stationName, string wo)
        {
            DateTime dt = DateTime.Now;
            WorkOrder? target = await GetWorkOrderByNo(wo);
            if (target != null)
            {
                if (!target.HasSerialNo)
                {
                    StationWorkOrderPartDetail? targetDetail = await GetWorkOrderDetailsStatus(stationName, wo, "");
                    if (targetDetail != null)
                    {
                        targetDetail.Finish(dt);
                        await UpdateWorkOrderDetailsTimestamp(targetDetail);
                        StationChangedByName(stationName);
                    }
                    else
                    {

                    }
                }
            }


            //if (await CheckIsLastStation(stationName))
            //{
            //    await WorkOrderFinished(wo, dt);
            //}
        }
        public async Task<RequestResult> StationInByNameAndPart(string stationName, string serialNo)
        {
            StationBase? targetStation = Stations.FirstOrDefault(x => x.name == stationName);
            string workorderNo = ((StationSingleOrderWithPartNO)targetStation).workorder;
            WorkOrder? targetWorkorder = await GetWorkOrderByNo(workorderNo);
            if (targetStation != null && targetWorkorder != null)
            {
                if (targetStation.stationState == StationState.Running)
                {
                    if (targetStation.type == 0)
                    {
                        StationWorkOrderPartDetail? partDetails = await GetWorkOrderDetailsStatus(targetStation.name, workorderNo, serialNo);
                        if (partDetails == null)
                        {
                            partDetails = new StationWorkOrderPartDetail()
                            {
                                StationName = stationName,
                                WorkOrderNo = workorderNo,
                                PartName = targetWorkorder.PartName,
                                SerialNO = serialNo,
                                WIP = 1,
                                OKAmount = 0,
                                NGAmount = 0,
                                Status = 1,
                                StartTime = DateTime.Now,
                            };
                            ((StationSingleOrderWithPartNO)targetStation).StationIn(partDetails);
                            await InsertStationWorkOrderPartDetails(new List<StationWorkOrderPartDetail> { partDetails, });
                            StationChangedByName(stationName);
                            return new RequestResult(1, $"part {serialNo} station in {stationName} success");
                        }
                        else
                        {
                            return new RequestResult(3, $"part {serialNo} data in {stationName} already start");
                        }
                    }
                    else
                    {
                        return new RequestResult(3, $"{stationName} type {targetStation.type} error");
                    }
                }
                else
                {
                    return new RequestResult(3, $"{stationName} is not running");
                }
            }
            else
            {
                return new RequestResult(3, $"no station {stationName}");
            }
        }
        public async Task<RequestResult> StationOutByNameAndPart(string stationName, string serialNo, bool pass)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                if (target.stationState == StationState.Running)
                {
                    if (target.type == 0)
                    {
                        StationWorkOrderPartDetail? partDetails = await GetWorkOrderDetailsStatus(target.name, ((StationSingleOrderWithPartNO)target).workorder, serialNo);
                        if (partDetails != null && partDetails.Status == 1)
                        {
                            partDetails.Finish();
                            //partDetails.FinishedTime = DateTime.Now;
                            if (pass)
                            {
                                partDetails.OKAmount += 1;
                            }
                            else
                            {
                                partDetails.NGAmount += 1;
                                //await UpdateConsecutiveTaskTargetAmount(partDetails);
                            }
                            partDetails.WIP -= 1;
                            //partDetails.Status = 3;
                            ((StationSingleOrderWithPartNO)target).StationOut(partDetails);
                            await UpdateWorkOrderDetailsAmount(partDetails);
                            await UpdateWorkOrderDetailsTimestamp(partDetails);
                            StationChangedByName(stationName);
                            return new RequestResult(1, $"{stationName} WIP added  success");
                        }
                        return new RequestResult(3, $"serial no {serialNo} data in {stationName} is not in process");
                    }
                    else
                    {
                        return new RequestResult(3, $"{stationName} type {target.type} error");
                    }
                }
                else
                {
                    return new RequestResult(3, $"{stationName} is not running");
                }
            }
            else
            {
                return new RequestResult(3, $"no station {stationName}");
            }
        }
        public async Task UpdateConsecutiveTaskTargetAmount(StationWorkOrderPartDetail ngpart)
        {
            List<string> consecutiveStations = GetConsecutiveStationsInSameProcess(ngpart.StationName);
            if (consecutiveStations.Any())
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                    var target = context.StationWorkOrderPartDetails.Where(x => consecutiveStations.Contains(x.StationName) && x.PartName == ngpart.PartName);
                    if (target != null)
                    {
                        await target.ForEachAsync(x =>
                        {
                            x.TargetAmount -= ngpart.NGAmount;
                            SingleWorkOrderDetailsAmountChanged(x);
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
        public async Task<RequestResult> StationInByName(string stationName, int amount)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                if (target.stationState == StationState.Running)
                {
                    if (target.type == 1)
                    {
                        StationWorkOrderPartDetail? partDetails = ((StationSingleOrderWithoutPartNO)target).stationWorkOrderPartDetail;
                        if (partDetails != null)
                        {
                            //first
                            if (partDetails.Status == 0)
                            {
                                partDetails.Start();
                                //partDetails.StartTime = DateTime.Now;
                                //partDetails.Status = 1;
                                await UpdateWorkOrderDetailsTimestamp(partDetails);
                            }
                            else
                            {

                            }
                        }
                        ((StationSingleOrderWithoutPartNO)target).StationIn(amount);
                        await UpdateWorkOrderDetailsAmount(((StationSingleOrderWithoutPartNO)target).stationWorkOrderPartDetail);
                        StationChangedByName(stationName);
                        return new RequestResult(1, $"{stationName} WIP add {amount} success");

                    }
                    else
                    {
                        return new RequestResult(3, $"{stationName} type {target.type} error");
                    }
                }
                else
                {
                    return new RequestResult(3, $"{stationName} is not running");
                }
            }
            else
            {
                return new RequestResult(3, $"no station {stationName}");
            }
        }
        public async Task<RequestResult> StationOutByName(string stationName, int ok, int ng)
        {
            StationBase target = Stations.FirstOrDefault(x => x.name == stationName);
            if (target != null)
            {
                if (target.stationState == StationState.Running)
                {
                    if (target.type == 1)
                    {
                        ((StationSingleOrderWithoutPartNO)target).StationOut(ok, ng);
                        await UpdateWorkOrderDetailsAmount(((StationSingleOrderWithoutPartNO)target).stationWorkOrderPartDetail);
                        if (ng > 0)
                        {
                            await WorkorderAddngByno(((StationSingleOrderWithoutPartNO)target).stationWorkOrderPartDetail.WorkOrderNo, ng);
                            //await UpdateConsecutiveTaskTargetAmount(((StationSingleOrderWithoutPartNO)target).stationWorkOrderPartDetail);
                        }
                        StationChangedByName(stationName);
                        return new RequestResult(1, $"{stationName} finish ok:{ok} ng:{ng} success");
                    }
                    else
                    {
                        return new RequestResult(3, $"{stationName} type {target.type} error");

                    }
                }
                else
                {
                    return new RequestResult(3, $"{stationName} is not running");
                }
            }
            else
            {
                return new RequestResult(3, $"no station {stationName}");
            }
        }
        //for no part update amount
        private async Task UpdateWorkOrderDetailsAmount(StationWorkOrderPartDetail stationWorkOrderPartDetail)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                StationWorkOrderPartDetail target = context.StationWorkOrderPartDetails.FirstOrDefault(x => x.ID == stationWorkOrderPartDetail.ID);//&& x.StartTime != null && x.FinishedTime == null
                if (target != null)
                {
                    target.WIP = stationWorkOrderPartDetail.WIP;
                    target.OKAmount = stationWorkOrderPartDetail.OKAmount;
                    target.NGAmount = stationWorkOrderPartDetail.NGAmount;
                    await context.SaveChangesAsync();
                    SingleWorkOrderDetailsAmountChanged(target);
                }
                else
                {

                }
            }
        }
        //for part update timestamp
        private async Task UpdateWorkOrderDetailsTimestamp(StationWorkOrderPartDetail stationWorkOrderPartDetail)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                StationWorkOrderPartDetail target = context.StationWorkOrderPartDetails.FirstOrDefault(x => x.ID == stationWorkOrderPartDetail.ID);
                if (target != null)
                {
                    target.StartTime = stationWorkOrderPartDetail.StartTime;
                    target.FinishedTime = stationWorkOrderPartDetail.FinishedTime;
                    target.Status = stationWorkOrderPartDetail.Status;
                    await context.SaveChangesAsync();
                    SingleWorkOrderDetailsAmountChanged(target);
                }
                else
                {

                }
            }
        }
        public async Task<RequestResult> UpdateWorkOrderDetailsCustomData(StationWorkOrderPartDetail stationWorkOrderPartDetail)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                    StationWorkOrderPartDetail target = context.StationWorkOrderPartDetails.FirstOrDefault(x => x.ID == stationWorkOrderPartDetail.ID);
                    if (target != null)
                    {
                        target.Bool_1 = stationWorkOrderPartDetail.Bool_1;
                        target.Bool_2 = stationWorkOrderPartDetail.Bool_2;
                        target.Bool_3 = stationWorkOrderPartDetail.Bool_3;
                        target.Bool_4 = stationWorkOrderPartDetail.Bool_4;
                        target.Bool_5 = stationWorkOrderPartDetail.Bool_5;

                        target.Int_1 = stationWorkOrderPartDetail.Int_1;
                        target.Int_2 = stationWorkOrderPartDetail.Int_2;
                        target.Int_3 = stationWorkOrderPartDetail.Int_3;
                        target.Int_4 = stationWorkOrderPartDetail.Int_4;
                        target.Int_5 = stationWorkOrderPartDetail.Int_5;

                        target.Double_1 = stationWorkOrderPartDetail.Double_1;
                        target.Double_2 = stationWorkOrderPartDetail.Double_2;
                        target.Double_3 = stationWorkOrderPartDetail.Double_3;
                        target.Double_4 = stationWorkOrderPartDetail.Double_4;
                        target.Double_5 = stationWorkOrderPartDetail.Double_5;

                        target.String_1 = stationWorkOrderPartDetail.String_1;
                        target.String_2 = stationWorkOrderPartDetail.String_2;
                        target.String_3 = stationWorkOrderPartDetail.String_3;
                        target.String_4 = stationWorkOrderPartDetail.String_4;
                        target.String_5 = stationWorkOrderPartDetail.String_5;

                        await context.SaveChangesAsync();
                        SingleWorkOrderDetailsAmountChanged(target);
                        return new RequestResult(1, $"custom data log success");
                    }
                    else
                    {
                        return new RequestResult(3, $"detail not found");
                    }
                }
            }
            catch (Exception e)
            {
                return new RequestResult(3, e.Message);
            }

        }
        private Task<bool> CheckIsFirstStation(string stationName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                StationConfig stationConfig = context.StationConfigs.FirstOrDefault(x => x.Name == stationName);
                //check is first station
                int lastStationIndex = context.StationConfigs.Where(x => x.ProcessName == stationConfig.ProcessName).Min(x => x.ProcessStep);
                return Task.FromResult(lastStationIndex == stationConfig.ProcessStep);
            }
        }
        public async Task<RequestResult> WorkOrderStart(string no, DateTime dt)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                WorkOrder target = context.WorkOrders.FirstOrDefault(x => x.WorkOrderNo == no && x.StartTime == null);
                if (target != null)
                {
                    target.Status = 1;
                    target.StartTime = dt;
                    await context.SaveChangesAsync();
                    WorkOrderStatusChanged();
                    await BroadcastToAllStationBaseParamChanged();
                    return new RequestResult(1, $"Start workorder {no} success");
                }
                else
                {
                    return new RequestResult(3, $"workorder {no} with not yet start doesn't exist");
                }
            }
        }
        private Task<bool> CheckIsLastStation(string stationName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                StationConfig stationConfig = context.StationConfigs.FirstOrDefault(x => x.Name == stationName);
                //check is last station
                int lastStationIndex = context.StationConfigs.Where(x => x.ProcessName == stationConfig.ProcessName).Max(x => x.ProcessStep);
                return Task.FromResult(lastStationIndex == stationConfig.ProcessStep);
            }
        }
        public async Task<RequestResult> WorkOrderFinished(string no, DateTime dt)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                int toatlOK = 0;//context.StationWorkOrderPartDetails.Where(x => x.WorkOrderNo == no && x.Status == 3).Min(x => x.OKAmount);
                int totalNG = 0;//context.StationWorkOrderPartDetails.Where(x => x.WorkOrderNo == no && x.Status == 3).Sum(x => x.NGAmount);
                WorkOrder target = context.WorkOrders.FirstOrDefault(x => x.WorkOrderNo == no && x.FinishedTime == null);
                if (target != null)
                {
                    List<StationWorkOrderPartDetail> finishedDetails = await GetStationWorkOrderPartDetailFinishedByWorkorder(no);
                    toatlOK = finishedDetails.Sum(x => x.OKAmount);
                    totalNG = finishedDetails.Sum(x => x.NGAmount);

                    target.Okamount = toatlOK;
                    target.Ngamount = totalNG;
                    target.Status = 3;
                    target.FinishedTime = dt;
                    await context.SaveChangesAsync();
                    WorkOrderStatusChanged();
                    return new RequestResult(1, $"Finish workorder {no} success");
                }
                else
                {
                    return new RequestResult(3, $"workorder {no} which is running doesn't exist");
                }
            }
        }
        #endregion

        #region system config
        public Task<List<SystemConfig>> GetSystemConfigs()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.SystemConfigs.ToList());
            }
        }
        public async Task UpdateSystemConfigs(SystemConfig systemConfig)
        {

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                SystemConfig target = context.SystemConfigs.FirstOrDefault(x => x.ConfigName == systemConfig.ConfigName);
                if (target != null)
                {
                    try
                    {
                        if (target.Value != systemConfig.Value)
                        {
                            //valid type
                            switch (target.ValueType)
                            {
                                case 0:
                                    bool b = Boolean.Parse(target.Value);
                                    break;
                                case 1:
                                    int i = Int32.Parse(target.Value);
                                    break;
                                case 2:
                                    float f = Int32.Parse(target.Value);
                                    break;
                                case 3:
                                    break;
                                default:
                                    break;
                            }
                            target.Value = systemConfig.Value;
                            await context.SaveChangesAsync();
                        }


                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }
        public List<string> GetSystemConfigList(string configName)
        {
            switch (configName)
            {
                case "Language":
                    return SupportLanguages.langs;
                default:
                    return new List<string> { };
            }
        }
        public Task<string> GetLanguage()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                SystemConfig lang = context.SystemConfigs.FirstOrDefault(x => x.ConfigName == "Language");
                return Task.FromResult(lang == null ? "" : lang.Value);
            }
        }
        public async Task SetLanguage(string lang)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                SystemConfig target = context.SystemConfigs.FirstOrDefault(x => x.ConfigName == "Language");
                if (target.Value != lang)
                {
                    target.Value = lang;
                    await context.SaveChangesAsync();
                }
            }
        }
        #endregion

        #region developer
        public Task<List<DeveloperCommand>> GetDeveloperCommands()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                return Task.FromResult(context.DeveloperCommands.ToList());
            }
        }
        public async Task<RequestResult> ResetWorkOrder(string wo)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopfloorServiceDbContext>();
                WorkOrder workOrder = context.WorkOrders.FirstOrDefault(x => x.WorkOrderNo == wo);
                if (workOrder != null)
                {
                    workOrder.Okamount = 0;
                    workOrder.Ngamount = 0;
                    workOrder.StartTime = null;
                    workOrder.FinishedTime = null;
                    workOrder.Status = 0;

                    var stationWorkOrderPartDetail = context.StationWorkOrderPartDetails.Where(x => x.WorkOrderNo == wo);
                    if (stationWorkOrderPartDetail != null)
                    {
                        //await stationWorkOrderPartDetail.ForEachAsync(x =>
                        //{
                        //    x.WIP = 0;
                        //    x.OKAmount = 0;
                        //    x.NGAmount = 0;
                        //    if (String.IsNullOrEmpty(x.PartName))
                        //    {
                        //        x.TargetAmount = workOrder.TargetAmount;
                        //    }
                        //    else
                        //    {
                        //        x.TargetAmount = 1;
                        //    }
                        //    x.StartTime = null;
                        //    x.FinishedTime = null;
                        //    x.Status = 0;
                        //    x.Bool_1 = null;
                        //    x.Bool_2 = null;
                        //    x.Bool_3 = null;
                        //    x.Bool_4 = null;
                        //    x.Bool_5 = null;

                        //    x.Int_1 = null;
                        //    x.Int_2 = null;
                        //    x.Int_3 = null;
                        //    x.Int_4 = null;
                        //    x.Int_5 = null;

                        //    x.Double_1 = null;
                        //    x.Double_2 = null;
                        //    x.Double_3 = null;
                        //    x.Double_4 = null;
                        //    x.Double_5 = null;

                        //    x.String_1 = null;
                        //    x.String_2 = null;
                        //    x.String_3 = null;
                        //    x.String_4 = null;
                        //    x.String_5 = null;
                        //});
                        context.RemoveRange(stationWorkOrderPartDetail);
                        await context.SaveChangesAsync();
                        return new RequestResult(1, $"workorder {wo} reset success");
                    }
                    else
                    {
                        return new RequestResult(3, $"workorder {wo} details not exist");
                    }
                }
                else
                {
                    return new RequestResult(3, $"workorder {wo} not exist");
                }
            }
        }


        #endregion
    }
}
