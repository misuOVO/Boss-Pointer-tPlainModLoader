# boss指针 — tPlainModLoader 移植

把 tModLoader 模组 **Boss Cursor**（作者 kgoyo，GPL-3.0）移植到 **tPlainModLoader / Terraria 1.4.6.1**，版本 1.0.1。

原项目：https://github.com/kgoyo/BossCursor

## 功能

- 在玩家周围显示指向每个 Boss 的**箭头**；
- 箭头随距离变化：越远越小越淡，越近越大越清晰；
- 箭头内侧显示 **Boss 头像**；
- 打开大地图（覆盖地图）时自动隐藏；
- 重力翻转（重力药水）时箭头方向同步翻转；
- **快捷键开关**（默认 F8，可在设置里自定义）。

## 设置（游戏内「模组设置 → boss指针」）

| 选项 | 默认 | 说明 |
|---|---|---|
| 显示 Boss 光标 | 开 | 总开关 |
| 隐藏已出现在屏幕内的 Boss | 关 | 只给屏幕外的 Boss 画箭头 |
| 屏蔽四柱 | 开 | 不给日耀/星云/星旋/星尘柱画箭头 |
| 开关按键 | F8 | 自定义快捷键，填 `Keys` 枚举名（F8、B、OemOpenBrackets…） |
| 距离玩家 | 150 | 箭头离玩家多远（像素） |
| 箭头大小 | 1.0 | 缩放 |
| 不透明度 | 0.9 | 箭头/头像透明度 |

修改即时生效并写入 `Mods\BossCursor\config.json`。

## 安装

把下面 4 个文件放到 `<Terraria>/Mods/BossCursor/`：

```
BossCursor.dll
info.json
loadConfig.json
ico.png
```

## 构建

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

需要 .NET SDK（用自带 Roslyn）与游戏目录 `D:\steam\steamapps\common\Terraria`。

## 实现说明

tPlainModLoader 没有 `ModSystem` / `UIState` / `ModConfig`，所以：

- **绘制**：Harmony 挂在 `Main.DrawInterface_25_ResourceBars`，用 SpriteBatch 直接画；
- **坐标**：世界 → 屏幕像素 → UI 坐标（用 `GameViewMatrix.Translation/Zoom` 反算，再除以 `UIScale`），任何缩放/分辨率下箭头都贴着玩家；
- **Boss 判定**：`NPC.GetBossHeadTextureIndex() != -1`（与原模组一致），并跳过 `dontCountMe`；
- **快捷键**：读 `Microsoft.Xna.Framework.Input.Keys` 枚举名，聊天/告示牌输入时不响应；
- **设置页**：继承 `tContentPatch.ModSetting`，用加载器自带的开关、滑条、文本框、滚动容器。

## 许可 / 致谢

- 原模组 **Boss Cursor** by **kgoyo**：https://github.com/kgoyo/BossCursor ，**GPL-3.0**；
- 本项目沿用 GPL-3.0（见 `LICENSE`），箭头贴图 `assets/Cursor.png`、图标均来自原项目。
