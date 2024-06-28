using System.ComponentModel.DataAnnotations;

namespace ShopfloorBlazorApp.Data
{
    public class CustomDataParam
    {
        [Required]
        public string stationName { get; set; }
        [Required]
        public string serialNo { get; set; }
        [Required]
        public int dataType { get; set; }
        [Required]
        public int sequence { get; set; }
        [Required]
        public string val { get; set; }
    }
}
