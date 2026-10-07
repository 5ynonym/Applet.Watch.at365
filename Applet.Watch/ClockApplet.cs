using AppDock.SDK;

namespace Applets.Watch;

public sealed class ClockApplet : IAppDockExtension
{
    private IExtensionContext? context;
    private IDisposable? settingsSubscription;
    public async Task ActivateAsync(IExtensionContext services, CancellationToken cancellationToken)
    {
        context = services;
        await PublishAsync(cancellationToken);
        services.Commands.Register(services.ExtensionId + ".show", "時計と日付を表示", token => SetVisibleAsync(true, token));
        services.Commands.Register(services.ExtensionId + ".hide", "時計と日付を非表示", token => SetVisibleAsync(false, token));
        services.Commands.Register(services.ExtensionId + ".toggle", "時計と日付の表示を切り替え", ToggleAsync);
        settingsSubscription = services.Settings.OnChanged(PublishAsync);
    }
    private async Task ToggleAsync(CancellationToken token)
    {
        var services = context ?? throw new InvalidOperationException("Appletは停止しています。");
        var placements = await services.Widgets.GetPlacementsAsync(token);
        await SetVisibleAsync(!placements.Values.Any(p => p.Desktop), token);
    }
    private async Task SetVisibleAsync(bool visible, CancellationToken token)
    {
        var services = context ?? throw new InvalidOperationException("Appletは停止しています。");
        await services.Widgets.SetDesktopAsync([services.ExtensionId + ".clock", services.ExtensionId + ".date"], visible, token);
        await services.Settings.SetAsync("visible", visible, token);
    }
    internal static WidgetDefinition[] Definitions(string id, ClockOptions options)
    {
        var bottom = options.Alignment == "bottom";
        return [
            new(id + ".clock", "時計", new("clock") { ShowSeconds = options.ShowSeconds, LetterSpacing = options.LetterSpacing, Locale = "en-GB" }) {
                Description = "時・分・秒。ホームと透過デスクトップで共通の時計を表示します。", FontFile = "Resources/Hatten.ttf",
                InitialPlacement = new() { Desktop = options.Visible, Monitor = options.Monitor, Anchor = bottom ? "bottom-left" : "top-left", X = options.HorizontalMargin, Y = options.VerticalMargin, Width = Math.Clamp(options.FontSize * 2.5, 120, 7680), Height = Math.Max(60, options.FontSize), FontSize = options.FontSize, Opacity = options.Opacity }
            },
            new(id + ".date", "日付", new("date") { LetterSpacing = options.LetterSpacing, Locale = "en-US" }) {
                Description = "月日と曜日。時計とは独立してピン留めや配置を変更できます。", FontFile = "Resources/Hatten.ttf",
                InitialPlacement = new() { Desktop = options.Visible && options.ShowDate, Order = 1, Monitor = options.Monitor, Anchor = bottom ? "bottom-right" : "top-right", X = options.HorizontalMargin, Y = options.VerticalMargin, Width = Math.Clamp(options.FontSize * 1.7, 120, 7680), Height = Math.Max(60, options.FontSize / 2), FontSize = Math.Max(12, options.FontSize / 2), Opacity = options.Opacity }
            }
        ];
    }
    private async Task PublishAsync(CancellationToken token)
    {
        if (context is null) return;
        await context.Widgets.ReplaceAsync(Definitions(context.ExtensionId, ClockOptions.Read(context.Settings)), token);
        await context.Ui.ShowPanelAsync(new("時計と日付のウィジェット", "ホームへのピン留め・デスクトップの表示場所・前後関係は、AppDockのウィジェットページで設定できます。", Actions: [
            new("デスクトップに表示", context.ExtensionId + ".show"), new("デスクトップから非表示", context.ExtensionId + ".hide"), new("表示を切り替え", context.ExtensionId + ".toggle")]), token);
    }
    public Task DeactivateAsync(CancellationToken cancellationToken)
    {
        settingsSubscription?.Dispose(); settingsSubscription = null; context = null;
        return Task.CompletedTask;
    }
}
