# Applet.Watch.at365 開発ガイド

実装時のテスト選択、コミット前の必要回帰、リリース前のコミット/プッシュ確認と検証証跡の再利用は、[本体・Applet共通手順](../AppDock.at365/docs/development-workflow.md)に従います。この文書の試験コマンドは、その段階に応じて実行します。

利用方法は[README.md](README.md)、実測結果と未確認事項は[VERIFICATION.md](VERIFICATION.md)を参照してください。

## 必要な環境

Windows x64、.NET 10 SDK、隣接するAppDock.at365のソースが必要です。

## ビルド・配置

```powershell
dotnet build Applet.Watch.at365.slnx -c Release
dotnet run --project Applet.Watch.RegressionTests -c Release
.\publish.bat
.\deploy.bat "C:\Tools\AppDock.at365"
```

`publish.bat`の第1引数には任意の発行先、`deploy.bat`の第1引数にはAppDock.at365.exeがあるフォルダーを指定できます。配置先は引数、`deploy.local.txt`の先頭行、PowerShellスクリプトの既定値の順です。実利用先への配置時はAppDockを完全終了してください。

発行物は`publish/Applet.Watch.at365/`に生成します。配置に必要なファイルは次の2つです。

```text
extensions/Applet.Watch.at365/
├─ extension.json
└─ Applet.Watch.at365.exe
```

AppletはWPFで時計を描画するself-contained単一EXEです。AppDockはAppletの起動・停止・設定・コマンドを管理し、時計の描画、透過、クリック透過、モニター配置はApplet自身が担当します。配布先PCに.NETランタイムは不要です。

## 検証

```powershell
node scripts/test-ui.cjs
node scripts/test-ui.cjs ../AppDock.at365/publish/win-unpacked/AppDock.at365.exe
```

UIテストは隔離profileへApplet EXEを配置し、時計の描画・設定反映・コマンド・終了を検証します。グローバルNodeは不要で、ホストの`.tools/node/24.21.0/node.exe`でも実行できます。実利用のAppDock設定、元Watchのプロセス、壁紙、マウス位置を変更しません。検証範囲と未確認項目は[VERIFICATION.md](VERIFICATION.md)を参照してください。

Appletのnative実装とホストとの接続は、[AppDockのApplet実装ガイド](../AppDock.at365/docs/applet-development.md)を参照してください。

## 文書の更新

READMEには動作環境・導入・操作・設定・利用上の制約を記載します。開発環境・ビルド・テスト・発行・開発者用配置・実装の説明はこのファイル、実測結果と未検証事項はVERIFICATION.mdへ記載します。共通方針は[AppDockのドキュメント方針](../AppDock.at365/docs/documentation.md)を参照してください。

## 更新配布物の発行

`publish.bat`は通常の発行先を生成した後、兄弟のAppDockリポジトリにある`scripts/pack-applet-update.ps1`で`publish/update.json`と`publish/update.zip`を自動生成します。共通パッカーのビルドに.NET 10 SDKが必要です。Gmail以外のAppletは、このパッケージ生成のためにNode.jsを導入する必要はありません。

ZIP直下に`extension.json`と実行ファイル一式を置き、JSONにID・版・必要な本体版・ZIPのサイズとSHA256を記録します。`OutputDirectory`を指定できる発行スクリプトでも、指定先の配布内容を読み、更新用JSON/ZIPの出力先はこのリポジトリの`publish`です。通常配置用サブフォルダーへJSON/ZIPを混ぜず、`deploy.bat`の配置対象も増やしません。

Web配布やGitHub Releaseには同じ発行で生成したJSONとZIPを一緒に置き、JSONを最後に公開してください。ソースコードの自動生成ZIPは使用しません。発行スクリプトから外部公開は行いません。[共通更新仕様](../AppDock.at365/docs/updates.md)と[配布先の確認手順](../AppDock.at365/docs/update-checklist.md)を参照してください。

## 開発生成物の保存先

開発・テストの生成物は`.artifacts`へ保存します。2026-10-10に旧`artifacts`を中身を保持して改名しました。過去の検証記録内の当repoの`artifacts/`は`.artifacts/`へ読み替えてください。保存済みログ/JSONの内部パスは実行当時の値として保持しています。作業完了時の整理は[AppDockの共通手順](../AppDock.at365/DEVELOPMENT.md#作業完了時のテストフォルダー整理)に従い、実行中・状態不明・未解決の失敗記録・再利用する資料を保持します。

## 公開設定

外部公開項目はextension.jsonのsettings[].automationで宣言します。取得とrevision付き更新は[共通操作API](../AppDock.at365/docs/automation.md)を使い、Applet固有のAPIや本体側の許可一覧は追加しません。booleanのON/OFF/toggleはgenerateCommandsで明示生成します。設定反映は既存Settings.OnChangedを共用します。

GUI試験にscripts/settings-mcp-check.cjsによる隔離MCP確認を含みます。公開schema・読取り・更新、非公開項目/不正値の拒否、書込許可、dryRun、古いrevisionの拒否と、各Applet実プロセスへの反映を確認します。実利用Codex設定やモデルは使用しません。
