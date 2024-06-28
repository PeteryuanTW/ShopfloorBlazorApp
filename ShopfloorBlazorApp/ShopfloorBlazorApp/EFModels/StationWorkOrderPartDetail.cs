namespace ShopfloorBlazorApp.EFModels
{
    public partial class StationWorkOrderPartDetail
    {
        public int ID { get; set; }
        public string StationName { get; set; } = null!;
        public string WorkOrderNo { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public string SerialNO { get; set; } = null!;
        public int WIP { get; set; }
        public int TargetAmount { get; set; }
        public int OKAmount { get; set; }
        public int NGAmount { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishedTime { get; set; }
        public int Status { get; set; }
        public bool? Bool_1 {  get; set; }
        public bool? Bool_2 { get; set; }
        public bool? Bool_3 { get; set; }
        public bool? Bool_4 { get; set; }
        public bool? Bool_5 { get; set; }
        public int? Int_1 { get; set; }
        public int? Int_2 { get; set; }
        public int? Int_3 { get; set; }
        public int? Int_4 { get; set; }
        public int? Int_5 { get; set; }
        public double? Double_1 { get; set; }
        public double? Double_2 { get; set; }
        public double? Double_3 { get; set; }
        public double? Double_4 { get; set; }
        public double? Double_5 { get; set; }
        public string? String_1 { get; set; }
        public string? String_2 { get; set; }
        public string? String_3 { get; set; }
        public string? String_4 { get; set; }
        public string? String_5 { get; set; }
    }
}
