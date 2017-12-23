using Monkeyspeak.Lexical;
using Monkeyspeak.Lexical.Expressions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak
{
    [Serializable]
    public class MonkeyspeakException : Exception
    {
        public MonkeyspeakException()
        {
        }

        public MonkeyspeakException(string message)
            : base(message)
        {
        }

        public MonkeyspeakException(string format, params object[] message)
            : base(String.Format(format, message))
        {
        }

        public MonkeyspeakException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public MonkeyspeakException(string message, SourcePosition pos)
            : base(String.Format("{0} at {1}", message, pos))
        {
        }

        protected MonkeyspeakException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// The core class of Monkeyspeak, can create Pages by loading scripts from files, streams or raw strings.
    /// </summary>
    public sealed class MonkeyspeakEngine
    {
        public Options options;

        /// <summary>
        /// Set this to visit tokens during the parse step
        /// </summary>
        public TokenVisitorHandler VisitTokens;

        /// <summary>
        /// Occurs when [resetting].
        /// </summary>
        public event Action<MonkeyspeakEngine> Resetting;

        public MonkeyspeakEngine()
        {
            options = new Options();
        }

        public MonkeyspeakEngine(Options options)
        {
            this.options = options;
        }

        /// <summary>
        /// Gets the banner.
        /// </summary>
        /// <value>
        /// The banner.
        /// </value>
        public string Banner
        {
            get
            {
                // DO NOT MODIFY ORIGINAL AUTHOR, YOU MAY ADD ADD CONTRIBUTORS.
                StringBuilder sb = new StringBuilder();
                sb.Append("Monkeyspeak").Append(' ').Append(options.Version.ToString(4)).Append(Environment.NewLine);
                sb.AppendLine("Author: Kirk");
                //sb.AppendLine("Contributor: You");
                sb.Append(".NET Framework ").Append(Assembly.GetAssembly(typeof(MonkeyspeakEngine)).ImageRuntimeVersion.ToString());
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public Options Options
        {
            get { return options; }
            set { options = value; }
        }

        /// <summary>
        /// Replaces the expresson associated to the <paramref name="tokenType"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tokenType">Type of the token.</param>
        public void ReplaceExpresson<T>(TokenType tokenType) where T : Expression
        {
            Expressions.Instance[tokenType] = typeof(T);
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a string into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="chunk">String that contains the Monkeyspeak script source.</param>
        /// <returns></returns>
        public async Task<Page> LoadFromStringAsync(string chunk)
        {
            return await Task.Run(() => LoadFromString(chunk)).ContinueWith(task =>
            {
                if (task.Exception != null) throw task.Exception;
                else return task.Result;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a string into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="chunk">String that contains the Monkeyspeak script source.</param>
        /// <returns></returns>
        public Page LoadFromString(string chunk)
        {
            if (string.IsNullOrWhiteSpace(chunk)) throw new NullReferenceException("chunk");
            try
            {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(chunk));
                using (var reader = new SStreamReader(stream, Encoding.UTF8))
                {
                    Page page = new Page(this);
                    using (var lexer = new Lexer(this, reader))
                    {
                        page.VisitingToken = VisitTokens;
                        page.GenerateBlocks(lexer);
                        page.VisitingToken = null;
                    }
                    return page;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug<MonkeyspeakEngine>(ex);
                throw;
            }
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a file into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="filePath">the file path to the script</param>
        /// <returns></returns>
        /// <exception cref="System.IO.IOException"></exception>
        public Page LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // small fix for NUnit cases where the path is a temp location, also useful when passing simple file names to this method
                var path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly()?.CodeBase).Path));
                path = Path.Combine(path, filePath);
                if (File.Exists(path))
                    filePath = path;
                else throw new IOException($"{path} does not exist");
            }
            using (var reader = new SStreamReader(filePath))
            {
                Page page = new Page(this);
                using (var lexer = new Lexer(this, reader))
                {
                    page.VisitingToken = VisitTokens;
                    page.GenerateBlocks(lexer);
                    page.VisitingToken = null;
                }
                return page;
            }
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a file into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="filePath">the file path to the script</param>
        /// <returns><see cref="Page"/></returns>
        public async Task<Page> LoadFromFileAsync(string filePath)
        {
            return await Task.Run(() => LoadFromFile(filePath)).ContinueWith(task =>
            {
                if (task.Exception != null) throw task.Exception;
                else return task.Result;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a string into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="chunk">String that contains the Monkeyspeak script source.</param>
        /// <param name="existingPage"></param>
        public async Task LoadFromStringAsync(Page existingPage, string chunk)
        {
            await Task.Run(() => LoadFromString(existingPage, chunk)).ContinueWith(task =>
            {
                if (task.Exception != null) throw task.Exception;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a string into <paramref name="existingPage"/>. and
        /// clears the old page
        /// </summary>
        /// <param name="existingPage">Reference to an existing Page</param>
        /// <param name="chunk">String that contains the Monkeyspeak script source.</param>
        /// <returns></returns>
        public void LoadFromString(Page existingPage, string chunk)
        {
            try
            {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(chunk));
                using (var reader = new SStreamReader(stream, Encoding.UTF8))
                {
                    using (var lexer = new Lexer(this, reader))
                    {
                        existingPage.VisitingToken = VisitTokens;
                        existingPage.GenerateBlocks(lexer);
                        existingPage.VisitingToken = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug<MonkeyspeakEngine>(ex);
            }
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a Stream into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="stream">Stream that contains the Monkeyspeak script. Closes the stream.</param>
        /// <returns><see cref="Monkeyspeak.Page"/></returns>
        public async Task<Page> LoadFromStreamAsync(Stream stream)
        {
            return await Task.Run(() => LoadFromStream(stream)).ContinueWith(task =>
            {
                if (task.Exception != null) throw task.Exception;
                else return task.Result;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a Stream into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="stream">Stream that contains the Monkeyspeak script. Closes the stream.</param>
        /// <returns><see cref="Monkeyspeak.Page"/></returns>
        public Page LoadFromStream(Stream stream)
        {
            try
            {
                using (var reader = new SStreamReader(stream, true))
                {
                    Page page = new Page(this);
                    using (var lexer = new Lexer(this, reader))
                    {
                        page.VisitingToken = VisitTokens;
                        page.GenerateBlocks(lexer);
                        page.VisitingToken = null;
                    }
                    return page;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug<MonkeyspeakEngine>(ex);
            }
            return new Page(this); // return a empty page so that the top level caller is not destroyed by nullreference exceptions :)
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a Stream into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="stream">Stream that contains the Monkeyspeak script. Closes the stream.</param>
        /// <param name="existingPage"></param>
        public async Task LoadFromStreamAsync(Page existingPage, Stream stream)
        {
            await Task.Run(() => LoadFromStream(existingPage, stream)).ContinueWith(task =>
            {
                if (task.Exception != null) throw task.Exception;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a Stream into <paramref name="existingPage"/>.
        /// </summary>
        /// <param name="existingPage">Reference to an existing Page</param>
        /// <param name="stream">Stream that contains the Monkeyspeak script. Closes the stream.</param>
        public void LoadFromStream(Page existingPage, Stream stream)
        {
            try
            {
                using (var reader = new SStreamReader(stream, true))
                {
                    using (var lexer = new Lexer(this, reader))
                    {
                        existingPage.VisitingToken = VisitTokens;
                        existingPage.GenerateBlocks(lexer);
                        existingPage.VisitingToken = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug<MonkeyspeakEngine>(ex);
            }
        }

        /// <summary>
        /// Loads and executes the script.
        /// </summary>
        /// <param name="chunk">The script code.</param>
        /// <param name="triggerIds">The trigger ids.</param>
        /// <param name="entryHandler">The entry handler.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public Page DoString(string chunk, int[] triggerIds, TriggerHandler entryHandler = null, params object[] args)
        {
            var page = LoadFromString(chunk);
            if (entryHandler != null) page.AddTriggerHandler(TriggerCategory.Cause, 0, entryHandler);
            page.LoadAllLibraries();
            page.Execute((triggerIds != null && triggerIds.Length > 0 ? triggerIds : new[] { 0 }), args);
            return page;
        }

        /// <summary>
        /// Loads and executes the script.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="triggerIds">The trigger ids.</param>
        /// <param name="entryHandler">The entry handler.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public Page DoStream(Stream stream, int[] triggerIds, TriggerHandler entryHandler = null, params object[] args)
        {
            var page = LoadFromStream(stream);
            if (entryHandler != null) page.AddTriggerHandler(TriggerCategory.Cause, 0, entryHandler);
            page.LoadAllLibraries();
            page.Execute((triggerIds != null && triggerIds.Length > 0 ? triggerIds : new[] { 0 }), args);
            return page;
        }

        /// <summary>
        /// Loads and executes the script.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="triggerIds">The trigger ids.</param>
        /// <param name="entryHandler">The entry handler.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public Page DoFile(string filePath, int[] triggerIds, TriggerHandler entryHandler = null, params object[] args)
        {
            var page = LoadFromFile(filePath);
            if (entryHandler != null) page.AddTriggerHandler(TriggerCategory.Cause, 0, entryHandler);
            page.LoadAllLibraries();
            page.Execute((triggerIds != null && triggerIds.Length > 0 ? triggerIds : new[] { 0 }), args);
            return page;
        }

        /// <summary>
        /// Loads compiled script from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public Page LoadCompiledFile(string filePath)
        {
            try
            {
                // small fix for NUnit cases where the path is a temp location, also useful when passing simple file names to this method
                var path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
                filePath = Path.Combine(path, filePath);
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return LoadCompiledStream(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug<MonkeyspeakEngine>(ex);
            }
            return new Page(this);
        }

        /// <summary>
        /// Loads a compiled script from stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Page LoadCompiledStream(Stream stream)
        {
            try
            {
                var page = new Page(this);
                var compiler = new Compiler(this);
                page.LoadCompiledStream(stream);
                return page;
            }
            catch (Exception ex)
            {
                Logger.Debug<MonkeyspeakEngine>(ex);
            }
            return new Page(this);
        }
    }
}