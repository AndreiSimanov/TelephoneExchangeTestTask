namespace TelephoneExchange.Models;

public record class Call(Guid Id)
{
    public string? AgentName { get; set; }
    public int DurationSec { get; set; }

}
