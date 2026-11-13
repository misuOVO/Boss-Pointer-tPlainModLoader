using System;
using Terraria.UI;
using tContentPatch;
using tContentPatch.Content.UI;
using tContentPatch.Content.UI.ModSet;

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
            item.SetVal(get());
            item.OnValUpdate = v =>
            {
                set(v);
                Commit();
            };
            return item;
        }

        private UIItemValueSlider Slider(string label, float min, float max, Func<float> get, Action<float> set, bool isInt = false)
        {
            UIItemValueSlider item = isInt
                ? new UIItemValueSlider((int)min, (int)max, null, label)
                : new UIItemValueSlider(min, max, null, label);

            item.FloatToString = v => isInt ? v.ToString("0") : v.ToString("0.0");

            item.SetVal(get());
            item.OnValUpdate = v =>
            {
                set(isInt ? (float)Math.Round(v) : (float)Math.Round(v, 1));
                Commit();
            };
            return item;
        }

        public override UIElement GetUI()
        {
            UIScrollViewer2 root = new UIScrollViewer2();
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

            UIItemTextBox keyBox = new UIItemTextBox(M.ToggleKey, 24, null, "开关按键（如 F8 / B）");
            keyBox.OnLostFocus = () =>
            {
                string text = keyBox.GetText();
                if (KeyUtil.TryParse(text, out _))
                {
                    M.ToggleKey = text.Trim().ToUpperInvariant();
                    Commit();
                }
                else
                {
                    keyBox.SetText(M.ToggleKey);
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
