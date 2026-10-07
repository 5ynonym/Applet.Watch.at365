# Applet.Watch.at365

Watch.at365のデスクトップ時計を、AppDock.at365用のAppletとして切り出したものです。C# / WPF製で、元のHaettenschweilerフォント、半透明、時分・秒・日付の見た目を引き継ぎます。クリックを透過し、フォーカスを奪わない時計専用ウィンドウとして表示します。

## 動作環境

Windows x64とAppDock.at365 **v0.3.0以降**が必要です。現行のAppDockでも利用できます。

## 導入・更新

1. AppDockをトレイの「終了」から完全終了します。
2. `publish/Applet.Watch.at365/` を、AppDock.at365.exeの隣の `extensions/Applet.Watch.at365/` に配置します。
3. AppDockを起動し、「Applet」一覧で **Applet.Watch.at365** を有効にします。

更新時も配布物一式を置き換えてください。AppDockの`settings.json`に保存済みの設定は維持されます。

```text
任意の配置先/
├─ AppDock.at365.exe
├─ settings.json
└─ extensions/
   └─ Applet.Watch.at365/
      ├─ extension.json
      └─ Applet.Watch.at365.exe
```

## 使い方と設定

「設定 → Applet設定 → Applet.Watch.at365」で、表示状態・モニター・上端／下端・左右／上下の余白・文字サイズ・不透明度・秒／日付表示を編集して保存します。変更は再起動せずに反映されます。

モニター未接続時はメインモニターへ退避し、選択は保存したまま保持します。余白と文字サイズはDIPで扱い、DPIに合わせて描画します。画面からはみ出す場合は時計を画面内に収めます。

時計はAppDock終了・Applet無効化・再起動時に閉じます。AppletはAppDockが起動・管理する専用EXEで、直接起動する独立した設定画面はありません。

## コマンド

| 表示名 | コマンドID |
| --- | --- |
| 時計を表示 | `at365.watch.show` |
| 時計を非表示 | `at365.watch.hide` |
| 時計の表示を切り替え | `at365.watch.toggle` |

Ctrl+PのコマンドパレットとApplet画面のボタンから実行できます。AppDock v0.3.1以降では、`at365.watch.toggle`の既定キーはPauseです。キーとグローバル登録は「設定 → ショートカット」から変更できます。

---

開発・ビルドについては[開発ガイド](DEVELOPMENT.md)を参照してください。
