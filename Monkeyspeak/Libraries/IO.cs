using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Monkeyspeak.Logging;

namespace Monkeyspeak.Libraries
{
    /// <summary>
    /// Basic file operations
    /// </summary>
    /// <seealso cref="Monkeyspeak.Libraries.BaseLibrary"/>
    public class IO : BaseLibrary
    {
        private List<string> tempFiles = new List<string>();
        private string DefaultAuthorizedPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="IO"/> class.
        /// </summary>
        public IO() : this(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal))
        {
            // satisfies the page.LoadAllLibraries reflection usage.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IO"/> class.
        /// </summary>
        /// <param name="authorizedPath">The authorized path.</param>
        public IO(string authorizedPath = null)
        {
            if (!string.IsNullOrEmpty(authorizedPath)) DefaultAuthorizedPath = authorizedPath;
        }

        /// <summary>
        /// Initializes this instance. Add your trigger handlers here.
        /// </summary>
        /// <param name="args">
        /// Parametized argument of objects to use to pass runtime objects to a library at initialization
        /// </param>
        public override void Initialize(params object[] args)
        {
            // (1:200) and the file {...} exists,
            Add(TriggerCategory.Condition, 200, FileExists,
                "and the file {...} exists,");

            // (1:201) and the file {...} does not exist,
            Add(TriggerCategory.Condition, 201, FileNotExists,
                "and the file {...} does not exist,");

            // (1:202) and the file {...} can be read from,
            Add(TriggerCategory.Condition, 202, CanReadFile,
                "and the file {...} can be read from,");

            // (1:203) and the file {...} can be written to,
            Add(TriggerCategory.Condition, 203, CanWriteFile,
                "and the file {...} can be written to,");

            // (5:200) append {...} to file {...}.
            Add(TriggerCategory.Effect, 200, AppendToFile,
                "append {...} to file {...}.");

            // (5:201) read from file {...} and put it into variable %.
            Add(TriggerCategory.Effect, 201, ReadFileIntoVariable,
                "read from file {...} and put it into variable %.");

            // (5:202) delete file {...}.
            Add(TriggerCategory.Effect, 202, DeleteFile,
                "delete file {...}.");

            //(5:203) create file {...}.
            Add(TriggerCategory.Effect, 203, CreateFile,
                "create file {...}.");

            Add(TriggerCategory.Effect, 204, CreateTempFile,
                "create a temporary file and put the location into variable %");
        }

        [TriggerDescription("Creates a temporary file and puts the location into the specified variable")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        private bool CreateTempFile(TriggerReader reader)
        {
            var tempFileName = reader.ReadString();
            var tempFile = Path.GetTempFileName();
            tempFiles.Add(tempFile);

            var var = reader.ReadVariableAsConstant(true);
            var.SetValue(tempFile);
            return true;
        }

        /// <summary>
        /// Called when page is disposing or resetting.
        /// </summary>
        /// <param name="page">The page.</param>
        public override void Unload(Page page)
        {
            foreach (var tempFile in tempFiles)
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch (Exception ex) { Logger.Error<IO>($"Failed to remove temp file {tempFile}. {ex.Message}"); }
            }
        }

        [TriggerDescription("Adds text to the specified file or creates the file if it doesn't exist")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        private bool AppendToFile(TriggerReader reader)
        {
            string data = reader.ReadString();
            string file = reader.ReadString();

            using (var streamWriter = new StreamWriter(Path.Combine(DefaultAuthorizedPath, file), true))
            {
                streamWriter.WriteLine(data);
            }

            return true;
        }

        [TriggerDescription("Checks to see if the file can be read from")]
        [TriggerStringParameter]
        private bool CanReadFile(TriggerReader reader)
        {
            string file = reader.ReadString();
            try
            {
                using (var stream = File.Open(Path.Combine(DefaultAuthorizedPath, file), FileMode.Open, FileAccess.Read))
                {
                    return true;
                }
            }
            catch // (UnauthorizedAccessException ex)
            {
                return false;
            }
        }

        [TriggerDescription("Checks to see if the file can be written to")]
        [TriggerStringParameter]
        private bool CanWriteFile(TriggerReader reader)
        {
            string file = reader.ReadString();
            try
            {
                using (var stream = File.Open(Path.Combine(DefaultAuthorizedPath, file), FileMode.Open, FileAccess.Write))
                {
                    return true;
                }
            }
            catch // (UnauthorizedAccessException ex)
            {
                return false;
            }
        }

        [TriggerDescription("Creates the file or overwrites it if it already exists")]
        [TriggerStringParameter]
        private bool CreateFile(TriggerReader reader)
        {
            if (!reader.PeekString()) return false;
            string file = reader.ReadString();
            File.CreateText(Path.Combine(DefaultAuthorizedPath, file)).Close();
            return true;
        }

        [TriggerDescription("Deletes the file from disk")]
        [TriggerStringParameter]
        private bool DeleteFile(TriggerReader reader)
        {
            if (!reader.PeekString()) return false;
            string file = reader.ReadString();
            File.Delete(Path.Combine(DefaultAuthorizedPath, file));
            return true;
        }

        [TriggerDescription("Checks to see if the file exists")]
        [TriggerStringParameter]
        private bool FileExists(TriggerReader reader)
        {
            string file = (reader.PeekString()) ? reader.ReadString() : "";
            return File.Exists(file);
        }

        [TriggerDescription("Checks to see if the file does not exist")]
        [TriggerStringParameter]
        private bool FileNotExists(TriggerReader reader)
        {
            return !FileExists(reader);
        }

        [TriggerDescription("Reads the file contents and puts it into the specified variable")]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool ReadFileIntoVariable(TriggerReader reader)
        {
            string file = reader.ReadString();
            var var = reader.ReadVariable(true);
            StringBuilder sb = new StringBuilder();
            using (var stream = File.Open(Path.Combine(DefaultAuthorizedPath, file), FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    sb.Append(streamReader.ReadToEnd());
                }
            }
            var.Value = sb.ToString();
            return true;
        }
    }
}