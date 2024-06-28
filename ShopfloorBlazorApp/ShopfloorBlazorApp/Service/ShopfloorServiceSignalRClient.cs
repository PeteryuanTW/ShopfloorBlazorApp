using CommonLibrary.SignalRPack;
using CommonLibrary.TagPack;
using Microsoft.AspNetCore.SignalR.Client;

namespace ShopfloorBlazorApp.Service
{
    public class ShopfloorServiceSignalRClient : SignalRClient
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ShopfloorServiceDataService shopfloorServiceDataService;

        public ShopfloorServiceSignalRClient(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
            this.shopfloorServiceDataService = this.scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ShopfloorServiceDataService>();
            StateChanged(SignalRConnectionState.Init);
            ConnectToSignalRServer("s", 0);
        }
        public override async Task<RequestResult> ConnectToSignalRServer(string ip, int port)
        {
            try
            {
                string connectionString = await shopfloorServiceDataService.GetServerConnectionString();
                hubConnection = new HubConnectionBuilder()
                    .WithUrl(connectionString, (opts) =>
                    {
                        opts.HttpMessageHandlerFactory = (message) =>
                        {
                            if (message is HttpClientHandler clientHandler)
                                // always verify the SSL certificate
                                clientHandler.ServerCertificateCustomValidationCallback +=
                                    (sender, certificate, chain, sslPolicyErrors) => { return true; };
                            return message;
                        };
                    })
                    .WithAutomaticReconnect(new ReconnectPolicy(10))
                    .Build();

                hubConnection.Closed += async (e) =>
                {
                    await Task.Run(() =>
                    {
                        StateChanged(SignalRConnectionState.Disconnected);
                    });
                };
                hubConnection.Reconnecting += async (e) =>
                {
                    await Task.Run(() =>
                    {
                        StateChanged(SignalRConnectionState.Connecting);
                    });
                };
                hubConnection.Reconnected += async (e) =>
                {
                    await Task.Run(() =>
                    {
                        StateChanged(SignalRConnectionState.Connected);
                    });
                };

                hubConnection.On<string, string, RequestResult>("ShopfloorStationInWithPart_ServerToClient", async (machineName, part) =>
                {
                    return await shopfloorServiceDataService.StationInByNameAndPart(machineName, part);
                });

                hubConnection.On<string, int, RequestResult>("ShopfloorStationInWithoutPart_ServerToClient", async (machineName, amount) =>
                {
                    return await shopfloorServiceDataService.StationInByName(machineName, amount);
                });


                hubConnection.On<string, string, bool, RequestResult>("ShopfloorStationOutWithPart_ServerToClient", async (machineName, part, pass) =>
                {
                    return await shopfloorServiceDataService.StationOutByNameAndPart(machineName, part, pass);
                });
                hubConnection.On<string, int, int, RequestResult>("ShopfloorStationOutWithoutPart_ServerToClient", async (machineName, ok, ng) =>
                {
                    return await shopfloorServiceDataService.StationOutByName(machineName, ok, ng);
                });
                



                StateChanged(SignalRConnectionState.Connecting);
                await hubConnection.StartAsync();
                StateChanged(SignalRConnectionState.Connected);
                return new RequestResult(1, $"Connect to Service Manager Success");
            }
            catch (Exception e)
            {
                StateChanged(SignalRConnectionState.Disconnected);
                return new RequestResult(3, e.Message);
            }
        }
    }
}
