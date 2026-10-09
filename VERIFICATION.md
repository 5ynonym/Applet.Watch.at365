# 検証記録

## 2026-10-10: 開発生成物を`.artifacts`へ改名

- ユーザー指定でartifacts→.artifactsを改名。移動直後に既存751項目の相対パス/size/mtime/ディレクトリ・リンク属性が一致し、検証終了時も元の全項目のsize/mtime/属性が不変。配布物4ファイルのSHA256も検証前後で一致。保存済みログ/JSONは内部パスを含めて保持し、過去記録の当repoのartifacts/は.artifacts/へ読み替える。
- テスト/開発用の参照とGit除外/開発手順を更新。6repo合計の変更CJS18件の構文、Gmail start-dev.ps1の構文/UTF-8 BOM、各repoのgit diff --checkが成功。旧artifactsの再生成なし、新.artifactsのGit除外を確認。
- 既存.NET回帰8/8。scripts/test-ui.cjsは旧Welcome to your Dock.見出し待ちでTimeout（現行UIにない）。製品・旧試験の意味を変更せず記録し、別の隔離確認で開始/3コマンド登録/停止成功（.artifacts/rename-startstop-1791569715736/result.json）。
- ログは.artifacts/rename-20261010-regression.log。Gmail/Wallpaper/Watchの元の統合試験ログは.artifacts/rename-20261010-integration.log。残りの開始/停止確認の再現スクリプト/ログはA:/XX.TEMP/applets-artifacts-rename-startstop-20261010.cjsと同.log。確認スクリプトのsnapshot非同期取得/待機の途中失敗は修正し、最終は4件すべて終了0。棚卸し/最終照合はA:/XX.TEMP/applets-artifacts-rename-20261010-{before,after,final}.json。
- 製品実装は変更せず、manifest版0.1.2と既存publishを保持。再発行/commit/push/Release/実利用deployなし。同期・バックアップ設定はユキちゃんが担当。今回の成功した新規profileは各方式で直近3回以下、古い証跡は使用終了/再利用要否を一括確定していないため保持し削除0。

## 2026-10-09: v0.1.2 ショートカット初期値

- 既存の実行時登録と同じ3コマンドをmanifestへ宣言し、時計切替のグローバルPauseを`defaultKeybindings`へ追加。AppDock本体のWatch固有既定値は除去。Appletの実行コードは変更なし。
- 最初の並列`publish.bat`はWPF生成ファイルの競合で終了1、単独再実行は終了0。`publish/update.json`はid `at365.watch`/版0.1.2、最終`update.zip`は69922837 bytes/SHA256 `8928ba1d5155d9af8c1b2ed8e47791b410df090ec0f242b2b0a65c545344de96`。AppDock 0.25.2の全体ZIPに同梱し、隔離起動後のsettingsでPause/global条件を確認。実時計表示・実利用先deploy・個別GitHub公開は未実施。

## 2026-10-09: 更新配布物の自動生成

- `codex/update-packages`で発行スクリプトだけを更新。Applet本体の版は0.1.1を維持し、`publish.bat`終了コード0。共通パッカーはAppDock 0.23.0のソースから発行。
- `publish/update.json`のID・版をmanifestと照合し、ZIPのサイズ69922712bytesとSHA256 `ed9187ac51aff4fbde7f22e2cecc8564ff6c0ab9871555bc5c7750ea4a51fb67`を照合。ZIP内2ファイルすべてを通常発行フォルダーとバイト単位で比較し一致。収録: `Applet.Watch.at365.exe`, `extension.json`。
- 旧SDK/旧DLLの生成物が残るWatch・WindowMover・WindowsToolsでは、既存deployと一致する配布内容へ整理する処理を追加。任意のユーザーファイルの再帰削除は行わない。
- 共通検証結果はAppDockの`artifacts/applet-update-packages.json`、発行ログは`artifacts/Applet.Watch.at365-update-publish.log`。実利用先deploy・外部公開・pushは未実施。実GitHub/HTTP(S)/UNC配布先の確認はユーザーが後で行う。Applet固有機能・実アカウント操作の再試験は今回の発行変更の対象外。

## 2026-10-08: 実利用先へのdeploy

- 配置後の実利用について、ユーザーが正常動作を確認したと報告（2026-10-08）。

