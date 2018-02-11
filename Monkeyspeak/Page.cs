using Monkeyspeak.Extensions;
using Monkeyspeak.Lexical;
using Monkeyspeak.Libraries;
using Monkeyspeak.Utils;
using Monkeyspeak.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace Monkeyspeak
{
    [Serializable]
    public class TypeNotSupportedException : Exception
    {
        public TypeNotSupportedException()
        {
        }

        public TypeNotSupportedException(string message)
            : base(message)
        {
        }

        public TypeNotSupportedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected TypeNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Used for handling triggers at runtime.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns>
    /// True = Continue to the Next Trigger, False = Stop executing current block of Triggers
    /// </returns>
    public delegate bool TriggerHandler(TriggerReader reader);

    /// <summary>
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="handler">The handler.</param>
    public delegate void TriggerAddedEventHandler(Trigger trigger, TriggerHandler handler);

    public delegate bool TriggerHandledEventHandler(Trigger trigger);

    /// <summary>
    /// Event for any errors that occur during execution If not assigned Exceptions will be thrown.
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="ex">     </param>
    public delegate void TriggerHandlerErrorEvent(Page page, TriggerHandler handler, Trigger trigger, Exception ex);

    /// <summary>
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns></returns>
    public delegate Token TokenVisitorHandler(ref Token token);

    [Serializable]
    public sealed class Page : IDisposable
    {
        public object syncObj = new Object();

        private Parser parser;
        private List<TriggerBlock> triggerBlocks;
        internal Dictionary<string, IVariable> scope;
        private HashSet<BaseLibrary> libraries;

        internal Dictionary<Trigger, TriggerHandler> handlers = new Dictionary<Trigger, TriggerHandler>();
        private MonkeyspeakEngine engine;

        public event Action Resetting;

        /// <summary>
        /// Occurs when [initiating]. Best to call your variable additions in the event.
        /// </summary>
        public event Action<Page> Initiating;

        internal TokenVisitorHandler VisitingToken;

        /// <summary>
        /// Called when a Monkeyspeak exception is raised during execution
        /// </summary>
        public event TriggerHandlerErrorEvent Error;

        /// <summary>
        /// Called when a Trigger and TriggerHandler is added to the Page
        /// </summary>
        public event TriggerAddedEventHandler TriggerAdded;

        /// <summary>
        /// Called after the Trigger's TriggerHandler is called. If there is no TriggerHandler for
        /// that Trigger then this event is not raised.
        /// </summary>
        public event TriggerHandledEventHandler TriggerHandled;

        public IReadOnlyCollection<Trigger> Triggers
        {
            get => new ReadOnlyCollection<Trigger>(triggerBlocks.SelectMany(block => block).ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        public Page(MonkeyspeakEngine engine)
        {
            this.engine = engine;
            parser = new Parser(engine);
            triggerBlocks = new List<TriggerBlock>();
            scope = new Dictionary<string, IVariable>();
            libraries = new HashSet<BaseLibrary>();
            CanExecute = true;
            Initiate();
        }

        internal void Initiate()
        {
            SetVariable(new ConstantVariable(Variable.NoValue));
            SetVariable(new ConstantVariable("%MONKEY", engine.Banner));
            SetVariable(new ConstantVariable("%VERSION", engine.Options.Version.ToString(2)));
            Initiating?.Invoke(this);
        }

        internal void GenerateBlocks(AbstractLexer lexer)
        {
            parser.VisitToken = VisitingToken;
            AddBlocks(parser.Parse(lexer));
            parser.VisitToken = null;
        }

        internal void AddBlocks(IEnumerable<Trigger> triggers)
        {
            lock (syncObj)
            {
                Trigger nextTrig = Trigger.Undefined, curTrig = Trigger.Undefined;
                TriggerBlock block = new TriggerBlock(5);
                foreach (var trig in triggers)
                {
                    curTrig = nextTrig;
                    nextTrig = trig;

                    if (curTrig != Trigger.Undefined)
                    {
                        Size++;
                        block.Add(curTrig);
                    }

                    if ((curTrig.Category == TriggerCategory.Effect) &&
                        nextTrig.Category == TriggerCategory.Cause)
                    {
                        triggerBlocks.Add(block);
                        block = new TriggerBlock(5);
                    }

                    if (Size > engine.Options.TriggerLimit)
                    {
                        throw new MonkeyspeakException("Trigger limit exceeded.");
                    }
                }
                block.Add(nextTrig);
                triggerBlocks.Add(block);
            }
        }

        internal void OnError(TriggerHandler triggerHandler, Trigger current, Exception e)
        {
            try
            {
                Error?.Invoke(this, triggerHandler, current, e);
            }
            catch (Exception ex) { ex.Log(Level.Error); }
        }

        private void TrimToLimit(int limit)
        {
            lock (syncObj)
            {
                for (int i = triggerBlocks.Count - 1; i >= 0; i--)
                {
                    for (int j = triggerBlocks[i].Count - 1; j >= 0; j--)
                    {
                        var curSize = i + j;
                        if (curSize >= limit)
                        {
                            triggerBlocks[i].RemoveAt(j);
                        }
                    }
                    if (triggerBlocks[i].Count == 0) triggerBlocks.RemoveAt(i);
                }
            }
        }

        internal bool CheckType(object value)
        {
            if (value == null) return true;

            return value is string ||
                   value is double;
        }

        /// <summary>
        /// Gets the engine.
        /// </summary>
        /// <value>The engine.</value>
        public MonkeyspeakEngine Engine
        {
            get { return engine; }
        }

        /// <summary>
        /// Loads the compiled stream. *DOES NOT CLOSE THE STREAM!*
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void LoadCompiledStream(Stream stream)
        {
            try
            {
                Compiler compiler = new Compiler(engine);
                AddBlocks(compiler.DecompileFromStream(stream));
            }
            catch (Exception ex)
            {
                throw new MonkeyspeakException("Error reading compiled stream.", ex);
            }
        }

        public void LoadCompiledFile(string filePath)
        {
            try
            {
                Compiler compiler = new Compiler(engine);
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    AddBlocks(compiler.DecompileFromStream(fileStream));
            }
            catch (Exception ex)
            {
                throw new MonkeyspeakException("Error reading compiled file.", ex);
            }
        }

        /// <summary>
        /// Compiles to stream. *DOES NOT CLOSE THE STREAM!*
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <exception cref="MonkeyspeakException">Error compiling to stream.</exception>
        public void CompileToStream(Stream stream)
        {
            try
            {
                Compiler compiler = new Compiler(engine);
                compiler.CompileToStream(triggerBlocks, stream);
            }
            catch (Exception ex)
            {
                throw new MonkeyspeakException("Error compiling to stream.", ex);
            }
        }

        public void CompileToFile(string filePath)
        {
            try
            {
                if (!Path.HasExtension(filePath)) filePath += ".msx";
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    CompileToStream(fileStream);
                }
            }
            catch (IOException ioex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MonkeyspeakException("Error compiling to file.", ex);
            }
        }

        /// <summary>
        /// Clears all Variables and optionally clears all TriggerHandlers from this Page.
        /// </summary>
        public void Reset(bool resetTriggerHandlers = false)
        {
            Resetting?.Invoke();
            lock (syncObj)
            {
                scope.Clear();
                foreach (var lib in libraries) lib.Unload(this);
                if (resetTriggerHandlers) handlers.Clear();
            }
            Initiate();
        }

        /// <summary>
        /// Removes the triggers and variables.
        /// </summary>
        public void Clear()
        {
            triggerBlocks.Clear();
            scope.Clear();
            Size = 0;
            Initiate();
        }

        /// <summary>
        /// Gets the description for all triggers
        /// </summary>
        /// <param name="excludeLibraryName">
        /// [true] hide library name, [false] show library name above triggers
        /// </param>
        /// <returns>IEnumerable of Triggers</returns>
        public IEnumerable<Tuple<BaseLibrary, Trigger, string>> GetTriggerDescriptions(bool excludeLibraryName = false)
        {
            lock (syncObj)
            {
                foreach (var lib in libraries)
                {
                    foreach (var kv in lib.Handlers)
                    {
                        yield return new Tuple<BaseLibrary, Trigger, string>(lib, kv.Key, lib.ToString(kv.Key));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the description for a trigger
        /// </summary>
        /// <param name="excludeLibraryName">
        /// [true] hide library name, [false] show library name above triggers
        /// </param>
        /// <param name="trigger">           todo: describe trigger parameter on GetTriggerDescription</param>
        /// <returns>string</returns>
        public string GetTriggerDescription(Trigger trigger, bool excludeLibraryName = false)
        {
            if (trigger == null || trigger == Trigger.Undefined) return string.Empty;
            lock (syncObj)
            {
                return libraries.FirstOrDefault(lib => lib.Contains(trigger.Category, trigger.Id))?.ToString(trigger) ?? trigger.ToString();
            }
        }

        /// <summary>
        /// All variables loaded into this page
        /// </summary>
        public ReadOnlyCollection<IVariable> Scope
        {
            get { return new ReadOnlyCollection<IVariable>(scope.Values.ToArray()); }
        }

        /// <summary>
        /// Loads Monkeyspeak Sys Library into this Page
        /// <para>Used for System operations involving the Environment or Operating System</para>
        /// </summary>
        public void LoadSysLibrary()
        {
            LoadLibrary(new Libraries.Sys());
        }

        /// <summary>
        /// Loads Monkeyspeak String Library into this Page
        /// <para>Used for basic String operations</para>
        /// </summary>
        public void LoadStringLibrary()
        {
            LoadLibrary(new Libraries.StringOperations());
        }

        /// <summary>
        /// Loads Monkeyspeak IO Library into this Page
        /// <para>Used for File Input/Output operations</para>
        /// </summary>
        /// <param name="authorizedPath">
        /// the directory the IO library will use that it can read/write to
        /// </param>
        public void LoadIOLibrary(string authorizedPath = null)
        {
            LoadLibrary(new Libraries.IO(authorizedPath));
        }

        /// <summary>
        /// Loads Monkeyspeak Math Library into this Page
        /// <para>Used for Math operations (add, subtract, multiply, divide)</para>
        /// </summary>
        public void LoadMathLibrary()
        {
            LoadLibrary(new Libraries.Math());
        }

        /// <summary>
        /// Loads Monkeyspeak Timer Library into this Page
        /// </summary>
        /// <param name="timersLimit">the timer limit, if null or 0, defaults to 10</param>
        public void LoadTimerLibrary(uint timersLimit = 10, TimeZoneInfo timeZoneInfo = default(TimeZoneInfo))
        {
            LoadLibrary(new Timers(timersLimit, timeZoneInfo));
        }

        /// <summary>
        /// Loads a <see cref="BaseLibrary"/> into this Page
        /// </summary>
        /// <param name="lib"> </param>
        /// <param name="args">Arguments to pass on the library's Initialize method</param>
        public void LoadLibrary(BaseLibrary lib, params object[] args)
        {
            foreach (var existing in libraries)
                if (existing.GetType().Equals(lib.GetType())) return;

            try
            {
                lib.Initialize(args);
                lock (syncObj)
                {
                    foreach (var kv in lib.Handlers)
                    {
                        AddTriggerHandler(kv.Key, kv.Value);
                    }
                    libraries.Add(lib);
                }
                Logger.Debug<Page>($"Added library {lib.GetType().Name}");
            }
            catch (Exception ex)
            {
                Logger.Error<Page>($"Failed to initialize {lib.GetType().Name}\n{ex}");
            }
        }

        /// <summary>
        /// Loads trigger handlers from a assembly instance
        /// </summary>
        /// <param name="asm"> The assembly instance</param>
        /// <param name="args">Arguments to pass on the library's Initialize method</param>
        public void LoadLibraryFromAssembly(Assembly asm, params object[] args)
        {
            if (asm == null) return;

            foreach (var lib in BaseLibrary.GetLibrariesFromAssembly(asm))
            {
                LoadLibrary(lib, args);
            }
        }

        /// <summary>
        /// Loads trigger handlers from a assembly dll file
        /// </summary>
        /// <param name="assemblyFile">The assembly in the local file system</param>
        /// <param name="args">        Arguments to pass on the library's Initialize method</param>
        public void LoadLibraryFromAssembly(string assemblyFile, params object[] args)
        {
            Assembly asm;
            if (!File.Exists(assemblyFile))
                throw new MonkeyspeakException("Load library from file '" + assemblyFile + "' failed, file not found.");
            else if (!ReflectionHelper.TryLoadAssemblyFromFile(assemblyFile, out asm))
            {
                throw new MonkeyspeakException("Load library from file '" + assemblyFile + "' failed.");
            }

            foreach (var lib in BaseLibrary.GetLibrariesFromAssembly(asm))
            {
                LoadLibrary(lib, args);
            }
        }

        /// <summary>
        /// Loads all libraries.
        /// </summary>
        /// <param name="args">Arguments to pass on the library's Initialize method</param>
        public void LoadAllLibraries(params object[] args)
        {
            foreach (var lib in BaseLibrary.GetAllLibraries())
                LoadLibrary(lib, args);
        }

        public bool RemoveLibrary(BaseLibrary lib)
        {
            return RemoveAllTriggerHandlers(libraries.FirstOrDefault(l => l == lib)) > 0;
        }

        public bool RemoveLibrary(Type libraryType)
        {
            return RemoveAllTriggerHandlers(libraries.FirstOrDefault(lib => lib.GetType() == libraryType)) > 0;
        }

        public bool RemoveLibrary<T>() where T : BaseLibrary
        {
            return RemoveAllTriggerHandlers(libraries.OfType<T>().FirstOrDefault()) > 0;
        }

        public T GetLibrary<T>() where T : BaseLibrary
        {
            return libraries.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Removes the variable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            if (name[0] != engine.Options.VariableDeclarationSymbol)
                name = engine.Options.VariableDeclarationSymbol + name;

            lock (syncObj)
            {
                bool result = scope.Remove(name);
                return result;
            }
        }

        /// <summary>
        /// Removes the variable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveVariable(IVariable var)
        {
            if (var == null) return true;

            string varName = var.Name;
            if (varName[0] != engine.Options.VariableDeclarationSymbol)
                varName = engine.Options.VariableDeclarationSymbol + varName;

            lock (syncObj)
            {
                bool result = scope.Remove(varName);
                return result;
            }
        }

        /// <summary>
        /// Sets the variable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="var">The variable.</param>
        /// <exception cref="Monkeyspeak.TypeNotSupportedException"></exception>
        /// <exception cref="Exception">Variable limit exceeded, operation failed.</exception>
        public T SetVariable<T>(T var) where T : IVariable
        {
            if (!CheckType(var.Value)) throw new TypeNotSupportedException(String.Format("{0} is not a supported type. Expecting string or double.", var.Value.GetType().Name));

            string varName = var.Name;
            if (varName[0] != engine.Options.VariableDeclarationSymbol)
                varName = engine.Options.VariableDeclarationSymbol + varName;

            lock (syncObj)
            {
                if (scope.TryGetValue(varName, out IVariable existing))
                {
                    if (!(existing is ConstantVariable))
                        existing.Value = var.Value;
                    return (T)existing;
                }
                else
                {
                    if (scope.Count + 1 > engine.Options.VariableCountLimit) throw new Exception("Variable limit exceeded, operation failed.");
                    scope.Add(varName, var);
                    return var;
                }
            }
        }

        /// <summary>
        /// Sets the variable.
        /// </summary>
        /// <param name="name">      The name.</param>
        /// <param name="value">     The value.</param>
        /// <param name="isConstant">if set to <c>true</c> [is constant].</param>
        /// <returns></returns>
        /// <exception cref="Monkeyspeak.TypeNotSupportedException"></exception>
        /// <exception cref="Exception">Variable limit exceeded, operation failed.</exception>
        public IVariable SetVariable(string name, object value, bool isConstant = false)
        {
            if (!CheckType(value)) throw new TypeNotSupportedException(String.Format("{0} is not a supported type. Expecting string or double.", value.GetType().Name));
            if (name[0] != engine.Options.VariableDeclarationSymbol)
                name = engine.Options.VariableDeclarationSymbol + name;
            IVariable var;

            lock (syncObj)
            {
                if (scope.TryGetValue(name, out IVariable v))
                {
                    var = v;
                    var.Value = value;
                }
                else
                {
                    if (isConstant)
                        var = new ConstantVariable(name, value);
                    else
                        var = new Variable(name, value, isConstant);
                    if (scope.Count + 1 > engine.Options.VariableCountLimit) throw new Exception("Variable limit exceeded, operation failed.");
                    scope.Add(var.Name, var);
                }
                return var;
            }
        }

        /// <summary>
        /// Creates a <seealso cref="VariableTable"/>
        /// </summary>
        /// <param name="name">      The name.</param>
        /// <param name="value">     The value.</param>
        /// <param name="isConstant">if set to <c>true</c> [is constant].</param>
        /// <returns></returns>
        /// <exception cref="Monkeyspeak.TypeNotSupportedException"></exception>
        /// <exception cref="Exception">Variable limit exceeded, operation failed.</exception>
        public VariableTable CreateVariableTable(string name, bool isConstant)
        {
            if (name[0] != engine.Options.VariableDeclarationSymbol)
                name = engine.Options.VariableDeclarationSymbol + name;
            VariableTable var;

            lock (syncObj)
            {
                if (scope.TryGetValue(name, out IVariable v))
                {
                    var = (VariableTable)v;
                }
                else
                {
                    var = new VariableTable(name, isConstant);
                    if (scope.Count + 1 > engine.Options.VariableCountLimit) throw new Exception("Variable limit exceeded, operation failed.");
                    scope.Add(var.Name, var);
                }
                return var;
            }
        }

        /// <summary>
        /// Gets a Variable with Name set to <paramref name="name"/><b>Throws Exception if Variable
        /// not found.</b>
        /// </summary>
        /// <param name="name">The name of the Variable to retrieve</param>
        /// <returns>
        /// The variable found with the specified <paramref name="name"/> or throws Exception
        /// </returns>
        public IVariable GetVariable(string name)
        {
            if (name[0] != engine.Options.VariableDeclarationSymbol)
                name = engine.Options.VariableDeclarationSymbol + name;
            if (name.IndexOf('[') != -1)
                name = name.Substring(0, name.IndexOf('[') - 1);

            lock (syncObj)
            {
                if (scope.TryGetValue(name, out IVariable var))
                    return var;
                throw new Exception("Variable \"" + name + "\" not found.");
            }
        }

        /// <summary>
        /// Checks the scope for the Variable with Name set to <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name of the Variable to retrieve</param>
        /// <returns>
        /// True on Variable found.
        /// <para>False if Variable not found.</para>
        /// </returns>
        public bool HasVariable(string name)
        {
            if (name[0] != engine.Options.VariableDeclarationSymbol)
                name = engine.Options.VariableDeclarationSymbol + name;
            if (name.IndexOf('[') != -1)
                name = name.LeftOf('[');

            lock (syncObj)
            {
                return scope.ContainsKey(name);
            }
        }

        /// <summary>
        /// Determines whether the specified variable exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="var"> The variable.</param>
        /// <returns><c>true</c> if the specified variable exists; otherwise, <c>false</c>.</returns>
        public bool HasVariable<T>(string name, out T var) where T : IVariable
        {
            if (name[0] != engine.Options.VariableDeclarationSymbol)
                name = engine.Options.VariableDeclarationSymbol + name;
            if (name.IndexOf('[') != -1)
                name = name.LeftOf('[');

            lock (syncObj)
            {
                if (!scope.TryGetValue(name, out IVariable _var))
                {
                    var = default(T);
                    return false;
                }
                var = (T)_var;
                return true;
            }
        }

        /// <summary>
        /// Assigns the TriggerHandler to a trigger with <paramref name="category"/> and <paramref name="id"/>
        /// </summary>
        /// <param name="category">   </param>
        /// <param name="id">         </param>
        /// <param name="handler">    </param>
        /// <param name="description"></param>
        public void AddTriggerHandler(TriggerCategory category, int id, TriggerHandler handler)
        {
            AddTriggerHandler(new Trigger(category, id), handler);
        }

        /// <summary>
        /// Assigns the TriggerHandler to <paramref name="trigger"/>
        /// </summary>
        /// <param name="trigger">    <see cref="Monkeyspeak.Trigger"/></param>
        /// <param name="handler">    <see cref="Monkeyspeak.TriggerHandler"/></param>
        /// <param name="description">
        /// optional description of the trigger, normally the human readable form of the trigger
        /// <para>Example: "(0:1) when someone says something,"</para>
        /// </param>
        public void AddTriggerHandler(Trigger trigger, TriggerHandler handler)
        {
            lock (syncObj)
            {
                if (!handlers.ContainsKey(trigger))
                {
                    handlers.Add(trigger, handler);
                    TriggerAdded?.Invoke(trigger, handler);
                }
                else if (engine.Options.CanOverrideTriggerHandlers)
                {
                    handlers[trigger] = handler;
                }
                else throw new UnauthorizedAccessException($"Override of existing Trigger handler from {handler.Method} for {trigger} set to handler in {handlers[trigger].Method}");
            }
        }

        /// <summary>
        /// Removes the trigger handler.
        /// </summary>
        /// <param name="cat">The category.</param>
        /// <param name="id"> The identifier.</param>
        public void RemoveTriggerHandler(TriggerCategory cat, int id)
        {
            handlers.Remove(new Trigger(cat, id));
        }

        /// <summary>
        /// Removes the trigger handler.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        public void RemoveTriggerHandler(Trigger trigger)
        {
            lock (syncObj)
            {
                handlers.Remove(trigger);
            }
        }

        /// <summary>
        /// Removes all trigger handlers.
        /// </summary>
        /// <param name="lib">The library.</param>
        private int RemoveAllTriggerHandlers(BaseLibrary lib)
        {
            if (lib == null) return 0;
            int countRemoved = 0;
            foreach (var handler in lib.Handlers)
            {
                if (handlers.Remove(handler.Key)) countRemoved++;
            }
            return countRemoved;
        }

        /// <summary>
        /// Returns the Trigger count on this Page.
        /// </summary>
        /// <returns></returns>
        public int Size
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Page"/> is in debug mode.
        /// </summary>
        /// <value><c>true</c> if debug; otherwise, <c>false</c>.</value>
        public bool Debug { get; set; }

        /// <summary>
        /// Gets the libraries.
        /// </summary>
        /// <value>The libraries.</value>
        public IEnumerable<BaseLibrary> Libraries { get => libraries; }

        public bool CanExecute { get; set; }

        /// <summary>
        /// Executes a trigger block containing TriggerCategory.Cause with ID equal to <paramref name="id"/>
        /// </summary>
        /// <param name="id">  </param>
        /// <param name="args"></param>
        public void Execute(int id = 0, params object[] args)
        {
            if (!CanExecute) return;
            try
            {
                lock (syncObj)
                {
                    int index = -1, executed = 0;
                    for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                    {
                        if ((index = triggerBlocks[j].IndexOfTrigger(TriggerCategory.Cause, id)) != -1)
                        {
                            new ExecutionContext(this, triggerBlocks[j], args).Run(index);
                            executed++;
                        }
                    }
                    if (executed == 0)
                        Logger.Debug<Page>($"Trigger ({(int)TriggerCategory.Cause}:{id}) not found.");
                }
            }
            catch (Exception ex)
            {
                ex.Log<Page>();
            }
        }

        /// <summary>
        /// Executes a trigger block containing TriggerCategory.Cause with ID equal to <param name="id"/>
        /// </summary>
        /// <param name="ids"> I dunno</param>
        /// <param name="args"></param>
        public void Execute(int[] ids, params object[] args)
        {
            if (!CanExecute) return;
            try
            {
                lock (syncObj)
                {
                    var cat = TriggerCategory.Cause;
                    for (int i = 0; i <= ids.Length - 1; i++)
                    {
                        int index = -1, executed = 0, id = ids[i];
                        for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                        {
                            if ((index = triggerBlocks[j].IndexOfTrigger(cat, id)) != -1)
                            {
                                new ExecutionContext(this, triggerBlocks[j], args).Run(index);
                                executed++;
                            }
                        }
                        if (executed == 0)
                            Logger.Debug<Page>($"Trigger ({(int)cat}:{id}) not found.");
                        executed = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Log<Page>();
            }
        }

        /// <summary>
        /// Executes the specified Cause Executes with specified <paramref name="ids"/> asynchronously.
        /// </summary>
        /// <param name="ids">              The ids.</param>
        /// <param name="args">             todo: describe args parameter on ExecuteAsync</param>
        /// <param name="cat">              <see cref="TriggerCategory"/></param>
        /// <param name="cancellationToken">cancellation token to end the executing task</param>
        /// <returns></returns>
        public async Task ExecuteAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default(CancellationToken), params object[] args)
        {
            if (!CanExecute) return;
            foreach (var id in ids)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        Execute(id, args);
                    }, cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
#if DEBUG
                    ex.Log<Page>();
#endif
                }
                catch (Exception ex)
                {
                    ex.Log<Page>();
                }
            }
        }

        /// <summary>
        /// Executes the specified Cause asynchronously.
        /// </summary>
        /// <param name="args">             </param>
        /// <param name="id">               The id</param>
        /// <param name="cancellationToken">cancellation token to end the executing task</param>
        /// <returns></returns>
        public async Task ExecuteAsync(int id = 0, CancellationToken cancellationToken = default(CancellationToken), params object[] args)
        {
            if (!CanExecute) return;
            try
            {
                await Task.Run(() =>
                {
                    Execute(id, args);
                }, cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
#if DEBUG
                ex.Log<Page>();
#endif
            }
            catch (Exception ex)
            {
                ex.Log<Page>();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var lib in libraries)
            {
                lib.Unload(this);
            }
            Clear();
            Reset(true);
        }
    }
}