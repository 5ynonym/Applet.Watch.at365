const path = require("node:path");
const fs = require("node:fs");
const assert = require("node:assert/strict");
const root = path.resolve(__dirname, "..");
const host = path.resolve(root, "../AppDock.at365");
const packagedExecutable = process.argv[2]
  ? path.resolve(process.argv[2])
  : null;
const { _electron: electron } = require(
  path.join(host, "node_modules/playwright"),
);
const profile = path.join(root, ".artifacts", `ui-${Date.now()}`);
const probe = path.join(profile, "clock");
const folder = path.join(profile, "extensions/Applet.Watch.at365");
fs.mkdirSync(folder, { recursive: true });
for (const name of ["extension.json", "Applet.Watch.at365.exe"])
  fs.copyFileSync(
    path.join(root, "publish/Applet.Watch.at365", name),
    path.join(folder, name),
  );
const { createDefaultSettings } = require(
  path.join(host, "out/main/shared/settings-schema.js"),
);
const config = createDefaultSettings();
config.host.notifications = false;
fs.writeFileSync(path.join(profile, "settings.json"), JSON.stringify(config));
let application;
const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));
async function until(check, message) {
  const end = Date.now() + 20000;
  while (Date.now() < end) {
    try {
      const value = await check();
      if (value) return value;
    } catch {}
    await sleep(80);
  }
  throw new Error(message);
}
function state() {
  return JSON.parse(
    fs.readFileSync(path.join(probe, "clock-state.json"), "utf8"),
  );
}
function saved() {
  return JSON.parse(fs.readFileSync(path.join(profile, "settings.json")))
    .extensions["at365.watch"].settings;
}
function alive(pid) {
  try {
    process.kill(pid, 0);
    return true;
  } catch {
    return false;
  }
}
async function launch() {
  application = await electron.launch({
    executablePath:
      packagedExecutable ?? require(path.join(host, "node_modules/electron")),
    args: packagedExecutable
      ? [`--test-profile=${profile}`]
      : [host, `--test-profile=${profile}`],
    env: { ...process.env, APPDOCK_WATCH_TEST_OUTPUT: probe },
    timeout: 30000,
  });
  const page = await application.firstWindow();
  await page.getByRole("heading", { name: "Welcome to your Dock." }).waitFor();
  return page;
}
async function command(page, id) {
  await page.keyboard.press("Control+p");
  await page
    .locator(`[data-command-id="at365.watch.${id}"] .palette-execute`)
    .click();
  await page
    .getByRole("status")
    .filter({ hasText: "コマンドを実行" })
    .waitFor();
}
async function save(page) {
  await page.getByRole("button", { name: "保存", exact: true }).click();
  await page.getByText("すべて保存されています", { exact: true }).waitFor();
}
(async () => {
  try {
    let page = await launch();
    await page
      .getByRole("button", { name: "Appletを管理", exact: true })
      .click();
    await page
      .getByRole("button", { name: "Applet.Watch.at365 C# / .NET 10" })
      .click();
    await page
      .getByRole("switch", { name: "Applet.Watch.at365を有効にする" })
      .click();
    await page.getByRole("heading", { name: "Your desktop clock." }).waitFor();
    const initial = await until(
      () => state().visible && state(),
      "Clock did not show",
    );
    assert.match(initial.text, /^\d{2}:\d{2}:\d{2}$/);
    assert.equal(initial.styles & 0x080000a0, 0x080000a0);
    assert.equal(initial.showInTaskbar, false);
    assert.equal(initial.showActivated, false);
    assert.match(
      initial.font,
      /Applet.Watch.at365;component\/Resources\/#Haettenschweiler/,
    );
    await until(() => state().text !== initial.text, "Clock did not tick");
    await until(
      () => fs.existsSync(path.join(probe, "clock.png")),
      "Clock capture missing",
    );
    await until(() => {
      fs.copyFileSync(
        path.join(probe, "clock.png"),
        path.join(profile, "clock-default.png"),
      );
      return true;
    }, "Clock capture stayed locked");
    const snap = await page.evaluate(() => window.dock.snapshot());
    const applet = snap.extensions.find((e) => e.id === "at365.watch");
    assert.equal(applet.commands.length, 3);
    assert.deepEqual(applet.tray, []);
    await page.screenshot({ path: path.join(profile, "applet.png") });
    await command(page, "hide");
    await until(() => !state().visible, "Hide did not apply");
    assert.equal(saved().visible, false);
    await command(page, "show");
    await until(() => state().visible, "Show did not apply");
    assert.equal(saved().visible, true);
    await page.keyboard.press("Control+,");
    await page.getByRole("button", { name: "Applet設定", exact: true }).click();
    await page.getByLabel("時計の位置", { exact: true }).selectOption("bottom");
    await page.getByLabel("時計の文字サイズ", { exact: true }).fill("120");
    await page.getByLabel("時計の不透明度", { exact: true }).fill("0.65");
    await page.getByLabel("上下の余白", { exact: true }).fill("15");
    await page.getByLabel("左右の余白", { exact: true }).fill("35");
    await page.getByRole("switch", { name: "秒を表示", exact: true }).click();
    await save(page);
    await until(
      () => state().opacity === 0.65 && !state().showSeconds,
      "Settings did not apply without restart",
    );
    const changed = state();
    assert.equal(changed.processId, initial.processId);
    assert.ok(
      Math.abs(
        changed.top -
          (changed.monitor.Top +
            changed.monitor.Height -
            changed.height -
            Math.round((15 * changed.dpi) / 96)),
      ) <= 2,
    );
    const options = applet.settingOptions.monitor.filter(
      (o) => o.value !== "primary",
    );
    for (const option of options) {
      await page
        .getByLabel("表示するモニター", { exact: true })
        .selectOption(option.value);
      await save(page);
      const current = await until(
        () => state().monitor.Id === option.value && state(),
        "Monitor selection did not apply",
      );
      assert.equal(current.left, current.monitor.Left);
      assert.equal(current.width, current.monitor.Width);
    }
    await page.getByLabel("時計の不透明度", { exact: true }).fill("2");
    await page.getByRole("button", { name: "保存", exact: true }).click();
    await page.getByRole("alert").filter({ hasText: "不透明度" }).waitFor();
    assert.equal(saved().opacity, 0.65);
    await page.getByRole("button", { name: "再読み込み", exact: true }).click();
    await page
      .getByRole("heading", { name: "Applet.Watch.at365", exact: true })
      .evaluate((element) => element.scrollIntoView({ block: "start" }));
    await page.screenshot({ path: path.join(profile, "settings.png") });
    await page
      .getByRole("button", { name: "ショートカット", exact: true })
      .click();
    await page
      .getByLabel("時計の表示を切り替えのショートカット 1", { exact: true })
      .press("Control+Alt+w");
    await save(page);
    await page
      .getByRole("button", { name: "ホーム", exact: true })
      .first()
      .click();
    await page.keyboard.press("Control+Alt+w");
    await until(() => !state().visible, "Native shortcut did not hide");
    assert.equal(saved().visible, false);
    const pid = state().processId;
    await application.close();
    application = undefined;
    await until(() => !alive(pid), "Clock survived host shutdown");
    page = await launch();
    const restored = await until(
      () => state().processId !== pid && state(),
      "Clock did not reconnect",
    );
    assert.equal(restored.visible, false);
    assert.equal(restored.opacity, 0.65);
    await page.waitForFunction(() =>
      window.dock
        .snapshot()
        .then(
          (s) =>
            s.extensions.find((e) => e.id === "at365.watch")?.state ===
            "running",
        ),
    );
    await page.keyboard.press("Control+p");
    await page
      .locator('[data-command-id="at365.watch.toggle"] .palette-execute')
      .waitFor();
    await page.keyboard.press("Escape");
    await page.keyboard.press("Control+Alt+w");
    await until(() => state().visible, "Restored shortcut did not show");
    await page
      .getByRole("button", { name: "Appletを管理", exact: true })
      .click();
    await page
      .getByRole("button", { name: "Applet.Watch.at365 C# / .NET 10" })
      .click();
    await page.getByRole("button", { name: "再起動", exact: true }).click();
    const restarted = await until(
      () => state().processId !== restored.processId && state(),
      "Applet did not restart",
    );
    await until(
      () => !alive(restored.processId),
      "Previous clock process survived restart",
    );
    await page
      .getByRole("switch", { name: "Applet.Watch.at365を有効にする" })
      .click();
    await until(
      () => !alive(restarted.processId),
      "Clock survived Applet disable",
    );
    console.log(
      JSON.stringify(
        {
          ok: true,
          profile,
          monitorsChecked: options.length,
          checks: [
            "native WPF rendering / embedded font / ticking",
            "click-through and no-activation styles / no taskbar or Applet tray",
            "show / hide persistence",
            "live settings / bottom placement / connected monitor selection",
            "numeric validation preserves settings",
            "native command shortcuts / restart persistence",
            "shutdown / restart / disable clean up clock process",
          ],
        },
        null,
        2,
      ),
    );
  } finally {
    await application?.close();
  }
})().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
