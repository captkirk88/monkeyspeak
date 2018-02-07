using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Monkeyspeak.Editor.HelperClasses
{
    public delegate void SettingChangedHandler(string property, object value);

    public class Settings
    {
        public static event Action Saving;

        public static event SettingChangedHandler SettingChanged;

        private static Dictionary<string, object> dictionary = new Dictionary<string, object>();

        [Browsable(false)]
        public static Dictionary<string, object> Dictionary => dictionary;

        static Settings()
        {
            RememberWindowPosition = false;
            WindowSizeWidth = 800;
            WindowSizeHeight = 600;
            WindowPositionX = (SystemParameters.WorkArea.Width - WindowSizeWidth) / 2 + SystemParameters.WorkArea.Left;
            WindowPositionY = (SystemParameters.WorkArea.Height - WindowSizeHeight) / 2 + SystemParameters.WorkArea.Top;
            WindowState = WindowState.Normal;
            Color = AppColor.Brown;
            Theme = AppTheme.Light;
            Intellisense = true;
            AutoOpenOnWarning = true;
            SyntaxCheckingEnabled = true;
            ShowWarnings = true;
            AutoCompileScriptsOnSave = false;
            SaveSession = true;
            LastSession = string.Empty;
            TriggerSplitterPosition = 250;
            ResetSplitterPosition = false;
#if DEBUG
            Debug = true;
#else
            Debug = false;
#endif
        }

        public static bool SaveSession
        {
            get
            {
                return ((bool)(dictionary["SaveSession"]));
            }
            set
            {
                dictionary["SaveSession"] = value;
                SettingChanged?.Invoke("SaveSession", value);
            }
        }

        public static string LastSession
        {
            get
            {
                return ((string)(dictionary["LastSession"]));
            }
            set
            {
                dictionary["LastSession"] = value;
                SettingChanged?.Invoke("LastSession", value);
            }
        }

        public static global::Monkeyspeak.Editor.AppColor Color
        {
            get
            {
                return ((AppColor)(dictionary["Color"]));
            }
            set
            {
                dictionary["Color"] = value;
                SettingChanged?.Invoke("Color", value);
            }
        }

        public static AppTheme Theme
        {
            get
            {
                return ((AppTheme)(dictionary["Theme"]));
            }
            set
            {
                dictionary["Theme"] = value;
                SettingChanged?.Invoke("Theme", value);
            }
        }

        public static bool RememberWindowPosition
        {
            get
            {
                return ((bool)(dictionary["RememberWindowPosition"]));
            }
            set
            {
                dictionary["RememberWindowPosition"] = value;
                SettingChanged?.Invoke("RememberWindowPosition", value);
            }
        }

        [Browsable(false)]
        public static double WindowPositionX
        {
            get
            {
                return (double)(dictionary["!WindowPositionX"]);
            }
            set
            {
                dictionary["!WindowPositionX"] = value;
                SettingChanged?.Invoke("!WindowPositionX", value);
            }
        }

        [Browsable(false)]
        public static double WindowPositionY
        {
            get
            {
                return (double)(dictionary["!WindowPositionY"]);
            }
            set
            {
                dictionary["!WindowPositionY"] = value;
                SettingChanged?.Invoke("!WindowPositionY", value);
            }
        }

        [Browsable(false)]
        public static global::System.Windows.WindowState WindowState
        {
            get
            {
                return ((global::System.Windows.WindowState)(dictionary["!WindowState"]));
            }
            set
            {
                dictionary["!WindowState"] = value;
                SettingChanged?.Invoke("!WindowState", value);
            }
        }

        [Browsable(false)]
        public static double WindowSizeHeight
        {
            get
            {
                return ((double)(dictionary["!WindowSizeHeight"]));
            }
            set
            {
                dictionary["!WindowSizeHeight"] = value;
                SettingChanged?.Invoke("!WindowSizeHeight", value);
            }
        }

        [Browsable(false)]
        public static double WindowSizeWidth
        {
            get
            {
                return ((double)(dictionary["!WindowSizeWidth"]));
            }
            set
            {
                dictionary["!WindowSizeWidth"] = value;
                SettingChanged?.Invoke("!WindowSizeWidth", value);
            }
        }

        public static bool Intellisense
        {
            get
            {
                return ((bool)(dictionary["Intellisense"]));
            }
            set
            {
                dictionary["Intellisense"] = value;
                SettingChanged?.Invoke("Intellisense", value);
            }
        }

        public static bool ResetSplitterPosition
        {
            get
            {
                return ((bool)(dictionary["ResetSplitterPosition"]));
            }
            set
            {
                dictionary["ResetSplitterPosition"] = value;
                if (value == true)
                    TriggerSplitterPosition = -1d;
                SettingChanged?.Invoke("ResetSplitterPosition", value);
            }
        }

        [Browsable(false)]
        public static double TriggerSplitterPosition
        {
            get
            {
                return ((double)(dictionary["TriggerSplitterPosition"]));
            }
            set
            {
                dictionary["TriggerSplitterPosition"] = value;
                SettingChanged?.Invoke("TriggerSplitterPosition", value);
            }
        }

        public static bool Debug
        {
            get
            {
                return ((bool)(dictionary["Debug"]));
            }
            set
            {
                dictionary["Debug"] = value;
                Monkeyspeak.Logging.Logger.DebugEnabled = value;
                SettingChanged?.Invoke("Debug", value);
            }
        }

        public static bool ShowWarnings
        {
            get
            {
                return ((bool)(dictionary["ShowWarnings"]));
            }
            set
            {
                dictionary["ShowWarnings"] = value;
                SettingChanged?.Invoke("ShowWarnings", value);
            }
        }

        public static bool AutoOpenOnWarning
        {
            get
            {
                return ((bool)(dictionary["AutoOpenOnWarning"]));
            }
            set
            {
                dictionary["AutoOpenOnWarning"] = value;
                SettingChanged?.Invoke("AutoOpenOnWarning", value);
            }
        }

        public static bool AutoCompileScriptsOnSave
        {
            get
            {
                return ((bool)(dictionary["AutoCompileScriptsOnSave"]));
            }
            set
            {
                dictionary["AutoCompileScriptsOnSave"] = value;
                SettingChanged?.Invoke("AutoCompileScriptsOnSave", value);
            }
        }

        public static bool SyntaxCheckingEnabled
        {
            get
            {
                return ((bool)(dictionary["SyntaxCheckingEnabled"]));
            }
            set
            {
                dictionary["SyntaxCheckingEnabled"] = value;
                SettingChanged?.Invoke("SyntaxCheckingEnabled", value);
            }
        }

        public static void Save()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Monkeyspeak", "settings.yml");
            Saving?.Invoke();
            Utils.YAML.SerializeToFile(dictionary, path);
        }

        public static void Load()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Monkeyspeak", "settings.yml");
            var settings = Utils.YAML.DeserializeFromFile<Dictionary<string, object>>(path);
            if (settings != null)
            {
                foreach (var kv in settings)
                {
                    if (kv.Value != null)
                        dictionary[kv.Key] = kv.Value;
                }
            }
        }
    }
}