namespace ShopfloorBlazorApp.EFModels
{
    public partial class MapStationConfig
    {
        public string MapName { get; set; }
        public string StationName { get; set; }
        public int Position_x { get; set; }
        public int Position_y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
