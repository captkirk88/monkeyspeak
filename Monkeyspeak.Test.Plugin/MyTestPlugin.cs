using Monkeyspeak.Editor;
using Monkeyspeak.Editor.Interfaces.Notifications;
using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Test.Plugin
{
    /// <summary>
    /// Colorizes the highlighted word Red
    /// </summary>
    /// <seealso cref="Monkeyspeak.Editor.Plugins.Plugin"/>
    public class MyTestPlugin : Editor.Plugins.Plugin
    {
        /// <summary>
        /// Initializes this plugin.
        /// </summary>
        /// <param name="notificationManager">
        /// The notification manager. Add your notifications or store the notificationManager
        /// somewhere to use later on.
        /// </param>
        public override void Initialize(INotificationManager notificationManager)
        {
            notificationManager.AddNotification(new MyTimedNotification());
        }

        public override void OnEditorSaveCompleted(IEditor editor)
        {
        }

        /// <summary>
        /// Called when [editor selection changed].
        /// </summary>
        /// <param name="editor">The editor.</param>
        public override void OnEditorSelectionChanged(IEditor editor)
        {
        }

        /// <summary>
        /// Called when [editor text changed].
        /// </summary>
        /// <param name="editor">The editor.</param>
        public override void OnEditorTextChanged(IEditor editor)
        {
            editor.SetTextColor(Colors.Red, editor.CaretLine, 0, editor.CurrentLine.Length - 1);
        }

        public override void Unload()
        {
        }
    }
}