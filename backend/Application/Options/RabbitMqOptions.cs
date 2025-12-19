using System.ComponentModel.DataAnnotations;

namespace AdSPMdS.DemanioDigitale.Application.Options;

public class RabbitMqOptions
{
    [Required]
    public string Host { get; set; } = "localhost";

    public string VirtualHost { get; set; } = "/";

    [Required]
    public string Username { get; set; } = "guest";

    [Required]
    public string Password { get; set; } = "guest";

    public string UserRegisteredQueue { get; set; } = "demaniodigitale-user-registered";
}
