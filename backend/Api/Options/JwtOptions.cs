namespace AdSPMdS.DemanioDigitale.Api.Options;

public class JwtOptions
{
    public string Key { get; set; } = "ChangeMeSuperSecretKey!";
    public string Issuer { get; set; } = "AdspMds.Identity";
    public string Audience { get; set; } = "AdspMds.Client";
    public int ExpirationMinutes { get; set; } = 120;
}
