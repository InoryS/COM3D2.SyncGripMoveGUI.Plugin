[English](#english) | [简体中文](#%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87) | [日本語](#%E6%97%A5%E6%9C%AC%E8%AA%9E)

# English
# COM3D2.SyncGripMoveGUI.Plugin
 COM3D2 Sync [COM3D2.GripMovePlugin's](https://ux.getuploader.com/scarletkom_mod/download/45) "OLD GUI" with offical tablet GUI, avoid the annoying white pointer.

In VR, the original game UI is placed on a floating tablet, where you can operate.

But using GripMove's "Old GUI" option always introduces the desktop mode pointer, which cannot be easily hidden, so I synchronized GripMove's "Old GUI" switch with the tablet's display and hiding.

This way, you won't see the pointer when you hide the tablet.


BepinEx Plugin, place DLLs into your BepinEx/plugins folder.

1. When the official tablet is turned off, the "old GUI" is also turned off.
2. The "old GUI" is turned on only when the tablet is turned on and the controller is DIRECT (1.0.6.0).
3. ~~Press G+I (default) can turn "old GUI" off.~~ (not 1.0.8.0 and after)


| official tablet status | 	Is GripMove's Direct Mode Active | Is GripMove's "old GUI" Show (result)  |
| :---  |  :---- |   :--- |
|false|	false|	false|
|false|	true|	false|
|true|	false|	false|
|true|	true|	true|


<br>

# 简体中文
COM3D2 插件：将 [COM3D2.GripMovePlugin](https://ux.getuploader.com/scarletkom_mod/download/45) 插件的 “旧 GUI” 模式与官方平板电脑 GUI 同步，避免烦人的白色指针。

在 VR 环境中，原有的游戏 UI 界面会被放置到一个悬浮的平板电脑上，你可以在上面操作。

但使用 GripMove 的 “旧GUI” 选项时时会引入桌面模式的指针，这个指针不能方便的隐藏，所以我将 GripMove 的 “旧GUI” 开关与平板电脑的显示和隐藏同步。

这样隐藏平板电脑时就不会看到指针了。


### 使用

BepinEx 插件，将 DLL 放入 BepinEx/plugins 文件夹中。

1. 在官方平板电脑关闭时，同样关闭 “旧GUI”.
2. 仅在平板电脑开启，且手柄为 DIRECT 时，才开启 “旧GUI” (1.0.6.0).
3. ~~按 G+I（默认）可以关闭 “旧GUI”。~~ (1.0.8.0 及之后取消)

| 官方平板电脑状态 | 是否处于 GripMove 插件的 Direct 操作模式 | GripMove 插件的旧 GUI 是否展示（结果）  |
| :---        |    :----   |          :--- |
|false|	false|	false|
|false|	true|	false|
|true|	false|	false|
|true|	true|	true|




<br>

# 日本語
AI translate

COM3D2 プラグイン: [COM3D2.GripMovePlugin](https://ux.getuploader.com/scarletkom_mod/download/45) の「旧GUI」モードを公式タブレットGUIと同期させ、煩わしい白いカーソルを回避するプラグイン。

VR環境では、ゲームの既存のUIは浮遊するタブレット上に配置され、そこから操作できます。

しかし、GripMove の「旧GUI」オプションを使用すると、デスクトップモードのカーソルが表示される問題があり、このカーソルを簡単に隠すことができません。そのため、GripMove の「旧GUI」スイッチをタブレットの表示・非表示と同期させました。

これにより、タブレットが非表示のときにカーソルも表示されなくなります。

### 使用方法

BepInExプラグインであり、DLLを BepInEx/plugins フォルダに配置してください。

1. 公式タブレットがオフのとき、「旧GUI」もオフになります。
2. タブレットがオンで、かつコントローラーが DIRECT モードのときのみ、「旧GUI」がオンになります。（1.0.6.0）
3. ~~G+I（デフォルト）で「旧GUI」をオフにする機能を削除。~~（1.0.8.0以降）

| 公式タブレットの状態 | GripMoveプラグインの Direct 操作モードか | GripMoveプラグインの旧GUIの表示（結果） |
| :---        |    :----   |          :--- |
|false|	false|	false|
|false|	true|	false|
|true|	false|	false|
|true|	true|	true|

