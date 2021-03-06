﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Interfaces.Notifications
{
    /// <summary>
    /// Critical notifications are not removable
    /// </summary>
    public interface ICriticalNotification : INotification
    {
    }
}