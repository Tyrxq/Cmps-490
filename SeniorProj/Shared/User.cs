namespace SeniorProj.Shared;

public class User
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public string? Email { get; set; }
    public string? PostalCode { get; set; }
    public bool? Notifications { get; set; }
    public string? Name { get; set; }
}