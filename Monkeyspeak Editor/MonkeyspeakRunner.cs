﻿using System;
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
        private static MonkeyspeakEngine engine = new MonkeyspeakEngine();
        private static Page page = new Page(Engine);

        [Browsable(false)]
        public static Page CurrentPage => page;

        public static Options Options => Engine.Options;

        public static MonkeyspeakEngine Engine { get => engine; }

        static MonkeyspeakRunner()
        {
            page.LoadAllLibraries();
        }

        public static void LoadString(string code)
        {
            page = Engine.LoadFromString(code);
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
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return false;
        }

        public static Page LoadCompiled(string filePath)
        {
            try
            {
                return Engine.LoadCompiledFile(filePath);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return null;
        }
    }
}