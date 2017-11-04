namespace Monkeyspeak.Libraries
{
    public class Debug : BaseLibrary
    {
        public override void Initialize(params object[] args)
        {
            //(0:10000) when a debug breakpoint is hit,
            Add(TriggerCategory.Cause, 10000, WhenBreakpointHit,
                "when a debug breakpoint is hit,");

            //(5:10000) create a debug breakpoint here,
            Add(TriggerCategory.Effect, 10000, CreateBreakPoint,
                "create a debug breakpoint here,");
        }

        public override void Unload(Page page)
        {
        }

        private bool CreateBreakPoint(TriggerReader reader)
        {
            if (System.Diagnostics.Debugger.Launch())
            {
                System.Diagnostics.Debugger.NotifyOfCrossThreadDependency();
                System.Diagnostics.Debugger.Break();
                reader.Page.Execute(10000);
            }
            else RaiseError("Debugger could not be attached.");

            return true;
        }

        private bool WhenBreakpointHit(TriggerReader reader)
        {
            return true;
        }
    }
}