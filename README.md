# Applet.Watch.at365

Watch.at365のデスクトップ時計を、AppDock.at365用のAppletとして切り出したものです。C# / WPF製。元のHaettenschweilerフォント、半透明、時分・秒・日付の見た目を引き継ぎ、クリックを透過する、フォーカスを奪わないウィンドウとして表示します。

時計だけを扱います。マウスジェスチャー、ホットキーの登録、AutoLock、クリップボード履歴、タスクトレイ、ほかの機能を追加するための独自拡張機構は含みません。元のWatch.at365のソースと設定は変更していません。

次のAppletを作るときは、AppDock側の[Applet実装ガイド](../AppDock.at365/docs/applet-development.md)と[ホスト機能追加ガイド](../AppDock.at365/docs/host-development.md)を参照してください。時計で確認した設定購読、native接続、DPI、終了処理、UIテストの注意点もまとめています。

## 使い始める

AppDock.at365 **v0.3.0以降**が必要です。`publish/Applet.Watch.at365` のフォルダーを、AppDockのEXEの隣の `extensions` に配置してください。

```text
任意の配置先/
├─ AppDock.at365.exe
├─ settings.json
└─ extensions/
   └─ Applet.Watch.at365/
      ├─ extension.json
      └─ Applet.Watch.at365.exe
```

作成済みのAppDockプロジェクトの `publish` に配置する場合は、次のスクリプトも使えます。実行中のAppDockは先に終了してください。

```powershell
.\scripts\install-local.ps1
# 別の配置先へ
.\scripts\install-local.ps1 -AppDockDirectory 'C:\Tools\AppDock.at365'
```

同じ操作はプロジェクト直下のバッチからも実行できます。

```bat
publish.bat
deploy.bat
rem 別のAppDock配置先へ
deploy.bat "C:\Tools\AppDock.at365"
```

`deploy.local.txt` にAppDock本体があるフォルダーの絶対パスを1行で保存しておけば、引数なしの `deploy.bat` でもその配置先を使えます。明示的な引数がある場合はそちらが優先され、ファイルがない場合は既存の既定配置先を使います。

AppDockを起動し直し、「Applet」で **Applet.Watch.at365** を有効にすると時計が表示されます。新しく追加したAppletは既定では無効です。

「設定 → Applet設定 → Applet.Watch.at365」で、表示状態・モニター・上端／下端・左右／上下の余白・文字サイズ・不透明度・秒／日付表示を編集し、「保存」してください。変更は再起動なしで反映されます。

## コマンド

| 表示名 | コマンドID |
| --- | --- |
| 時計を表示 | `at365.watch.show` |
| 時計を非表示 | `at365.watch.hide` |
| 時計の表示を切り替え | `at365.watch.toggle` |

Ctrl+PのコマンドパレットとApplet画面のボタンから実行できます。AppDock **v0.3.1以降**では、`at365.watch.toggle` の既定キーは **Pause** です。Applet有効時は、ほかのアプリを操作中やAppDockのトレイ格納中にも時計の表示を切り替えられます。「設定 → ショートカット」でキーと「グローバル」を変更できます。ホットキーの登録・競合表示・解除はAppDockが担当します。元のWatchがPauseを使用している場合は、元Watchを終了するか割り当てを変更してください。既存の明示的なキー設定は保持されます。

表示／非表示コマンドで変更した状態も、AppDockの `settings.json` の `extensions.at365.watch.settings.visible` に保存します。Applet専用の設定ファイルは作りません。アバター等のホスト設定にも触れません。

モニターはWindowsのデバイス名で保存し、「メインモニター（自動）」も選べます。選択したモニターが未接続の場合はメインへ退避し、保存された選択は保持します。再接続・表示構成変更時に再配置します。余白と文字サイズの単位はDIPで、DPIに合わせて描画します。画面幅が足りない場合は文字を縮小し、上下の余白が大きすぎる場合も画面内に収めます。

AppDockの終了、Appletの無効化、再起動で時計ウィンドウを閉じます。ホストとの接続が失われた場合も、Appletプロセスを終了します。AppletはAppDockから起動する専用EXEです。直接起動する独立アプリとしての設定画面はありません。

## ビルドと検証

Windows x64 / .NET 10 SDKが必要です。隣の `AppDock.at365/dotnet` のSDKとRuntimeプロジェクトを参照します。

```powershell
dotnet build .\Applet.Watch.at365.slnx -c Release
dotnet run --project .\Applet.Watch.RegressionTests -c Release
.\scripts\publish.ps1
node .\scripts\test-ui.cjs
# ビルド済みのAppDock本体でも検証する場合
node .\scripts\test-ui.cjs ..\AppDock.at365\publish\win-unpacked\AppDock.at365.exe
```

実行環境はEXEに同梱しているため、利用するPCに.NETを別途入れる必要はありません。[.NETの単一ファイル配布](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview)を使用します。初回にWindowsの一時領域へランタイムを展開します。DPI設定はWPF向けの[PerMonitorV2マニフェスト](https://learn.microsoft.com/en-us/windows/win32/hidpi/setting-the-default-dpi-awareness-for-a-process)で行います。

実画面テストは隣のAppDockプロジェクトのPlaywrightとElectronを利用し、Appletの `artifacts` 内の専用設定で動作します。実利用の設定、元Watchのプロセス、マウス・クリップボード・ロック状態を操作しません。`APPDOCK_WATCH_TEST_OUTPUT` はテスト時だけ、時計自身の描画とウィンドウ状態を検証用フォルダーへ出す環境変数です。

フォントとアイコンは `Watch.at365/Watch` の既存リソースを複製して同梱しています。公開配布は行っていません。
