using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Commands;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;

using System.Linq;

namespace Monkeyspeak.Editor.HelperClasses
{
    public class HotKeyWithDefault : HotKey
    {
        private Key defaultKey;
        private ModifierKeys defaultModifierKeys;

        public HotKeyWithDefault(Key key, ModifierKeys modifierKeys = ModifierKeys.None, Key defaultKey = Key.None, ModifierKeys defaultModifierKeys = ModifierKeys.None) : base(key, modifierKeys)
        {
            this.defaultKey = defaultKey == Key.None ? key : defaultKey;
            this.defaultModifierKeys = defaultModifierKeys == ModifierKeys.None ? modifierKeys : defaultModifierKeys;
        }

        public ModifierKeys DefaultModifierKeys { get => defaultModifierKeys; set => defaultModifierKeys = value; }
        public Key DefaultKey { get => defaultKey; set => defaultKey = value; }

        public HotKeyWithDefault RevertChanges()
        {
            return new HotKeyWithDefault(defaultKey, defaultModifierKeys);
        }
    }

    public static class HotkeyManager
    {
        public static ConcurrentDictionary<BaseCommand, HotKeyWithDefault> Defaults = new ConcurrentDictionary<BaseCommand, HotKeyWithDefault>();

        static HotkeyManager()
        {
            CreateDefaultHotKeyEntry(MonkeyspeakCommands.New, Key.N, ModifierKeys.Control);
            CreateDefaultHotKeyEntry(MonkeyspeakCommands.Open, Key.O, ModifierKeys.Control);
            CreateDefaultHotKeyEntry(MonkeyspeakCommands.Save, Key.S, ModifierKeys.Control);
            CreateDefaultHotKeyEntry(MonkeyspeakCommands.SaveAll, Key.S, ModifierKeys.Control | ModifierKeys.Alt);
            CreateDefaultHotKeyEntry(MonkeyspeakCommands.Close, Key.X, ModifierKeys.Control);
            CreateDefaultHotKeyEntry(MonkeyspeakCommands.Exit, Key.F4, ModifierKeys.Control | ModifierKeys.Alt);
            CreateDefaultHotKeyEntry(MonkeyspeakCommands.Completion, Key.Space, ModifierKeys.Control);
            Load();
        }

        internal static void Populate(MetroWindow window, SplitContainer hotkeysContainer, StackPanel panel1, StackPanel panel2)
        {
            foreach (var kv in Defaults)
            {
                StackPanel container = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                Label labelCtrl = new Label() { Content = kv.Key.Name };
                HotKeyBox hotkey = new HotKeyBox
                {
                    HotKey = kv.Value
                };
                Button changeHotKey = new Button
                {
                    Content = "Change"
                };
                changeHotKey.Click += async delegate
                {
                    await GeneratePrompt(window, kv.Key, hotkey);
                };

                Button revertHotKey = new Button
                {
                    Content = "Default"
                };
                revertHotKey.Click += (sender, e) =>
                {
                    hotkey.HotKey = ((HotKeyWithDefault)hotkey.HotKey).RevertChanges();
                    ApplyChange(kv.Key, hotkey.HotKey as HotKeyWithDefault);
                };
                panel1.Children.Add(labelCtrl);
                container.Children.Add(hotkey);
                container.Children.Add(changeHotKey);
                container.Children.Add(revertHotKey);

                panel2.Children.Add(container);
            }
        }

        private static void CreateDefaultHotKeyEntry(BaseCommand command, Key key, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            Defaults.TryAdd(command, new HotKeyWithDefault(key, modifierKeys));
        }

