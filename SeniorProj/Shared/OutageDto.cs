namespace SeniorProj.Shared;

public class OutageDto
{

    public int OutageID { get; set; }
    public List<string> OutageZIP { get; set; }
    public string? CompanyName { get; set; }
    public string? RepairTime { get; set; }
    public string? RepairDate { get; set; }
    public string? Description { get; set; }


}