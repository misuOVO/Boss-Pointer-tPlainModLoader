using System;
using System.IO;
using Newtonsoft.Json;
using tContentPatch;

namespace BossCursor
{
    /// <summary>
    /// 对应原模组的 Config（客户端侧）。
    /// 配置保存在 Mods\BossCursor\config.json，设置页改动即时保存。
    /// </summary>
    public class CursorConfig
    {
        public bool Enabled = true;

        public bool HideOnScreen = false;

        public int CursorDistance = 150;

        public float CursorSize = 1f;

        public float CursorOpacity = 0.9f;

        public bool BlackListPillars = true;

        /// <summary>开关光标的按键名（Microsoft.Xna.Framework.Input.Keys），例如 F8、B、OemOpenBrackets。</summary>
        public string ToggleKey = "F8";

        // ------------------------------------------------------------------

        private static string PathFile
        {
            get
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods", "BossCursor");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "config.json");
            }
        }

        public static CursorConfig Instance { get; set; } = new CursorConfig();

        public static void ResetToDefault()
        {
            Instance = new CursorConfig();
        }

        public static void Load()
        {
            try
            {
                string path = PathFile;
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    CursorConfig loaded = JsonConvert.DeserializeObject<CursorConfig>(json);
                    if (loaded != null)
                        Instance = loaded;
                }
                else
                {
                    Save();
                }

                Log.Write("BossCursor 配置已加载: " + path);
            }
            catch (Exception e)
            {
                Log.Write("BossCursor 配置加载失败，使用默认值: " + e.Message);
                Instance = new CursorConfig();
            }
        }

        public static void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
                File.WriteAllText(PathFile, json);
            }
            catch (Exception e)
            {
                Log.Write("BossCursor 配置保存失败: " + e.Message);
            }
        }
    }
}
