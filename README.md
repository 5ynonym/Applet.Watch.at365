# Applet.Watch.at365

AppDock **0.10.0以降**に時計と日付を提供する、.NET 10のDLL Appletです。ホームへのピン留めと透過デスクトップ表示に対応し、元WatchのHatten.ttfを使います。描画・日時更新・表示ウィンドウはAppDock本体が担当します。

## 使い方

Appletを有効にして、AppDockの「ウィジェット」を開きます。「時計」「日付」をそれぞれホームにピン留めしたり、デスクトップに表示したりできます。

- 各ウィジェットにモニター、前面／デスクトップに固定、9方向のアンカー／自由位置、幅・高さ・文字サイズ・不透明度・色を設定できます。
- 「ドラッグで移動」で一時的な移動バーを表示し、「完了」で保存します。「取消」・Esc・2分経過でキャンセルします。
- 通常表示はクリックを透過し、フォーカスを奪いません。Applet停止時にはデスクトップ表示を解放し、保存済み配置とホームのピン留めは残ります。
- Applet設定の「秒を表示」は時計の内容に反映します。

既存コマンド`at365.watch.show`、`.hide`、`.toggle`とPauseの既定キーは維持しています。コマンドは時計と日付のデスクトップ表示をまとめて操作し、ホームのピン留めには影響しません。

0.1.xからの初回移行では、表示状態・日付の有無・上下アンカー・余白・文字サイズ・不透明度を新しい配置の既定値に取り込みます。その後はウィジェットページで保存した配置を優先します。旧設定は削除しません。旧モニターDeviceNameと新しい画面IDは異なるため、以前サブモニターを使っていた場合はウィジェットページで選び直してください。

## ビルド・配置

```powershell
dotnet build Applet.Watch.at365.slnx -c Release
dotnet run --project Applet.Watch.RegressionTests -c Release
.\publish.bat
.\deploy.bat "C:\Tools\AppDock.at365"
```

`publish.bat`の第1引数には任意の発行先、`deploy.bat`の第1引数にはAppDock.at365.exeがあるフォルダーを指定できます。配置先は引数、`deploy.local.txt`の先頭行、PowerShellスクリプトの既定値の順です。実利用先への配置時はAppDockを完全終了してください。

発行物は`publish/Applet.Watch.at365/`に生成します。配置に必要なファイルは次の4つです。

```text
extensions/Applet.Watch.at365/
├─ extension.json
├─ Applet.Watch.at365.dll
├─ Applet.Watch.at365.deps.json
└─ Resources/Hatten.ttf
```

DLLはAppDockの既存.NETホストがロードします。Applet自身のEXE・WPF・同梱.NETランタイムは不要です。発行・配置スクリプトは旧Watch EXEだけを除去して、DLL形式に更新します。独自ウィンドウを必要とするほかのAppletは引き続きnative EXEを使えます。

## 検証

```powershell
node scripts/test-ui.cjs
node scripts/test-ui.cjs ../AppDock.at365/publish/win-unpacked/AppDock.at365.exe
```

UIテストはAppDockのウィジェット検証を実行し、隔離profileにこのDLLを配置します。グローバルNodeは不要で、ホストの`.tools/node/24.21.0/node.exe`でも実行できます。実利用のAppDock設定、元Watchのプロセス、壁紙、マウス位置を変更しません。検証範囲と未確認項目は[VERIFICATION.md](VERIFICATION.md)を参照してください。

ウィジェットSDKとホストの構造は[AppDockのウィジェット開発ガイド](../AppDock.at365/docs/widgets.md)に記載しています。
