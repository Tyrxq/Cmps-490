namespace SeniorProj.Shared;

public class ForumPost
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Zip { get; set; }
    public string? Message { get; set; }
    public int UserId { get; set; }
}