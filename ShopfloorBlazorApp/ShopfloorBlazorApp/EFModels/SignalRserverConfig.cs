namespace ShopfloorBlazorApp.EFModels
{
    public class SignalRserverConfig
    {
        public int Protocol { get; set; }

        public string Ip { get; set; } = null!;

        public int Port { get; set; }

        public string Route { get; set; } = null!;
    }
}
