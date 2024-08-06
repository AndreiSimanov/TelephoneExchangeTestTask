namespace TelephoneExchange.Models;

public record Agent(string Name, CancellationTokenSource Cts) : IDisposable
{
    readonly Random rnd = new ();

    public async Task  ProcessCall(Call call)
    {
        var durationSec = rnd.Next(1000, 5000);
        await Task.Delay(durationSec);
        call.AgentName = Name;
        call.DurationSec = durationSec;
    }

    public void Dispose()
    {
        Cts.Cancel();
        Cts.Dispose();
    }
}
