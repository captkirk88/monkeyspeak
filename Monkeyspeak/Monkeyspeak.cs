using Monkeyspeak.lexical;
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

    public sealed class MonkeyspeakEngine
    {
        public Options options;

        public TokenVisitorHandler VisitTokens;

        public event Action<MonkeyspeakEngine> Resetting;

        public MonkeyspeakEngine()
        {
            options = new Options();
        }

        public MonkeyspeakEngine(Options options)
        {
            this.options = options;
        }

        public string Banner
        {
            get
            {
                // DO NOT MODIFY ORIGINAL AUTHOR, YOU MAY ADD ADDITIONAL AUTHORS.
                StringBuilder sb = new StringBuilder();
                sb.Append("Monkeyspeak").Append(' ').Append(options.Version.ToString(4)).Append(Environment.NewLine);
                sb.AppendLine("Author: Kirk");
                //sb.AppendLine("Author: You");
                sb.Append(".NET Framework ").Append(Assembly.GetAssembly(typeof(MonkeyspeakEngine)).ImageRuntimeVersion.ToString());
                return sb.ToString();
            }
        }

        public Options Options
        {
            get { return options; }
            set { options = value; }
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a string into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="chunk">String that contains the Monkeyspeak script source.</param>
        /// <returns></returns>
        public async Task<Page> LoadFromStringAsync(string chunk)
        {
            return await Task.Run(() => LoadFromString(chunk));
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a string into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="chunk">String that contains the Monkeyspeak script source.</param>
        /// <returns></returns>
        public Page LoadFromString(string chunk)
        {
            try
            {
                var stream = new MemoryStream();
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    writer.Write(chunk);
                    writer.Flush();
                }
                stream.Seek(0, SeekOrigin.Begin);
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

        public Page LoadFromFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Write))
            {
                using (var reader = new SStreamReader(stream))
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
        }

        /// <summary>
        /// Loads a Monkeyspeak script from a string into a <see cref="Monkeyspeak.Page"/>.
        /// </summary>
        /// <param name="chunk">String that contains the Monkeyspeak script source.</param>
        /// <param name="existingPage"></param>
        public async Task LoadFromStringAsync(Page existingPage, string chunk)
        {
            await Task.Run(() => LoadFromString(existingPage, chunk));
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
                var stream = new MemoryStream();
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    writer.Write(chunk);
                    writer.Flush();
                }
                stream.Seek(0, SeekOrigin.Begin);
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
            return await Task.Run(() => LoadFromStream(stream));
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
            await Task.Run(() => LoadFromStream(existingPage, stream));
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
        /// Loads compiled script from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public Page LoadCompiledFile(string filePath)
        {
            try
            {
                using (Stream stream = new FileStream(filePath, FileMode.Open))
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

        /// <summary>
        /// Compiles a Page to a file
        /// </summary>
        /// <param name="page"></param>
        /// <param name="filePath"></param>
        public void CompileToFile(Page page, string filePath)
        {
            page.CompileToFile(filePath);
        }

        /// <summary>
        /// Compiles a Page to a stream
        /// </summary>
        /// <param name="page"></param>
        /// <param name="stream"></param>
        public void CompileToStream(Page page, Stream stream)
        {
            page.CompileToStream(stream);
        }
    }
}