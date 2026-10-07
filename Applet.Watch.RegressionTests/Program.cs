using Applets.Watch;
using AppDock.SDK;

var passed = 0;
void Check(string name, Action assertion) { assertion(); passed++; Console.WriteLine("PASS " + name); }
void Equal<T>(T expected, T actual) { if (!Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}"); }
var primary = new MonitorInfo("main", "Main", 0, 0, 2560, 1440, true);
var left = new MonitorInfo("left", "Left", -1920, -360, 1920, 1080, false);
var monitors = new[] { left, primary };
Check("primary fallback does not depend on enumeration order", () => Equal(primary, ClockLayout.SelectMonitor(monitors, "primary")));
Check("select a monitor at negative desktop coordinates", () => Equal(left, ClockLayout.SelectMonitor(monitors, "left")));
Check("unplugged monitor falls back without replacing the configured ID", () => { var options = new ClockOptions(Monitor: "unplugged"); Equal(primary, ClockLayout.SelectMonitor(monitors, options.Monitor)); Equal("unplugged", options.Monitor); });
Check("top margin converts DIP to physical pixels", () => Equal(new PixelBounds(-1920, -315, 1920, 300), ClockLayout.Place(left, 200, 1.5, new(VerticalMargin: 30))));
Check("bottom placement respects monitor origin and DPI", () => Equal(new PixelBounds(-1920, 375, 1920, 300), ClockLayout.Place(left, 200, 1.5, new(Alignment: "bottom", VerticalMargin: 30))));
Check("oversized clock is clipped to the selected monitor", () => Equal(new PixelBounds(-1920, -360, 1920, 1080), ClockLayout.Place(left, 3000, 2, new(Alignment: "bottom", VerticalMargin: 1000))));
Check("excessive margins keep the window in the screen", () => Equal(1140, ClockLayout.Place(primary, 300, 1, new(VerticalMargin: 10000)).Top));
Check("manual invalid settings remain safe", () => { var options = new ClockOptions(Alignment: "invalid", FontSize: double.NaN, Opacity: 20, HorizontalMargin: -100, VerticalMargin: double.PositiveInfinity, LetterSpacing: double.PositiveInfinity).Normalize(); Equal("top", options.Alignment); Equal(420d, options.FontSize); Equal(1d, options.Opacity); Equal(0d, options.HorizontalMargin); Equal(0d, options.VerticalMargin); Equal(0d, options.LetterSpacing); });
Check("legacy visibility, bottom anchor and opacity seed independent widgets", () => {
    var widgets = ClockApplet.Definitions("at365.watch", new(Visible: false, Alignment: "bottom", Opacity: 0.25));
    Equal("at365.watch.clock", widgets[0].Id); Equal("at365.watch.date", widgets[1].Id);
    Equal(false, widgets[0].InitialPlacement!.Desktop); Equal("bottom-left", widgets[0].InitialPlacement!.Anchor);
    Equal("bottom-right", widgets[1].InitialPlacement!.Anchor); Equal(0.25, widgets[1].InitialPlacement!.Opacity);
});
Check("legacy date and seconds opt-outs carry into widget definitions", () => {
    var widgets = ClockApplet.Definitions("at365.watch", new(ShowDate: false, ShowSeconds: false));
    Equal(false, widgets[0].Content.ShowSeconds); Equal(true, widgets[0].InitialPlacement!.Desktop); Equal(false, widgets[1].InitialPlacement!.Desktop);
});
Check("character spacing carries into both widget definitions", () => {
    var widgets = ClockApplet.Definitions("at365.watch", new(LetterSpacing: 24));
    Equal(24d, widgets[0].Content.LetterSpacing); Equal(24d, widgets[1].Content.LetterSpacing);
});
var services = new TestServices();
var applet = new ClockApplet();
await applet.ActivateAsync(services, CancellationToken.None);
Check("activation publishes two widgets without scheduling a per-second task", () => { Equal(2, services.Definitions.Count); Equal(3, services.Handlers.Count); Equal(1, services.Published); });
services.Placements["at365.watch.clock"] = new() { Desktop = true, Home = true, X = 123 };
services.Placements["at365.watch.date"] = new() { Desktop = false, X = 456 };
await services.Handlers["at365.watch.toggle"](CancellationToken.None);
Check("toggle follows actual independent placements and preserves pins and geometry", () => {
    Equal(false, services.Placements["at365.watch.clock"].Desktop); Equal(false, services.Placements["at365.watch.date"].Desktop);
    Equal(true, services.Placements["at365.watch.clock"].Home); Equal(123d, services.Placements["at365.watch.clock"].X); Equal(456d, services.Placements["at365.watch.date"].X);
});
await services.Handlers["at365.watch.toggle"](CancellationToken.None);
Check("toggle restores both widgets when all are hidden", () => { Equal(true, services.Placements.Values.All(p => p.Desktop)); Equal(true, services.Get("visible", false)); });
await applet.DeactivateAsync(CancellationToken.None);
Check("deactivation removes the settings subscription", () => Equal(null, services.Changed));
Console.WriteLine($"{passed}/{passed} passed");

sealed class TestServices : IExtensionContext, ICommandService, ISettingsService, IWidgetService, IUiService
{
    public string ExtensionId => "at365.watch";
    public ICommandService Commands => this;
    public ISettingsService Settings => this;
    public IWidgetService Widgets => this;
    public IUiService Ui => this;
    public ITrayService Tray => throw new NotSupportedException();
    public INotificationService Notifications => throw new NotSupportedException();
    public IBrowserService Browser => throw new NotSupportedException();
    public ILogService Log => throw new NotSupportedException();
    public IStorageService Storage => throw new NotSupportedException();
    public ISecretService Secrets => throw new NotSupportedException();
    public ISchedulerService Scheduler => throw new NotSupportedException("The clock must tick in the renderer.");
    public Dictionary<string, Func<CancellationToken, Task>> Handlers { get; } = [];
    public Dictionary<string, object> Values { get; } = [];
    public Dictionary<string, WidgetPlacement> Placements { get; } = [];
    public IReadOnlyList<WidgetDefinition> Definitions { get; private set; } = [];
    public int Published { get; private set; }
    public Func<CancellationToken, Task>? Changed { get; private set; }
    public void Register(string id, string title, Func<CancellationToken, Task> handler) => Handlers.Add(id, handler);
    public Task ReplaceAsync(IReadOnlyList<CommandRegistration> commands, CancellationToken token = default) => throw new NotSupportedException();
    public T Get<T>(string key, T fallback) => Values.TryGetValue(key, out var value) ? (T)value : fallback;
    public Task SetAsync<T>(string key, T value, CancellationToken token = default) { Values[key] = value!; return Task.CompletedTask; }
    public IDisposable OnChanged(Func<CancellationToken, Task> handler) { Changed = handler; return new Subscription(() => Changed = null); }
    public Task SetOptionsAsync(string key, IReadOnlyList<SettingOption> options, CancellationToken token = default) => throw new NotSupportedException();
    public Task ReplaceAsync(IReadOnlyList<WidgetDefinition> widgets, CancellationToken token = default) {
        Definitions = widgets; Published++;
        foreach (var w in widgets) Placements.TryAdd(w.Id, w.InitialPlacement ?? new());
        return Task.CompletedTask;
    }
    public Task<IReadOnlyDictionary<string, WidgetPlacement>> GetPlacementsAsync(CancellationToken token = default) => Task.FromResult<IReadOnlyDictionary<string, WidgetPlacement>>(Placements);
    public Task SetDesktopAsync(IReadOnlyList<string> ids, bool enabled, CancellationToken token = default) { foreach (var id in ids) Placements[id] = Placements[id] with { Desktop = enabled }; return Task.CompletedTask; }
    public Task ShowPanelAsync(Panel panel, CancellationToken token = default) => Task.CompletedTask;
    public Task<string> GetImageDirectoryAsync(CancellationToken token = default) => throw new NotSupportedException();
    private sealed class Subscription(Action dispose) : IDisposable { public void Dispose() => dispose(); }
}
