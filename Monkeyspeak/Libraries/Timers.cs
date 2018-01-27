﻿using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Monkeyspeak.Libraries
{
    /// <summary>
    /// A TimerTask object contains Timer and Page Owner.  Timer is not started from a TimerTask constructor.
    /// </summary>
    internal sealed class TimerTask : IEquatable<TimerTask>
    {
        public double Interval { get; set; }

        public double Delay { get; set; }
        public bool FirstRun { get; set; }

        public System.Timers.Timer Timer { get; set; }

        public Page Owner { get; set; }

        public double Id { get; set; }

        /// <summary>
        /// Timer task that executes (0:300) when it triggers
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="interval">Interval in Seconds</param>
        /// <param name="id"></param>
        public TimerTask(Page owner, double interval, double id, double delay = 0)
        {
            Id = id;
            this.Owner = owner;
            this.Interval = interval;
            Delay = delay;
            FirstRun = true;
            Timer = new System.Timers.Timer(TimeSpan.FromSeconds(Interval).TotalMilliseconds)
            {
                AutoReset = true
            };
            Timer.Elapsed += (_, args) => timer_Elapsed(this);
            Timer.Start();
        }

        public void Start()
        {
            Timer.Start();
        }

        public void Stop()
        {
            Timer.Stop();
        }

        public void Dispose()
        {
            Timer.Stop();
            Timer.Dispose();
        }

        private static void timer_Elapsed(object sender)
        {
            try
            {
                TimerTask timerTask = (TimerTask)sender;
                if (timerTask.Timer.Enabled)
                {
                    if (timerTask.FirstRun && timerTask.Delay > 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(timerTask.Delay));
                        timerTask.FirstRun = false;
                        timerTask.Stop(); // fixes timer offset due to Delay bug
                        timerTask.Start();
                    }
                    Timers.CurrentTimer = timerTask.Id;
                    //while (!timerTask.Owner.CanExecute) Thread.Sleep(100);
                    timerTask.Owner.Execute(300, timerTask.Id);
                    Timers.CurrentTimer = 0;
                }
            }
            catch
            {
                // Eat the exception.. yummy!
            }
        }

        public bool Equals(TimerTask other)
        {
            return other.Id == Id;
        }
    }

    public class Timers : AutoIncrementBaseLibrary
    {
        private static DateTime startTime = DateTime.Now;
        internal static double CurrentTimer;

        private static readonly object lck = new object();
        private static readonly List<TimerTask> timers = new List<TimerTask>();

        private uint timersLimit;

        public override int BaseId => 300;

        public static TimeZoneInfo TimeZone
        {
            get => timeZone;
            set
            {
                if (value == default(TimeZoneInfo))
                    timeZone = TimeZoneInfo.Local;
                else timeZone = value;
            }
        }

        /// <summary>
        /// Default Timer Library.
        /// </summary>
        public Timers() : this(10)
        {
            // needed for reflection based loading of libraries
        }

        /// <summary>
        /// Default Timer Library.
        /// </summary>
        public Timers(uint timersLimit = 10, TimeZoneInfo timeZoneInfo = default(TimeZoneInfo))
        {
            if (timersLimit == 0) timersLimit = 10;
            if (timeZoneInfo == default(TimeZoneInfo)) timeZoneInfo = TimeZoneInfo.Local;
            timeZone = timeZoneInfo;
            this.timersLimit = timersLimit;
        }

        public override void Initialize(params object[] args)
        {
            // (0:300) When timer # goes off,
            Add(TriggerCategory.Cause, WhenTimerGoesOff,
                "when timer # goes off,");

            // (1:300) and timer # is running,
            Add(TriggerCategory.Condition, AndTimerIsRunning,
                "and timer # is running,");
            // (1:301) and timer # is not running,
            Add(TriggerCategory.Condition, AndTimerIsNotRunning,
                "and timer # is not running,");

            // (5:300) create timer # to go off every # second(s).
            Add(TriggerCategory.Effect, CreateTimer,
                "create timer # to go off every # second(s) with a start delay of # second(s).");

            // (5:301) stop timer #.
            Add(TriggerCategory.Effect, StopTimer,
                "stop timer #.");

            Add(TriggerCategory.Effect, GetCurrentTimerIntoVar,
                "get current timer and put the id into variable %.");

            Add(TriggerCategory.Effect, PauseScriptExecution,
                "pause script execution for # seconds.");

            Add(TriggerCategory.Effect, GetCurrentUpTimeIntoVar,
                "get the current uptime and put it into variable %.");

            Add(TriggerCategory.Effect, GetCurrentHourIntoVar,
                "get the current hour and put it into variable %.");

            Add(TriggerCategory.Effect, GetCurrentMinutesIntoVar,
                "get the current minutes and put it into variable %.");

            Add(TriggerCategory.Effect, GetCurrentSecondsIntoVar,
                "get the current seconds and put it into variable %.");

            Add(TriggerCategory.Effect, GetCurrentDayOfMonthIntoVar,
                "get the current day of the month and put it into variable %.");

            Add(TriggerCategory.Effect, GetCurrentMonthIntoVar,
                "get the current month and put it into variable %.");

            Add(TriggerCategory.Effect, GetCurrentYearIntoVar,
                "get the current year and put it into variable %.");

            Add(TriggerCategory.Effect, StartTimer,
                "start timer #.");

            Add(TriggerCategory.Effect, SetTheTimeZone,
                "set the time zone to {...}");

            Add(TriggerCategory.Effect, GetUTCTimeZone,
                "get univeral time zone and put it into variable %");

            Add(TriggerCategory.Effect, GetAllTimeZones,
                "get the available time zones and put it into table %");
        }

        [TriggerDescription("Gets the universal time zone and puts it into a variable")]
        [TriggerVariableParameter]
        private bool GetUTCTimeZone(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = TimeZoneInfo.Utc.Id;
            return true;
        }

        [TriggerDescription("Gets all time zones available to the system and puts them into a table")]
        [TriggerVariableParameter]
        private bool GetAllTimeZones(TriggerReader reader)
        {
            var table = reader.ReadVariableTable(true);
            try
            {
                foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                    table.Add(tz.DisplayName, tz.Id);
            }
            catch (Exception ex)
            {
                Logger.Debug<Timers>(ex);
                reader.Page.RemoveVariable(table);
                return false;
            }
            return true;
        }

        private static TimeZoneInfo timeZone = TimeZoneInfo.Local;

        [TriggerDescription("Sets the time zone.  Any triggers before this will use the time zone set by the application.")]
        [TriggerStringParameter]
        private bool SetTheTimeZone(TriggerReader reader)
        {
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(reader.ReadString());
            }
            catch (Exception ex)
            {
                Logger.Debug<Timers>(ex);
                timeZone = TimeZoneInfo.Local;
                return false;
            }
            return true;
        }

        [TriggerDescription("Gets the current year and puts it into a variable")]
        [TriggerVariableParameter]
        private bool GetCurrentYearIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone).Year.AsDouble();
            return true;
        }

        [TriggerDescription("Gets the current month and puts it into a variable")]
        [TriggerVariableParameter]
        private bool GetCurrentMonthIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone).Month.AsDouble();
            return true;
        }

        [TriggerDescription("Gets the current day of the month and puts it into a variable")]
        [TriggerVariableParameter]
        private bool GetCurrentDayOfMonthIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone).Day.AsDouble();
            return true;
        }

        [TriggerDescription("Gets the current seconds and puts it into a variable")]
        [TriggerVariableParameter]
        private bool GetCurrentSecondsIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone).TimeOfDay.Seconds.AsDouble();
            return true;
        }

        [TriggerDescription("Gets the current minutes and puts it into a variable")]
        [TriggerVariableParameter]
        private bool GetCurrentMinutesIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone).TimeOfDay.Minutes.AsDouble();
            return true;
        }

        [TriggerDescription("Gets the current hour and puts it into a variable")]
        [TriggerVariableParameter]
        private bool GetCurrentHourIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone).TimeOfDay.Hours.AsDouble();
            return true;
        }

        [TriggerDescription("Gets the application uptime and puts it into a variable")]
        [TriggerVariableParameter]
        private bool GetCurrentUpTimeIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = System.Math.Round((TimeZoneInfo.ConvertTime(DateTime.Now, timeZone) - startTime).TotalSeconds);
            return true;
        }

        [TriggerDescription("Pauses script execution")]
        private bool PauseScriptExecution(TriggerReader reader)
        {
            double duration = reader.ReadNumber();
            reader.Page.CanExecute = false;
            reader.Page.CanExecute = false;
            Thread.Sleep(TimeSpan.FromSeconds(duration > 0 ? duration : 0));
            reader.Page.CanExecute = true;
            return true;
        }

        [TriggerDescription("Gets the triggered timer id and puts it into a variable")]
        [TriggerVariableParameter]
        private bool GetCurrentTimerIntoVar(TriggerReader reader)
        {
            if (reader.Parameters.Length > 0)
            {
                var var = reader.ReadVariable(true);
                var.Value = reader.GetParameter<double>(0);
                return true;
            }
            return false;
        }

        internal static void DestroyTimer(TimerTask task)
        {
            lock (lck)
            {
                task.Dispose();
                timers.Remove(task);
            }
        }

        [TriggerDescription("Gets whether the timer is not running")]
        [TriggerNumberParameter]
        private bool AndTimerIsNotRunning(TriggerReader reader)
        {
            return !AndTimerIsRunning(reader);
        }

        [TriggerDescription("Gets whether the timer is running")]
        [TriggerNumberParameter]
        private bool AndTimerIsRunning(TriggerReader reader)
        {
            if (!TryGetTimerFrom(reader, out TimerTask timerTask))
                return false;
            return timerTask.Timer.Enabled;
        }

        [TriggerDescription("Creates a timer with the specified id")]
        [TriggerNumberParameter]
        private bool CreateTimer(TriggerReader reader)
        {
            if (timers.Count >= timersLimit)
            {
                throw new MonkeyspeakException("The amount of timers has exceeded the limit of {0}", timersLimit);
            }
            double id = reader.ReadNumber();

            double interval = reader.ReadNumber();

            reader.TryReadNumber(out double delay);

            Logger.Debug<Timers>($"id={id} interval={TimeSpan.FromSeconds(interval)} delay = {delay}");
            if (interval <= 0) return false;
            if (id <= 0) return false; // NOTE no more timers with id 0, must be > 0

            lock (lck)
            {
                var existing = timers.FirstOrDefault(task => task.Id == id);
                if (existing != null)
                {
                    existing.Dispose();
                    timers.Remove(existing);
                }
                var timerTask = new TimerTask(reader.Page, interval, id, delay > 0 ? delay : 0);
                timers.Add(timerTask);
            }

            return true;
        }

        [TriggerDescription("Starts the timer")]
        [TriggerNumberParameter]
        private bool StartTimer(TriggerReader reader)
        {
            if (TryGetTimerFrom(reader, out TimerTask timer))
            {
                timer.Start();
                return true;
            }
            return false;
        }

        [TriggerDescription("Stops the timer")]
        [TriggerNumberParameter]
        private bool StopTimer(TriggerReader reader)
        {
            if (TryGetTimerFrom(reader, out TimerTask timer))
            {
                timer.Stop();
                return true;
            }
            return false;
        }

        private bool TryGetTimerFrom(TriggerReader reader, out TimerTask timerTask)
        {
            double num = reader.ReadNumber();

            if (num > 0)
            {
                timerTask = timers.FirstOrDefault(task => task.Id == num);
                return timerTask != null;
            }
            timerTask = null;
            return false;
        }

        [TriggerDescription("Triggered when a timer goes off with the specified id")]
        [TriggerNumberParameter]
        private bool WhenTimerGoesOff(TriggerReader reader)
        {
            if (TryGetTimerFrom(reader, out TimerTask timerTask))
            {
                return timerTask.Id == reader.GetParameter<double>();
            }
            return false;
        }

        public override void Unload(Page page)
        {
            lock (lck)
            {
                foreach (var task in timers.ToArray())
                    DestroyTimer(task);
            }
        }
    }
}