        private static async Task GeneratePrompt(MetroWindow window, BaseCommand command, HotKeyBox hotkeyBox)
        {
            CustomDialog popup = new CustomDialog
            {
                Title = "Change Hotkey?"
            };
            StackPanel view = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            TextBox keyInput = new TextBox();
            keyInput.Height = 32;
            TextBoxHelper.SetAutoWatermark(keyInput, true);
            TextBoxHelper.SetWatermark(keyInput, "Type keys then press enter");
            Keyboard.Focus(keyInput);

            keyInput.PreviewKeyDown += async (sender, e) =>
            {
                if (e.SystemKey != Key.None) return;
                if (e.Key == Key.Return)
                {
                    var oldDefaults = hotkeyBox.HotKey as HotKeyWithDefault;
                    try
                    {
                        KeyGestureConverter keyGestureConverter = new KeyGestureConverter();
                        var gesture = (KeyGesture)keyGestureConverter.ConvertFromInvariantString(keyInput.Text);

                        hotkeyBox.HotKey = new HotKeyWithDefault(gesture.Key,
                            gesture.Modifiers,
                            oldDefaults.DefaultKey, oldDefaults.DefaultModifierKeys);

                        ApplyChange(command, hotkeyBox.HotKey as HotKeyWithDefault);
                    }
                    catch { }
                    await DialogManager.HideMetroDialogAsync(window, popup);
                }

                if (!e.IsRepeat)
                {
                    string keys = "";
                    if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
                    {
                        keys += "Control+";
                    }
                    if ((Keyboard.Modifiers & ModifierKeys.Alt) > 0)
                    {
                        keys += "Alt+";
                    }
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0)
                    {
                        keys += "Shift+";
                    }

                    keys += e.Key;
                    keyInput.Text = keys;
                }
                e.Handled = true;
            };

            Button cancelButton = new Button() { Content = "Cancel" };
            cancelButton.Height = 32;
            cancelButton.Click += async (sender, e) => await DialogManager.HideMetroDialogAsync(window, popup);
            view.Children.Add(keyInput);
            view.Children.Add(cancelButton);
            popup.Content = view;

            await DialogManager.ShowMetroDialogAsync(window, popup);
        }

        private static void ApplyChange(BaseCommand command, HotKeyWithDefault hotkey)
        {
            Defaults[command] = hotkey;
        }

        public static void ApplyChangesToInputBindings()
        {
            var bindings = Application.Current.MainWindow.InputBindings;
            bindings.Clear();
            foreach (var kv in Defaults)
            {
                bindings.Add(new KeyBinding(kv.Key, kv.Value.Key, kv.Value.ModifierKeys));
            }
        }

        public static void Save()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "monkeyspeak");
            string filePath = Path.Combine(dir, "keybindings.conf");
            if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var kv in Defaults)
                {
                    string keys = string.Empty;
                    var hotkey = kv.Value;
                    if (hotkey.ModifierKeys != ModifierKeys.None)
                    {
                        foreach (var modifier in hotkey.ModifierKeys.GetUniqueFlags<ModifierKeys>())
                            keys += $"{modifier}+";
                    }
                    keys += hotkey.Key;
                    writer.WriteLine($"{kv.Key.GetType().ToString()}={keys}");
                }
            }
        }

        public static void Load()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "monkeyspeak");
            string filePath = Path.Combine(dir, "keybindings.conf");
            if (File.Exists(filePath) == false) return;

            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    Type commandType = Type.GetType(line.LeftOf('=').Trim());
                    string keyStr = line.RightOf('=').Trim();
                    KeyGestureConverter keyGestureConverter = new KeyGestureConverter();
                    var keyGesture = (KeyGesture)keyGestureConverter.ConvertFromString(keyStr);
                    var command = (BaseCommand)typeof(MonkeyspeakCommands).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).FirstOrDefault(prop => prop.FieldType.Equals(commandType))?.GetValue(null);
                    if (command != null)
                    {
                        Defaults.TryGetValue(command, out var oldHotKey);
                        var hotkey = new HotKeyWithDefault(keyGesture.Key, keyGesture.Modifiers, oldHotKey != null ? oldHotKey.Key : Key.None, oldHotKey != null ? oldHotKey.ModifierKeys : ModifierKeys.None);
                        if (Defaults.ContainsKey(command))
                            Defaults[command] = hotkey;
                        else Defaults.TryAdd(command, hotkey);
                    }
                }
            }
        }
    }
}