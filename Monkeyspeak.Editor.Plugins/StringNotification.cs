﻿using Monkeyspeak.Editor.Interfaces.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Plugins
{
    internal class StringNotification : INotification
    {
        private readonly TextBlock content;
        private Color foreground = Colors.White, background = Colors.Black;

        public StringNotification(string content)
        {
            this.content = new TextBlock
            {
                Text = content,
                TextWrapping = System.Windows.TextWrapping.WrapWithOverflow,
            };
        }

        public StringNotification(string content, Color foreground, Color background = default(Color))
        {
            this.content = new TextBlock
            {
                Text = content,
                TextWrapping = System.Windows.TextWrapping.WrapWithOverflow,
            };
        }

        public object Content => content;
    }
}