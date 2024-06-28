using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace ShopfloorBlazorApp.EFModels;

public partial class StationWorkOrderPartDetail
{
    public StationWorkOrderPartDetail()
    {}
    public StationWorkOrderPartDetail(StationWorkOrderPartDetail stationWorkOrderPartDetail)
    {
        ID = stationWorkOrderPartDetail.ID;
        StationName = stationWorkOrderPartDetail.StationName;
        WorkOrderNo = stationWorkOrderPartDetail.WorkOrderNo;
        PartName = stationWorkOrderPartDetail.PartName;
        SerialNO = stationWorkOrderPartDetail.SerialNO;
        WIP = stationWorkOrderPartDetail.WIP;
        TargetAmount = stationWorkOrderPartDetail.TargetAmount;
        OKAmount = stationWorkOrderPartDetail.OKAmount;
        NGAmount = stationWorkOrderPartDetail.NGAmount;
        StartTime = stationWorkOrderPartDetail.StartTime;
        FinishedTime = stationWorkOrderPartDetail.FinishedTime;
        Status = stationWorkOrderPartDetail.Status;

        Bool_1 = stationWorkOrderPartDetail.Bool_1;
        Bool_2 = stationWorkOrderPartDetail.Bool_2;
        Bool_3 = stationWorkOrderPartDetail.Bool_3;
        Bool_4 = stationWorkOrderPartDetail.Bool_4;
        Bool_5 = stationWorkOrderPartDetail.Bool_5;
        Int_1 = stationWorkOrderPartDetail.Int_1;
        Int_2 = stationWorkOrderPartDetail.Int_2;
        Int_3 = stationWorkOrderPartDetail.Int_3;
        Int_4 = stationWorkOrderPartDetail.Int_4;
        Int_5 = stationWorkOrderPartDetail.Int_5;
        Double_1 = stationWorkOrderPartDetail.Double_1;
        Double_2 = stationWorkOrderPartDetail.Double_2;
        Double_3 = stationWorkOrderPartDetail.Double_3;
        Double_4 = stationWorkOrderPartDetail.Double_4;
        Double_5 = stationWorkOrderPartDetail.Double_5;
        String_1 = stationWorkOrderPartDetail.String_1;
        String_2 = stationWorkOrderPartDetail.String_2;
        String_3 = stationWorkOrderPartDetail.String_3;
        String_4 = stationWorkOrderPartDetail.String_4;
        String_5 = stationWorkOrderPartDetail.String_5;
    }
    public void Start()
    {
        StartTime = DateTime.Now;
        Status = 1;
    }
    public void Start(DateTime dt)
    {
        StartTime = dt;
        Status = 1;
    }
    public void Finish(DateTime dt)
    {
        FinishedTime = dt;
        Status = 3;
    }
    public void Finish()
    {
        FinishedTime = DateTime.Now;
        Status = 3;
    }

    public bool SetCustomData(int dataType, int sequence, string valStr)
    {
        try
        {
            PropertyInfo? att;
            switch (dataType)
            {
                case 0:
                    bool b = Convert.ToBoolean(valStr);
                    att = typeof(StationWorkOrderPartDetail).GetProperty($"Bool_{sequence}");
                    if (att == null)
                    {
                        return false;
                    }
                    else
                    {
                        att.SetValue( this, b);
                    }
                    break;
                case 1:
                    int i = Int32.Parse(valStr);
                    att = typeof(StationWorkOrderPartDetail).GetProperty($"Int_{sequence}");
                    if (att == null)
                    {
                        return false;
                    }
                    else
                    {
                        att.SetValue(this, i);
                    }
                    break;
                case 2:
                    double d = Double.Parse(valStr);
                    att = typeof(StationWorkOrderPartDetail).GetProperty($"Double_{sequence}");
                    if (att == null)
                    {
                        return false;
                    }
                    else
                    {
                        att.SetValue(this, d);
                    }
                    break;
                case 3:
                    att = typeof(StationWorkOrderPartDetail).GetProperty($"String_{sequence}");
                    if (att == null)
                    {
                        return false;
                    }
                    else
                    {
                        att.SetValue(this, valStr);
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
        catch(Exception e)
        {
            return false;
        }
    } 
}
