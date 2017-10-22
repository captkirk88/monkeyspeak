using Monkeyspeak.Logging;
using System;
using System.Windows.Forms;

namespace Monkeyspeak.Editor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Options opts = new Options();
            var engine = new MonkeyspeakEngine();
            var page = engine.LoadFromFile("testBIG.ms");

            page.LoadSysLibrary();
            page.LoadIOLibrary();
            page.LoadStringLibrary();
            page.LoadMathLibrary();
            page.LoadTimerLibrary();
            page.TriggerAdded += (trigger, handler) => Logger.Info($"{trigger} = {handler.Target.GetType().Name}");

            page.Execute(0);
            using (var app = new Eto.Forms.Application(Eto.Platform.Detect))
                app.Run(new MainWindow());
        }
    }
}