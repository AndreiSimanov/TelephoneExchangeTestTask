using System.Threading.Channels;

namespace TelephoneExchange.Models;

public record Agent(string Name) : IDisposable
{
    readonly Random rnd = new();
    readonly CancellationTokenSource cts = new CancellationTokenSource();

    public void ProcessCalls(Channel<Call> сhannel, Action<Call> callback, TaskScheduler taskScheduler) =>
        Task.Factory.StartNew(async () =>
        {
            var token = cts.Token;
            while (!token.IsCancellationRequested)
            {
                var call = await сhannel.Reader.ReadAsync(token);
                if (call == null)
                    break;

                var durationSec = rnd.Next(1000, 5000);
                await Task.Delay(durationSec);
                call.AgentName = Name;
                call.DurationSec = durationSec;
                callback?.Invoke(call);
            }
        },
        CancellationToken.None,
        TaskCreationOptions.None,
        taskScheduler);

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }
}
