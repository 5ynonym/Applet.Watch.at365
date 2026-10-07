# 検証記録

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
## 2026-10-07: v0.2.0 ウィジェット/DLL化

- AppDock 0.10.0のIWidgetServiceを使用し、時計と日付を別IDで提供。独自WPFウィンドウ・EXE・Appletタイマーを除去。元のHatten.ttfをファイル同梱し、AppDockの共通描画へ移行。
- Release DLL build/publish成功、警告0・エラー0。`publish.bat`成功。既存の配置計算8件に移行・秒/日付opt-out・表示切替/配置保持・設定購読解放6件を加え、14/14成功。
- ホストの開発版/発行版隔離UIで、DLL実通信、初回の旧設定取り込み、保存配置の再登録/再起動後の保持、ホーム/デスクトップの独立性、秒更新、元フォント、2画面の独立配置（前面・背面）、共有透過画面、表示/非表示/Pauseと同じtoggle処理、Applet停止と再有効化を確認。管理ページ、ライト/ダーク、900pxのPNGを確認。
- 最終開発版: `../AppDock.at365/artifacts/widgets-ui-1791331213323`。最終win-unpacked版: `../AppDock.at365/artifacts/widgets-ui-1791331254254`。単一EXE+DLL smoke: `../AppDock.at365/artifacts/smoke-1791331259893/smoke-result.json`（ok=true）。
- 成果物: `publish/Applet.Watch.at365/Applet.Watch.at365.dll`、26,112 bytes、SHA256 `8CD9609C093965C55BBA7FB09B9D5A8D294C08DD10C9A1DEECE6AA71EA6F5466`。必要な配布ファイルはDLL、deps.json、extension.json、Resources/Hatten.ttf。フォントのSHA256は旧版と一致する`40E898E471FA4DE3CA09A6DFED961D00D6395AF20FE6CF1C6B83C795BEA04543`。
- プロジェクトのAppDock publish/extensionsへ配置し、4ファイルのハッシュ一致と旧EXE除去を確認。実利用先・元Watch・壁紙・マウス位置は変更していない。外部公開/pushなし。
- 物理マウスでのドラッグ/下側ウィンドウ操作、異なるDPI、実モニター着脱、Explorer強制再起動、RDP、実スリープ復帰、長期常駐は未確認。移動保存/取消は専用ウィンドウの座標をプログラムから変更して、実画面の完了/取消ボタンで検証。

冒頭の0.1.x検証記録は旧WPF/単一EXE版の履歴です。現在の成果物・検証範囲はこのv0.2.0節を参照してください。
