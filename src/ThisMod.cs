using System;
using tContentPatch;
using tContentPatch.ModLoad;
using tContentPatch.Patch;

namespace BossCursor
{
    /// <summary>
    /// tPlainModLoader 入口：Boss Cursor 移植版。
    ///
    /// 原模组在玩家周围画箭头指向 Boss，箭头大小/透明度随距离变化；
    /// 打开大地图时隐藏；支持距离/大小/隐藏屏幕内 Boss 等配置。
    /// </summary>
    public class ThisMod : Mod
    {
        public override void Load(ModObject mo)
        {
            Log.Write("BossCursor.Load key=" + (mo != null && mo.config != null ? mo.config.key : "?"));

            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                    Log.Write("!!! BossCursor 未处理异常: " + e.ExceptionObject);
            }
            catch
            {
            }

            CursorConfig.Load();
        }

        public override void AddPatch(IAddPatch addPatch)
        {
            CursorHooks.Apply(addPatch);
        }

        public override void Loaded()
        {
            Log.Write("BossCursor.Loaded");
        }
    }
}
