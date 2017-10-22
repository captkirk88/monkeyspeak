using Monkeyspeak.Extensions;
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
    internal sealed class TimerTask
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
                    timerTask.Owner.Execute(300, timerTask.Id);
                    Timers.CurrentTimer = 0;
                }
            }
            catch
            {
                // Eat the exception.. yummy!
            }
        }
    }

    // Changed from Internal to public in order to expose DestroyTimers() - Gerolkae
    public class Timers : BaseLibrary
    {
        private static DateTime startTime = DateTime.Now;
        internal static double CurrentTimer;

        private static readonly object lck = new object();
        private static readonly List<TimerTask> timers = new List<TimerTask>();

        private uint timersLimit;

        public Timers() : this(10)
        {
            // needed for reflection based loading of libraries
        }

        /// <summary>
        /// Default Timer Library.  Call static method Timers.DestroyTimers() when your application closes.
        /// </summary>
        public Timers(uint timersLimit = 10)
        {
            if (timersLimit == 0) timersLimit = 1;
            this.timersLimit = timersLimit;
        }

        public override void Initialize()
        {
            // (0:300) When timer # goes off,
            Add(new Trigger(TriggerCategory.Cause, 300), WhenTimerGoesOff,
                "when timer # goes off,");

            // (1:300) and timer # is running,
            Add(new Trigger(TriggerCategory.Condition, 300), AndTimerIsRunning,
                "and timer # is running,");
            // (1:301) and timer # is not running,
            Add(new Trigger(TriggerCategory.Condition, 301), AndTimerIsNotRunning,
                "and timer # is not running,");

            // (5:300) create timer # to go off every # second(s).
            Add(new Trigger(TriggerCategory.Effect, 300), CreateTimer,
                "create timer # to go off every # second(s) with a start delay of # second(s).");

            // (5:301) stop timer #.
            Add(new Trigger(TriggerCategory.Effect, 301), StopTimer,
                "stop timer #.");

            Add(new Trigger(TriggerCategory.Effect, 302), GetCurrentTimerIntoVar,
                "get current timer and put the id into variable %.");

            Add(new Trigger(TriggerCategory.Effect, 303), PauseScriptExecution,
                "pause script execution for # seconds.");

            Add(new Trigger(TriggerCategory.Effect, 304), GetCurrentUpTimeIntoVar,
                "get the current uptime and put it into variable %.");

            Add(new Trigger(TriggerCategory.Effect, 305), GetCurrentHourIntoVar,
                "get the current hour and put it into variable %.");

            Add(new Trigger(TriggerCategory.Effect, 306), GetCurrentMinutesIntoVar,
                "get the current minutes and put it into variable %.");

            Add(new Trigger(TriggerCategory.Effect, 307), GetCurrentSecondsIntoVar,
                "get the current seconds and put it into variable %.");

            Add(new Trigger(TriggerCategory.Effect, 308), GetCurrentDayOfMonthIntoVar,
                "get the current day of the month and put it into variable %.");

            Add(new Trigger(TriggerCategory.Effect, 309), GetCurrentMonthIntoVar,
                "get the current month and put it into variable %.");

            Add(new Trigger(TriggerCategory.Effect, 310), GetCurrentYearIntoVar,
                "get the current year and put it into variable %.");
        }

        private bool GetCurrentYearIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = DateTime.Now.Year.As<double>();
            return true;
        }

        private bool GetCurrentMonthIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = DateTime.Now.Month.As<double>();
            return true;
        }

        private bool GetCurrentDayOfMonthIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = DateTime.Now.Day.As<double>();
            return true;
        }

        private bool GetCurrentSecondsIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = DateTime.Now.TimeOfDay.Seconds.As<double>();
            return true;
        }

        private bool GetCurrentMinutesIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = DateTime.Now.TimeOfDay.Minutes.As<double>();
            return true;
        }

        private bool GetCurrentHourIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = DateTime.Now.TimeOfDay.Hours.As<double>();
            return true;
        }

        private bool GetCurrentUpTimeIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = System.Math.Round((DateTime.Now - startTime).TotalSeconds);
            return true;
        }

        private bool PauseScriptExecution(TriggerReader reader)
        {
            double delay = 0;
            if (reader.PeekVariable())
            {
                var var = reader.ReadVariable();
                delay = var.Value.As<double>();
            }
            else if (reader.PeekNumber())
            {
                delay = reader.ReadNumber();
            }
            Thread.Sleep(TimeSpan.FromSeconds(delay));
            return true;
        }

        private bool GetCurrentTimerIntoVar(TriggerReader reader)
        {
            if (CurrentTimer > 0)
            {
                var var = reader.ReadVariable(true);
                var.Value = CurrentTimer;
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

        private bool AndTimerIsNotRunning(TriggerReader reader)
        {
            return !AndTimerIsRunning(reader);
        }

        private bool AndTimerIsRunning(TriggerReader reader)
        {
            TimerTask timerTask;
            if (!TryGetTimerFrom(reader, out timerTask))
                return false;
            return timerTask.Timer.Enabled;
        }

        private bool CreateTimer(TriggerReader reader)
        {
            if (timers.Count >= timersLimit)
            {
                throw new MonkeyspeakException("The amount of timers has exceeded the limit of {0}", timersLimit);
            }
            double id = 0;
            if (reader.PeekVariable())
            {
                var var = reader.ReadVariable();
                id = var.Value.As<double>();
            }
            else if (reader.PeekNumber())
            {
                id = reader.ReadNumber();
            }

            double interval = 0;
            if (reader.PeekVariable())
            {
                var var = reader.ReadVariable();
                interval = var.Value.As<double>();
            }
            else if (reader.PeekNumber())
            {
                interval = reader.ReadNumber();
            }

            double delay = 0;
            if (reader.PeekVariable())
            {
                var var = reader.ReadVariable();
                delay = var.Value.As<double>();
            }
            else if (reader.PeekNumber())
            {
                delay = reader.ReadNumber();
            }

            if (interval <= 0) return false;
            if (id <= 0) return false; // NOTE no more timers with id 0, must be > 0

            lock (lck)
            {
                var timerTask = new TimerTask(reader.Page, interval, id, delay);
                var existing = timers.FirstOrDefault(task => task.Id == id);
                if (existing != null)
                {
#warning Replacing existing timer may cause any triggers dependent on that timer to behave differently
                    existing.Dispose();
                    timers.Add(timerTask);
                }
                else
                {
                    timers.Add(timerTask);
                }
            }

            return true;
        }

        private bool StopTimer(TriggerReader reader)
        {
            double num = 0;
            if (reader.PeekVariable())
            {
                var var = reader.ReadVariable();
                num = var.Value.As<double>();
            }
            else if (reader.PeekNumber())
            {
                num = reader.ReadNumber();
            }
            try
            {
                lock (lck)
                {
                    TimerTask task = timers.FirstOrDefault(timer => timer.Id == num);
                    task.Dispose();
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }
            return true;
        }

        private bool TryGetTimerFrom(TriggerReader reader, out TimerTask timerTask)
        {
            double num = 0;
            if (reader.PeekVariable())
            {
                var var = reader.ReadVariable();
                num = var.Value.As<double>();
            }
            else if (reader.PeekNumber())
            {
                num = reader.ReadNumber();
            }

            if (num > 0)
            {
                if (!timers.Any(task => task.Id == num))
                {
                    // Don't add a timer to the Dictionary if it don't
                    // exist. Just return a blank timer
                    // - Gerolkae
                    // no, must be null because it is a TryGetValue kind of method and those are the rules - Squizzle
                    timerTask = null;
                    return false;
                }
                timerTask = timers.FirstOrDefault(task => task.Id == num);
                return true;
            }
            timerTask = null;
            return false;
        }

        private bool WhenTimerGoesOff(TriggerReader reader)
        {
            if (TryGetTimerFrom(reader, out TimerTask timerTask))
            {
                return timerTask.Id == reader.GetParameter<double>(0);
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