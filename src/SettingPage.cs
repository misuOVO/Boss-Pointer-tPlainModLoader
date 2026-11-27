using System;
using Terraria.UI;
using tContentPatch;
using tContentPatch.Content.UI;
using tContentPatch.Content.UI.Item1;

namespace BossCursor
{
    /// <summary>
    /// 游戏内“模组设置”页面。
    /// </summary>
    public class BossCursorSetting : ModSetting
    {
        public override string Name => "BossCursor";
        public override string Title => "boss指针";
        public override string FilePath => "config.json";
        public override bool HasUI => true;
        public override Type DataType => typeof(CursorConfig);

        private static CursorConfig M => CursorConfig.Instance;

        public override void SetDefault()
        {
            CursorConfig.ResetToDefault();
        }

        public override void Load(object v)
        {
            if (v is CursorConfig cfg)
                CursorConfig.Instance = cfg;
        }

        public override object GetSaveData()
        {
            return M;
        }

        private void Commit()
        {
            NeedSave = true;
            Save();
        }

        private UIItemSwitch Switch(string label, Func<bool> get, Action<bool> set)
        {
            UIItemSwitch item = new UIItemSwitch(null, label);
            item.OnGetIsChecked = get;
            item.OnSetIsChecked = v =>
            {
                set(v);
                Commit();
            };
            return item;
        }

        private UIElement Slider(string label, float min, float max, Func<float> get, Action<float> set, bool isInt = false)
        {
            if (isInt)
            {
                UIItemValueSliderInt item = new UIItemValueSliderInt((int)min, (int)max, null, label);
                item.ValueToString = v => v.ToString("0");
                item.OnGetValue = () => (int)Math.Round(get());
                item.OnSetValue = v =>
                {
                    set(v);
                    Commit();
                };
                return item;
            }

            UIItemValueSliderFloat f = new UIItemValueSliderFloat(min, max, null, label);
            f.ValueToString = v => v.ToString("0.0");
            f.OnGetValue = get;
            f.OnSetValue = v =>
            {
                set((float)Math.Round(v, 1));
                Commit();
            };
            return f;
        }

        public override UIElement GetUI()
        {
            UIScrollViewer2 root = new UIScrollViewer2(false);
            root.Width.Precent = 1f;
            root.Height.Precent = 1f;
            root.ItemMargin = 4;

            UIStackPanel basic = new UIStackPanel
            {
                Width = { Precent = 1f },
                IsAutoUpdateSize = true
            };

            basic.Append(new UIItemTitle(null, "光标"));
            basic.Append(Switch("显示 Boss 光标", () => M.Enabled, v => M.Enabled = v));
            basic.Append(Switch("隐藏已出现在屏幕内的 Boss", () => M.HideOnScreen, v => M.HideOnScreen = v));
            basic.Append(Switch("屏蔽四柱（日耀/星云/星旋/星尘）", () => M.BlackListPillars, v => M.BlackListPillars = v));

            UIItemKeybinding keyBox = new UIItemKeybinding(null, "开关按键（点一下再按 F8 / B）");
            keyBox.OnGetBind = () => M.ToggleKey;
            keyBox.OnSetBind = v =>
            {
                if (string.IsNullOrEmpty(v))
                    return;

                if (KeyUtil.TryParse(v, out _))
                {
                    M.ToggleKey = v.Trim().ToUpperInvariant();
                    Commit();
                }
            };
            basic.Append(keyBox);

            basic.Append(new UIItemTitle(null, "外观"));
            basic.Append(Slider("距离玩家", 40f, 500f, () => M.CursorDistance, v => M.CursorDistance = (int)v, true));
            basic.Append(Slider("箭头大小", 0.1f, 2f, () => M.CursorSize, v => M.CursorSize = v));
            basic.Append(Slider("不透明度", 0.1f, 1f, () => M.CursorOpacity, v => M.CursorOpacity = v));

            root.AddChild(basic);

            return root;
        }
    }
}
