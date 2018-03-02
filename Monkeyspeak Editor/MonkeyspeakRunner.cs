using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor
{
    public static class MonkeyspeakRunner
    {
        private static MonkeyspeakEngine engine = new MonkeyspeakEngine()
        {
            Options = new Options()
            {
                TriggerLimit = int.MaxValue
            }
        };

        private static Page page = null;

        [Browsable(false)]
        public static Page CurrentPage
        {
            get
            {
                if (page == null)
                {
                    page = new Page(Engine);
                    page.LoadAllLibraries();
                }
                return page;
            }
        }

        public static Options Options => Engine.Options;

        [Browsable(false)]
        public static MonkeyspeakEngine Engine { get => engine; }

        public static void WarmUp()
        {
            var page = CurrentPage;
        }

        public static Page LoadFile(string filePath)
        {
            page = Engine.LoadFromFile(filePath);
            page.LoadAllLibraries();
            return page;
        }

        public static Page LoadString(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return CurrentPage;
            page = Engine.LoadFromString(code);
            page.LoadAllLibraries();
            return page;
        }

        public static void Run(int id = 0)
        {
            page.Execute(id);
        }

        public static bool Compile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            try
            {
                if (page != null)
                {
                    page.CompileToFile(Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}.msx"));
                    page.LoadAllLibraries();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return false;
        }
    }
}