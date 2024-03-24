namespace RevisioHub.Web.Model;

public class Configuration
{
    public String JwtSecret { get; set; } = string.Empty;
    public String JwtIssuer { get; set; } = string.Empty;
    public String JwtAudience { get; set; } = string.Empty;
}
