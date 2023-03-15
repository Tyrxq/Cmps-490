namespace SeniorProj.Shared;

public class UserDto
{

    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Password1 { get; set; }
    public string? Password2 { get; set; }
    public string? Email { get; set; }
    public string? PostalCode { get; set; }
    public bool? Notifications { get; set; }

}