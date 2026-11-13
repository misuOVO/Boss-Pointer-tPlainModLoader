using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace BossCursor
{
    /// <summary>
    /// 绘制：在玩家周围为每个 Boss 画一个指向箭头 + Boss 头像。
    /// 位置换算走「世界 → 屏幕像素 → UI 坐标」，因此缩放/分辨率都正确。
    /// </summary>
    internal static class CursorDraw
    {
        private const float HeadDistance = 20f;

        private static Texture2D _cursorTexture;

        private static Texture2D CursorTexture
        {
            get
            {
                if (_cursorTexture == null)
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    using (Stream s = asm.GetManifestResourceStream("BossCursor.Assets.Cursor.png"))
                    {
                        if (s == null)
                            return null;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            s.CopyTo(ms);
                            ms.Position = 0;
                            _cursorTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, ms);
                        }
                    }
                }
                return _cursorTexture;
            }
        }

        public static void Draw(SpriteBatch sb)
        {
            if (sb == null || Main.dedServ || Main.gameMenu)
                return;

            CursorConfig M = CursorConfig.Instance;
            if (!M.Enabled)
                return;

            // 大地图（覆盖地图）打开时隐藏
            if (Main.mapStyle == 2)
                return;

            Player p = Main.LocalPlayer;
            if (p == null || !p.active || p.dead)
                return;

            Texture2D cursorTex = CursorTexture;
            if (cursorTex == null)
                return;

            Vector2 playerPos = p.Center;
            Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc == null || !npc.active || npc.dontCountMe)
                    continue;

                if (IsBlacklisted(npc.type, M))
                    continue;

                int headIndex;
                try
                {
                    headIndex = npc.GetBossHeadTextureIndex();
                }
                catch
                {
                    continue;
                }

                if (headIndex < 0)
                    continue;

                // 屏幕内隐藏
                if (M.HideOnScreen)
                {
                    Vector2 bossUi = WorldToUI(npc.Center);
                    if (bossUi.X >= 0 && bossUi.Y >= 0 && bossUi.X <= screenSize.X && bossUi.Y <= screenSize.Y)
                        continue;
                }

                Vector2 bossVector = npc.Center - playerPos;
                bossVector.Y *= p.gravDir;      // 重力药水翻转

                float distance = bossVector.Length();
                if (distance < 1f)
                    continue;

                float modifier = Clamp(1.15f - 1f / (2f * Main.screenWidth) * distance, 0.02f, 1f);
                float alpha = modifier * M.CursorOpacity;
                float scale = modifier * 1.2f;

                bossVector.Normalize();

                // 箭头
                Vector2 arrowWorld = playerPos + bossVector * M.CursorDistance;
                Vector2 arrowUi = WorldToUI(arrowWorld);
                float rotation = (float)Math.Atan2(bossVector.Y, bossVector.X);

                sb.Draw(cursorTex, arrowUi, null, Color.White * alpha, rotation,
                    new Vector2(cursorTex.Width, cursorTex.Height) / 2f, 1.2f * M.CursorSize, SpriteEffects.None, 1f);

                // Boss 头像
                Texture2D headTex = GetHeadTexture(headIndex);
                if (headTex != null)
                {
                    Vector2 headWorld = playerPos + bossVector *
                        (M.CursorDistance - HeadDistance * Main.UIScale * M.CursorSize);
                    Vector2 headUi = WorldToUI(headWorld);

                    SpriteEffects effects = SpriteEffects.None;
                    try
                    {
                        effects = npc.GetBossHeadSpriteEffects();
                    }
                    catch
                    {
                    }

                    sb.Draw(headTex, headUi, null, Color.White * alpha, 0f,
                        new Vector2(headTex.Width, headTex.Height) * 0.5f, scale * M.CursorSize, effects, 0f);
                }
            }
        }

        private static bool IsBlacklisted(int type, CursorConfig M)
        {
            if (!M.BlackListPillars)
                return false;

            switch (type)
            {
                case NPCID.LunarTowerSolar:
                case NPCID.LunarTowerNebula:
                case NPCID.LunarTowerVortex:
                case NPCID.LunarTowerStardust:
                    return true;
                default:
                    return false;
            }
        }

        private static Texture2D GetHeadTexture(int index)
        {
            if (index < 0 || index >= TextureAssets.NpcHeadBoss.Length)
                return null;
            return TextureAssets.NpcHeadBoss[index].Value;
        }

        /// <summary>
        /// 世界坐标 → 界面坐标（与 UIScaleMatrix 一致）。
        /// 世界层绘制时用的矩阵是 ZoomMatrix：先平移 -Translation，再按 Zoom 缩放，
        /// 最后界面层还会乘 UIScale，所以这里反着除回去。
        /// </summary>
        private static Vector2 WorldToUI(Vector2 world)
        {
            Vector2 translation = Main.GameViewMatrix.Translation;
            Vector2 zoom = Main.GameViewMatrix.Zoom;

            Vector2 px = (world - Main.screenPosition - translation) * zoom;
            Vector2 ui = px / Main.UIScale;

            return ui;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value > max) return max;
            if (value < min) return min;
            return value;
        }
    }
}
