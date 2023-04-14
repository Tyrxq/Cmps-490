namespace SeniorProj.Shared;

public class Outage
{
    public int Id { get; set; }
    public string? OutageZIP { get; set; }
    public string? CompanyName { get; set; }
    public string? RepairTime { get; set; }
    public string? RepairDate { get; set; }
    public string? Description { get; set; }
}