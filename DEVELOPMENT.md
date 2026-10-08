# Applet.Watch.at365 開発ガイド

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
