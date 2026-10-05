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
        services.Commands.Register(services.ExtensionId + ".show", "時計を表示", token => SetVisibleAsync(true, token));
        services.Commands.Register(services.ExtensionId + ".hide", "時計を非表示", token => SetVisibleAsync(false, token));
        services.Commands.Register(services.ExtensionId + ".toggle", "時計の表示を切り替え", token => SetVisibleAsync(!services.Settings.Get("visible", true), token));
        settingsSubscription = services.Settings.OnChanged(ApplyAsync);
        SystemEvents.DisplaySettingsChanged += DisplayChanged;
        await PublishStateAsync(cancellationToken);
    }
    private async Task SetVisibleAsync(bool visible, CancellationToken token)
    {
        var services = context ?? throw new InvalidOperationException("Appletは停止しています。");
        await services.Settings.SetAsync("visible", visible, token);
        await ApplyAsync(token);
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
