using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using Terraria;

namespace BossCursor
{
    /// <summary>
    /// Harmony 挂点：
    ///   Player.Update(int)                 -> F8 快速开关
    ///   Main.DrawInterface_25_ResourceBars -> 绘制箭头（UI 层，spriteBatch 已 Begin）
    /// </summary>
    internal static class CursorHooks
    {
        private static Harmony _harmony;
        private static bool _applied;

        public static void Apply(tContentPatch.Patch.IAddPatch addPatch)
        {
            if (_applied)
                return;
            _applied = true;

            try
            {
                _harmony = new Harmony("BossCursor.Patches");

                MethodInfo update = typeof(Player).GetMethod(
                    "Update",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new[] { typeof(int) }, null);

                if (update != null)
                {
                    _harmony.Patch(update, postfix: new HarmonyMethod(
                        typeof(CursorHooks).GetMethod(nameof(PlayerUpdatePostfix),
                            BindingFlags.NonPublic | BindingFlags.Static)));
                    Log.Write("BossCursor: 已挂 Player.Update");
                }
                else
                {
                    Log.Write("BossCursor: 找不到 Player.Update(int)");
                }

                MethodInfo draw = typeof(Main).GetMethod(
                    "DrawInterface_25_ResourceBars",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, Type.EmptyTypes, null);

                if (draw == null)
                {
                    draw = typeof(Main).GetMethod(
                        "DrawInterface",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, new[] { typeof(Microsoft.Xna.Framework.GameTime) }, null);
                }

                if (draw != null)
                {
                    _harmony.Patch(draw, postfix: new HarmonyMethod(
                        typeof(CursorHooks).GetMethod(nameof(DrawPostfix),
                            BindingFlags.NonPublic | BindingFlags.Static)));
                    Log.Write("BossCursor: 已挂绘制点 " + draw.Name);
                }
                else
                {
                    Log.Write("BossCursor: 找不到绘制点");
                }
            }
            catch (Exception e)
            {
                Log.Write("BossCursor 挂载失败: " + e);
            }
        }

        private static void PlayerUpdatePostfix(Player __instance)
        {
            try
            {
                if (__instance == null || __instance.whoAmI != Main.myPlayer)
                    return;

                // 输入框 / 聊天 / 告示牌里打字时不响应热键
                if (Main.drawingPlayerChat || Main.editSign || Main.editChest || Main.blockInput)
                    return;

                Keys key = KeyUtil.Get(CursorConfig.Instance.ToggleKey);
                if (key == Keys.None)
                    return;

                if (Main.keyState.IsKeyDown(key) && Main.oldKeyState.IsKeyUp(key))
                {
                    CursorConfig cfg = CursorConfig.Instance;
                    cfg.Enabled = !cfg.Enabled;
                    CursorConfig.Save();
                    Log.Write("BossCursor: 光标 " + (cfg.Enabled ? "开启" : "关闭"));
                }
            }
            catch
            {
            }
        }

        private static void DrawPostfix()
        {
            try
            {
                CursorDraw.Draw(Main.spriteBatch);
            }
            catch (Exception e)
            {
                Log.Write("BossCursor 绘制异常: " + e);
            }
        }
    }
}
