using Monkeyspeak.Extensions;
using Monkeyspeak.lexical;
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
    /// <returns>True = Continue to the Next Trigger, False = Stop executing current block of Triggers</returns>
    public delegate bool TriggerHandler(TriggerReader reader);

    /// <summary>
    ///
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="handler">The handler.</param>
    public delegate void TriggerAddedEventHandler(Trigger trigger, TriggerHandler handler);

    public delegate bool TriggerHandledEventHandler(Trigger trigger);

    /// <summary>
    /// Event for any errors that occur during execution
    /// If not assigned Exceptions will be thrown.
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="ex"></param>
    public delegate void TriggerHandlerErrorEvent(TriggerHandler handler, Trigger trigger, Exception ex);

    /// <summary>
    ///
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
        /// Occurs when [initiating].  Best to call your variable additions in the event.
        /// </summary>
        public event Action<Page> Initiating;

        internal TokenVisitorHandler VisitingToken;

        /// <summary>
        /// Called when an Exception is raised during execution
        /// </summary>
        public event TriggerHandlerErrorEvent Error;

        /// <summary>
        /// Called when a Trigger and TriggerHandler is added to the Page
        /// </summary>
        public event TriggerAddedEventHandler TriggerAdded;

        /// <summary>
        /// Called before the Trigger's TriggerHandler is called.  If there is no TriggerHandler for that Trigger
        /// then this event is not raised.
        /// </summary>
        public event TriggerHandledEventHandler BeforeTriggerHandled;

        /// <summary>
        /// Called after the Trigger's TriggerHandler is called.  If there is no TriggerHandler for that Trigger
        /// then this event is not raised.
        /// </summary>
        public event TriggerHandledEventHandler AfterTriggerHandled;

        internal Page(MonkeyspeakEngine engine)
        {
            this.engine = engine;
            parser = new Parser(engine);
            triggerBlocks = new List<TriggerBlock>();
            scope = new Dictionary<string, IVariable>();
            libraries = new HashSet<BaseLibrary>();
            Initiate();
        }

        internal void Initiate()
        {
            SetVariable(new ConstantVariable(Variable.NoValue));
            SetVariable(new ConstantVariable("%MONKEY", engine.Banner));
            SetVariable(new ConstantVariable("%VERSION", engine.Options.Version.ToString(2)));
            LoadLibrary(Attributes.Instance);
            Initiating?.Invoke(this);
        }

        internal void GenerateBlocks(AbstractLexer lexer)
        {
            parser.VisitToken = VisitingToken;
            AddBlocks(parser.Parse(lexer));
            parser.VisitToken = null;
        }

        internal void AddBlocks(IEnumerable<TriggerBlock> blocks)
        {
            lock (syncObj)
            {
                var blocksArray = blocks.ToArray();
                for (int i = blocksArray.Length - 1; i >= 0; i--)
                    Size += blocksArray[i].Count;
                triggerBlocks.AddRange(blocksArray);
            }
            if (Size > engine.Options.TriggerLimit)
            {
                throw new MonkeyspeakException("Trigger limit exceeded.");
            }
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

        public MonkeyspeakEngine Engine
        {
            get { return engine; }
        }

        public void LoadCompiledStream(Stream stream)
        {
            try
            {
                Compiler compiler = new Compiler(engine);
                using (stream)
                    AddBlocks(compiler.DecompileFromStream(stream));
            }
            catch (Exception ex)
            {
                throw new MonkeyspeakException("Error reading compiled file.", ex);
            }
        }

        public void CompileToStream(Stream stream)
        {
            try
            {
                Compiler compiler = new Compiler(engine);
                using (stream)
                    compiler.CompileToStream(triggerBlocks, stream);
            }
            catch (Exception ex)
            {
                throw new MonkeyspeakException("Error compiling to file.", ex);
            }
        }

        public void CompileToFile(string filePath)
        {
            try
            {
                CompileToStream(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read));
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
        /// Removes the triggers.
        /// </summary>
        public void Clear()
        {
            triggerBlocks.Clear();
            foreach (var var in Scope)
            {
                if (var.Name.Contains("___")) RemoveVariable(var);
            }
            Size = 0;
        }

        /// <summary>
        /// Gets the description for all triggers
        /// </summary>
        /// <param name="excludeLibraryName">[true] hide library name, [false] show library name above triggers</param>
        /// <returns>IEnumerable of Triggers</returns>
        public IEnumerable<string> GetTriggerDescriptions(bool excludeLibraryName = false)
        {
            lock (syncObj)
            {
                foreach (var lib in libraries)
                {
                    yield return lib.ToString(excludeLibraryName);
                }
            }
        }

        /// <summary>
        /// Gets the description for a trigger
        /// </summary>
        /// <param name="excludeLibraryName">[true] hide library name, [false] show library name above triggers</param>
        /// <param name="trigger">todo: describe trigger parameter on GetTriggerDescription</param>
        /// <returns>string</returns>
        public string GetTriggerDescription(Trigger trigger, bool excludeLibraryName = false)
        {
            if (trigger == null) return "(#:#)";
            lock (syncObj)
            {
                return libraries.FirstOrDefault(lib => lib.Contains(trigger))?.ToString(trigger, excludeLibraryName) ?? trigger.ToString();
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
        public void LoadTimerLibrary(uint timersLimit = 10)
        {
            LoadLibrary(new Libraries.Timers(timersLimit));
        }

        /// <summary>
        /// Loads Monkeyspeak Debug Library into this Page
        /// <para>Used for Debug breakpoint insertion. Won't work without Debugger attached.</para>
        /// </summary>
        public void LoadDebugLibrary()
        {
            LoadLibrary(new Libraries.Debug());
        }

        /// <summary>
        /// Loads a <see cref="Libraries.BaseLibrary"/> into this Page
        /// </summary>
        /// <param name="lib"></param>
        public void LoadLibrary(Libraries.BaseLibrary lib)
        {
            foreach (var existing in libraries)
                if (existing.GetType().Equals(lib.GetType())) return;

            lib.Initialize();
            lock (syncObj)
            {
                foreach (var kv in lib.Handlers)
                {
                    SetTriggerHandler(kv.Key, kv.Value);
                }
                libraries.Add(lib);
            }
        }

        /// <summary>
        /// Loads trigger handlers from a assembly instance
        /// </summary>
        /// <param name="asm">The assembly instance</param>
        public void LoadLibraryFromAssembly(Assembly asm)
        {
            if (asm == null) return;
            foreach (var types in ReflectionHelper.GetAllTypesWithAttributeInMembers<TriggerHandlerAttribute>(asm))
                foreach (MethodInfo method in types.GetMethods().Where(method => method.IsDefined(typeof(TriggerHandlerAttribute), false)))
                {
                    foreach (TriggerHandlerAttribute attribute in ReflectionHelper.GetAllAttributesFromMethod<TriggerHandlerAttribute>(method))
                    {
                        attribute.owner = method;
                        try
                        {
                            attribute.Register(this);
                        }
                        catch (Exception ex)
                        {
                            throw new MonkeyspeakException(String.Format("Failed to load library from assembly '{0}', couldn't bind to method '{1}.{2}'", asm.FullName, method.DeclaringType.Name, method.Name), ex);
                        }
                    }
                }

            var subType = typeof(BaseLibrary);
            foreach (var type in asm.GetTypes())
            {
                if (type.IsSubclassOf(subType))
                    try
                    {
                        LoadLibrary((BaseLibrary)Activator.CreateInstance(type));
                    }
                    catch (MissingMemberException mme) { throw; }
                    catch (Exception ex) { }
            }
        }

        /// <summary>
        /// Loads trigger handlers from a assembly dll file
        /// </summary>
        /// <param name="assemblyFile">The assembly in the local file system</param>
        public void LoadLibraryFromAssembly(string assemblyFile)
        {
            Assembly asm;
            if (!File.Exists(assemblyFile))
                throw new MonkeyspeakException("Load library from file '" + assemblyFile + "' failed, file not found.");
            else if (!ReflectionHelper.TryLoad(assemblyFile, out asm))
            {
                throw new MonkeyspeakException("Load library from file '" + assemblyFile + "' failed.");
            }

            foreach (var types in ReflectionHelper.GetAllTypesWithAttributeInMembers<TriggerHandlerAttribute>(asm))
                foreach (MethodInfo method in types.GetMethods().Where(method => method.IsDefined(typeof(TriggerHandlerAttribute), false)))
                {
                    foreach (TriggerHandlerAttribute attribute in ReflectionHelper.GetAllAttributesFromMethod<TriggerHandlerAttribute>(method))
                    {
                        attribute.owner = method;
                        try
                        {
                            attribute.Register(this);
                        }
                        catch (Exception ex)
                        {
                            throw new MonkeyspeakException(String.Format("Failed to load library from file '{0}', couldn't bind to method '{1}.{2}'", assemblyFile, method.DeclaringType.Name, method.Name));
                        }
                    }
                }

            var subType = typeof(BaseLibrary);
            foreach (var type in asm.GetTypes())
            {
                if (type.BaseType == subType)
                    try
                    {
                        LoadLibrary((BaseLibrary)Activator.CreateInstance(type));
                    }
                    catch (MissingMemberException mme) { throw; }
                    catch (Exception ex) { }
            }
        }

        /// <summary>
        /// Loads all libraries.
        /// </summary>
        public void LoadAllLibraries()
        {
            foreach (var lib in BaseLibrary.GetAllLibraries())
                LoadLibrary(lib);
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
                    return existing.As<T>();
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
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
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
        /// Sets the variable table.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="isConstant">if set to <c>true</c> [is constant].</param>
        /// <returns></returns>
        /// <exception cref="Monkeyspeak.TypeNotSupportedException"></exception>
        /// <exception cref="Exception">Variable limit exceeded, operation failed.</exception>
        public VariableTable SetVariableTable(string name, bool isConstant)
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
        /// Gets a Variable with Name set to <paramref name="name"/>
        /// <b>Throws Exception if Variable not found.</b>
        /// </summary>
        /// <param name="name">The name of the Variable to retrieve</param>
        /// <returns>The variable found with the specified <paramref name="name"/> or throws Exception</returns>
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
        /// <returns>True on Variable found.  <para>False if Variable not found.</para></returns>
        public bool HasVariable(string name)
        {
            if (name[0] != engine.Options.VariableDeclarationSymbol)
                name = engine.Options.VariableDeclarationSymbol + name;
            if (name.IndexOf('[') != -1)
                name = name.Substring(0, name.IndexOf('[') - 1);

            lock (syncObj)
            {
                return scope.ContainsKey(name);
            }
        }

        /// <summary>
        /// Determines whether the specified variable exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="var">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the specified variable exists; otherwise, <c>false</c>.
        /// </returns>
        public bool HasVariable<T>(string name, out T var) where T : IVariable
        {
            if (name[0] != engine.Options.VariableDeclarationSymbol)
                name = engine.Options.VariableDeclarationSymbol + name;
            if (name.IndexOf('[') != -1)
                name = name.Substring(0, name.IndexOf('[') - 1);

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
        /// <param name="category"></param>
        /// <param name="id"></param>
        /// <param name="handler"></param>
        /// <param name="description"></param>
        public void SetTriggerHandler(TriggerCategory category, int id, TriggerHandler handler, string description = null)
        {
            SetTriggerHandler(new Trigger(category, id), handler, description);
        }

        /// <summary>
        /// Assigns the TriggerHandler to <paramref name="trigger"/>
        /// </summary>
        /// <param name="trigger"><see cref="Monkeyspeak.Trigger"/></param>
        /// <param name="handler"><see cref="Monkeyspeak.TriggerHandler"/></param>
        /// <param name="description">optional description of the trigger, normally the human readable form of the trigger
        /// <para>Example: "(0:1) when someone says something,"</para></param>
        public void SetTriggerHandler(Trigger trigger, TriggerHandler handler, string description = null)
        {
            Attributes.Instance.AddDescription(trigger, description);
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
        /// <param name="id">The identifier.</param>
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
        /// <value>
        ///   <c>true</c> if debug; otherwise, <c>false</c>.
        /// </value>
        public bool Debug { get; set; }

        /// <summary>
        /// Gets the libraries.
        /// </summary>
        /// <value>
        /// The libraries.
        /// </value>
        public IEnumerable<BaseLibrary> Libraries { get => libraries; }

        // Changed id to array for multiple Trigger processing.
        // This Compensates for a Design Flaw Lothus Marque spotted - Gerolkae

        /*
         * [1/7/2013 9:26:22 PM] Lothus Marque: Okay. Said feeling doesn't explain why 48 is
         * happening before 46, since your execute does them in increasing order. But what I
         * was suddenly staring at is that this has the definite potential to "run all 46,
         * then run 47, then run 48" ... and they're not all at once, in sequence.
         */

        private void ExecuteTrigger(TriggerBlock triggerBlock, ref int index, TriggerReader reader)
        {
            var current = triggerBlock[index];
            handlers.TryGetValue(current, out TriggerHandler handler);

            if (handler == null) Logger.Debug<Page>($"No handler found for {current}");

            reader.Trigger = current;
            reader.CurrentBlockIndex = index;
            try
            {
                bool canContinue = handler != null ? handler(reader) : false;
                if (AfterTriggerHandled != null && !AfterTriggerHandled(current)) return;
                Logger.Debug<Page>($"{GetTriggerDescription(current, true)} returned {canContinue}");
                if (reader.CurrentBlockIndex != index)
                {
                    index = reader.CurrentBlockIndex;
                    return;
                }

                if (!canContinue)
                {
                    bool found = false;
                    switch (current.Category)
                    {
                        case TriggerCategory.Cause:
                            // skip ahead for another condition to meet
                            Trigger possibleCause = Trigger.Undefined;
                            for (int i = index + 1; i <= triggerBlock.Count - 1; i++)
                            {
                                possibleCause = triggerBlock[i];
                                if (possibleCause.Category == TriggerCategory.Cause)
                                {
                                    index = i - 1; // set the current index of the outer loop
                                    found = true;
                                    break;
                                }
                            }
                            if (possibleCause.Category == TriggerCategory.Undefined) return;
                            break;

                        case TriggerCategory.Condition:
                            // skip ahead for another condition to meet
                            for (int i = index + 1; i <= triggerBlock.Count - 1; i++)
                            {
                                Trigger possibleCondition = triggerBlock[i];
                                if (possibleCondition.Category == TriggerCategory.Condition)
                                {
                                    index = i - 1; // set the current index of the outer loop
                                    found = true;
                                    break;
                                }
                            }
                            break;

                        case TriggerCategory.Flow:
                            // skip ahead for another flow trigger to meet
                            for (int i = index + 1; i <= triggerBlock.Count - 1; i++)
                            {
                                Trigger possibleFlow = triggerBlock[i];
                                if (possibleFlow.Category == TriggerCategory.Flow)
                                {
                                    index = i - 1; // set the current index of the outer loop
                                    found = true;
                                    break;
                                }
                            }
                            break;

                        case TriggerCategory.Effect:
                            found = true;
                            break;
                    }
                    if (!found) index = triggerBlock.Count;
                }
                else
                {
                    switch (current.Category)
                    {
                        case TriggerCategory.Flow:
                            var indexOfOtherFlow = triggerBlock.IndexOfTrigger(TriggerCategory.Flow, startIndex: index + 1);
                            var subBlock = triggerBlock.GetSubBlock(index + 1, indexOfOtherFlow);
                            var subReader = new TriggerReader(this, subBlock) { Parameters = reader.Parameters };
                            int i;
                            for (i = 0; i <= subBlock.Count - 1; i++)
                            {
                                ExecuteTrigger(subBlock, ref i, subReader);
                                if (i == -1)
                                    break;
                            }
                            //ExecuteBlock(subBlock, args: reader.Parameters);
                            if (i == -1)
                                index += subBlock.Count;
                            else index -= 1;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                if (Error != null)
                    Error(handlers[current], current, e);
                else throw;
                index = triggerBlock.Count;
            }
        }

        public void ExecuteBlock(TriggerBlock triggerBlock, int causeIndex = 0, params object[] args)
        {
            var reader = new TriggerReader(this, triggerBlock)
            {
                Parameters = args
            };

            Logger.Debug<Page>($"Block: {triggerBlock.ToString(',')}");

            int j = 0;
            for (j = causeIndex; j <= triggerBlock.Count - 1; j++)
            {
                ExecuteTrigger(triggerBlock, ref j, reader);
                if (j == -1) break;
            }
        }

        /// <summary>
        /// Executes a trigger block containing TriggerCategory.Cause with ID equal to <paramref name="id" />
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public void Execute(int id = 0, params object[] args)
        {
            lock (syncObj)
            {
                int index = -1, executed = 0;
                for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                {
                    if ((index = triggerBlocks[j].IndexOfTrigger(TriggerCategory.Cause, id)) != -1)
                    {
                        ExecuteBlock(triggerBlocks[j], index, args);
                        executed++;
                    }
                }
                if (index == -1 && executed == 0) Logger.Debug<Page>($"No {TriggerCategory.Cause} found with id {id}");
            }
        }

        /// <summary>
        /// Executes a trigger block containing <paramref name="cat"/> with ID equal to <paramref name="id" />
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <param name="cat"><see cref="TriggerCategory"/></param>
        public void Execute(TriggerCategory cat, int id = 0, params object[] args)
        {
            lock (syncObj)
            {
                int index = -1, executed = 0;
                for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                {
                    if ((index = triggerBlocks[j].IndexOfTrigger(cat, id)) != -1)
                    {
                        ExecuteBlock(triggerBlocks[j], index, args);
                        executed++;
                    }
                }
                if (index == -1 && executed == 0) Logger.Debug<Page>($"No {cat} found with id {id}");
            }
        }

        /// <summary>
        /// Executes a trigger block containing TriggerCategory.Cause with ID equal to <param name="id" />
        ///
        /// </summary>
        /// <param name="ids">I dunno</param>
        /// <param name="args"></param>
        public void Execute(int[] ids, params object[] args)
        {
            lock (syncObj)
            {
                int index = -1;
                for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                {
                    for (int i = 0; i <= ids.Length - 1; i++)
                    {
                        if ((index = triggerBlocks[j].IndexOfTrigger(TriggerCategory.Cause, ids[i])) != -1)
                        {
                            ExecuteBlock(triggerBlocks[j], index, args);
                        }
                    }
                }
                if (index == -1) Logger.Debug<Page>($"No Cause found that matches id's '{ids.ToString(' ')}'");
            }
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <param name="cat"><see cref="TriggerCategory"/></param>
        /// <returns></returns>
        public async Task ExecuteAsync(CancellationToken cancellationToken, TriggerCategory cat = TriggerCategory.Cause, int id = 0, params object[] args)
        {
            await Task.Run(() =>
            {
                lock (syncObj)
                {
                    for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                    {
                        int index;
                        if ((index = triggerBlocks[j].IndexOfTrigger(cat, id)) != -1)
                        {
                            ExecuteBlock(triggerBlocks[j], index, args);
                        }
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="ids">The ids.</param>
        /// <param name="args">todo: describe args parameter on ExecuteAsync</param>
        /// <param name="cat"><see cref="TriggerCategory"/></param>
        /// <returns></returns>
        public async Task ExecuteAsync(CancellationToken cancellationToken, TriggerCategory cat, int[] ids, params object[] args)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i <= ids.Length - 1; i++)
                {
                    int id = ids[i];
                    lock (syncObj)
                    {
                        for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                        {
                            int index;
                            if ((index = triggerBlocks[j].IndexOfTrigger(cat, id)) != -1)
                            {
                                ExecuteBlock(triggerBlocks[j], index, args);
                            }
                        }
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="ids">The ids.</param>
        /// <param name="args">todo: describe args parameter on ExecuteAsync</param>
        /// <param name="cat"><see cref="TriggerCategory"/></param>
        /// <returns></returns>
        public async Task ExecuteAsync(CancellationToken cancellationToken, int[] ids, params object[] args)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i <= ids.Length - 1; i++)
                {
                    int id = ids[i];
                    lock (syncObj)
                    {
                        for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                        {
                            int index;
                            if ((index = triggerBlocks[j].IndexOfTrigger(TriggerCategory.Cause, id)) != -1)
                            {
                                ExecuteBlock(triggerBlocks[j], index, args);
                            }
                        }
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Executes the specified Cause asynchronously.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cat"><see cref="TriggerCategory"/></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task ExecuteAsync(TriggerCategory cat = TriggerCategory.Cause, int id = 0, params object[] args)
        {
            await Task.Run(() =>
            {
                lock (syncObj)
                {
                    for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                    {
                        int index;
                        if ((index = triggerBlocks[j].IndexOfTrigger(cat, id)) != -1)
                        {
                            ExecuteBlock(triggerBlocks[j], index, args);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Executes the specified Cause asynchronously.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="args"></param>
        /// <param name="cat"><see cref="TriggerCategory"/></param>
        /// <returns></returns>
        public async Task ExecuteAsync(TriggerCategory cat, int[] ids, params object[] args)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i <= ids.Length - 1; i++)
                {
                    int id = ids[i];
                    lock (syncObj)
                    {
                        for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                        {
                            int index;
                            if ((index = triggerBlocks[j].IndexOfTrigger(cat, id)) != -1)
                            {
                                ExecuteBlock(triggerBlocks[j], index, args);
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Executes the specified Cause asynchronously.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task ExecuteAsync(int[] ids, params object[] args)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i <= ids.Length - 1; i++)
                {
                    int id = ids[i];
                    lock (syncObj)
                    {
                        for (int j = 0; j <= triggerBlocks.Count - 1; j++)
                        {
                            int index;
                            if ((index = triggerBlocks[j].IndexOfTrigger(TriggerCategory.Cause, id)) != -1)
                            {
                                ExecuteBlock(triggerBlocks[j], index, args);
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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