- ユーザーの明示指示により、AppDockと全6Appletの`deploy.bat`を引数なしで実行し、7件すべて終了コード0。配置先は`A:\00.ESSENTIAL\00.MainTools\AppDock.at365`。5つの.NET Appletは現ソース/SDKで`publish.bat`を先に実行し、Gmailはdeploy内で再発行した。
- AppDock0.16.2、Gmail0.5.1、WallpaperSlideshow0.3.0、Watch0.1.1（native）、WebBrowserTools0.2.4、WindowMover0.2.1、WindowsTools0.1.1を配置。Watchの古いDLL版manifestを配置せず、現ソースのnative版へ更新。
- 配置対象21ファイルのSHA256はすべて発行元と一致。現ソースと配置manifestの版/runtime/entry、minimumHostVersionも照合。settings.json・avatar.png・Gmail accounts.jsonの3ファイルは配置前後のハッシュ不変。
- 配置前後とも関連プロセスなし。実利用アプリは起動していないため、次回起動で反映する。旧ファイル退避は行わず、設定・認証領域を配置スクリプトで変更していない。結果は`../AppDock.at365/artifacts/deploy-2026-10-08-result.json`（本体では`artifacts/deploy-2026-10-08-result.json`）。

## 2026-10-08: 依存パッケージ確認

- 外部NuGet PackageReferenceなし。slnxの`dotnet list package --outdated`も更新なし。依存定義や製品コード・版の変更は不要。参照するAppDockのnpm更新詳細は[本体検証記録](../AppDock.at365/VERIFICATION.md)を参照。
- 現AppDock SDK/RuntimeでRelease build警告0/エラー0、既存RegressionTests成功。実アプリ/ハードウェアに作用するnative検証、publish/deployは今回実施していない。

2026-10-05 JST、Windows x64 / .NET 10.0.401 / AppDock.at365 v0.3.0で確認。元Watchのソース・設定・プロセスと、実利用のAppDock設定は変更していません。

| 検証 | 結果 |
| --- | --- |
| Release build / self-contained単一EXE publish | 成功、警告0・エラー0 |
| モニター選択・負座標・DPI・上下配置・範囲補正の回帰テスト | 8件成功 |
| WPF描画・埋め込みフォント・秒の更新 | 成功、描画PNGを目視確認 |
| クリック透過・非アクティブ化・タスクバー非表示 | ウィンドウスタイルと属性を確認 |
| Appletのトレイ項目 | 0件 |
| 表示・非表示・切り替えコマンド | 成功、AppDockのsettings.jsonへ保存 |
| モニター・下端・余白・文字サイズ・不透明度・秒表示の編集 | 再起動せずに反映 |
| 接続中モニターの選択 | 2台成功、各モニターの座標・画面幅を確認 |
| 不正な設定値の拒否 | 成功、保存済み設定を保持 |
| コマンドへのCtrl+Alt+W割り当て | 成功、再起動後も保持・実行 |
| AppDock終了・Applet再起動・無効化 | 成功、旧時計プロセスの終了を確認 |
| 開発版・ビルド済みAppDockでのUIテスト | 両方成功 |
| portable AppDock EXEとの接続 | 成功、ホスト側smoke-result.jsonに記録 |

実機はメイン5120×2160とサブ3840×2160（左座標5120）、両方DPI 144です。負座標と異なるDPIでの配置計算は回帰テストで検証しました。物理的な接続解除・再接続、異なるDPIを持つ実機間の移動、RDP、スリープ復帰、長期常駐、別のクリーンPCでは未検証です。クリック透過はWindowsのスタイル・ヒットテスト処理を確認し、実マウスによる下側アプリの操作テストは行っていません。

成果物: `publish/Applet.Watch.at365/Applet.Watch.at365.exe`、75,492,678 bytes。EXEとextension.jsonの2ファイルで配置します。未署名。

SHA256: `08808ACC84CF3F725F63E7C2AD0E71247FD36BD4F43F765A691DC0492B59397F`。

埋め込みHatten.ttfは元WatchとSHA256一致: `40E898E471FA4DE3CA09A6DFED961D00D6395AF20FE6CF1C6B83C795BEA04543`。

開発版UI記録は `artifacts/ui-1791209433885/`、ビルド済みAppDockの最終UI記録は `artifacts/ui-1791210165097/`。テストごとに専用フォルダーのsettings.jsonを使い、`clock-state.json`、時計・Applet画面・設定画面のPNGを残します。ホスト側portable接続記録は `../AppDock.at365/artifacts/smoke-1791209958264/smoke-result.json`。
