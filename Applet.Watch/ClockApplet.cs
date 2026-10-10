using System.Windows.Threading;
using AppDock.SDK;
using Microsoft.Win32;

namespace Applets.Watch;

internal sealed class ClockApplet(Dispatcher dispatcher) : IAppDockExtension
{
    private IExtensionContext? context;
    private ClockWindow? clock;
    private IDisposable? settingsSubscription;
    public async Task ActivateAsync(IExtensionContext services, CancellationToken cancellationToken)
    {
        context = services;
        await dispatcher.InvokeAsync(() => { clock = new ClockWindow(); clock.Apply(ClockOptions.Read(services.Settings)); });
        await PublishMonitorsAsync(cancellationToken);
        settingsSubscription = services.Settings.OnChanged(ApplyAsync);
        SystemEvents.DisplaySettingsChanged += DisplayChanged;
        await PublishStateAsync(cancellationToken);
    }
    private async Task ApplyAsync(CancellationToken token)
    {
        if (context is null) return;
        var options = ClockOptions.Read(context.Settings);
        await dispatcher.InvokeAsync(() => clock?.Apply(options));
        await PublishStateAsync(token);
    }
    private async Task PublishMonitorsAsync(CancellationToken token)
    {
        if (context is null) return;
        var options = await dispatcher.InvokeAsync(ClockWindow.MonitorOptions);
        await context.Settings.SetOptionsAsync("monitor", options, token);
    }
    private async Task PublishStateAsync(CancellationToken token)
    {
        if (context is null || clock is null) return;
        var panel = await dispatcher.InvokeAsync(() => clock.StatePanel(context.ExtensionId));
        await context.Ui.ShowPanelAsync(panel, token);
    }
    private async void DisplayChanged(object? sender, EventArgs e)
    {
        try { await PublishMonitorsAsync(CancellationToken.None); await ApplyAsync(CancellationToken.None); }
        catch (Exception error) { Console.Error.WriteLine(error); }
    }
    public async Task DeactivateAsync(CancellationToken cancellationToken)
    {
        SystemEvents.DisplaySettingsChanged -= DisplayChanged;
        settingsSubscription?.Dispose();
        context = null;
        await dispatcher.InvokeAsync(() => { clock?.Close(); clock = null; });
    }
}
