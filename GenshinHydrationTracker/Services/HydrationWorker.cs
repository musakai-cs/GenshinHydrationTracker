using System.Text;
using System.Text.Json;

namespace GenshinHydrationTracker.Services;

public class DiscordHydrationWorker(IConfiguration coniguration)
{
    private readonly string _webhookUrl = coniguration["DISCORD_WEBHOOK_URL"] ?? throw new NullReferenceException(nameof(coniguration));
    private readonly string _discordUserIdPin = coniguration["DISCORD_USER_ID_PIN"] ?? throw new NullReferenceException(nameof(coniguration));

    private readonly uint _intervalMinutes =
        uint.TryParse(coniguration["REMINDER_INTERVAL_MINUTES"], out var minutes) && minutes > 0
            ? minutes
            : throw new InvalidOperationException("REMINDER_INTERVAL_MINUTES musí být kladné číslo!");

    private CancellationTokenSource? _cts;

    public void StartHydrationReminders()
    {
        if (_cts is not null) return;

        _cts = new CancellationTokenSource();
        _ = RunLoopAsync(_cts.Token);
    }

    public void StopHydrationReminders()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    public bool IsHydrationRemindersRunning() => _cts is not null && !_cts.IsCancellationRequested;

    private async Task RunLoopAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_intervalMinutes));
        using var httpClient = new HttpClient();

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                var payload = new
                {
                    content = $"<@{_discordUserIdPin}>",
                    embeds = new[]
                    {
                        new {
                            title = "💧 Čas na osvěžení!",
                            description = $"Paimon hlásí: Uplynulo dalších **{_intervalMinutes} minut** hraní. Nezapomeň se napít vody a protáhnout se! 🧊",
                            color = 3447003,
                            footer = new {
                                text = "Genshin Hydration Tracker • DanielKlement.net"
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await httpClient.PostAsync(_webhookUrl, content, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}