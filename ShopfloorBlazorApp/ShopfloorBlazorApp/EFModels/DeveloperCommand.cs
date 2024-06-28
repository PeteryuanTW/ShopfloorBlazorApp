namespace ShopfloorBlazorApp.EFModels
{
    public class DeveloperCommand
    {
        public int CommandCode { get; set; }
        public string CommandName { get; set; } = null!;
        public int ParameterAmount {  get; set; }
        public string Hint { get; set; } = null!;
        public string ParameterType {  get; set; } = null!;
    }
}
