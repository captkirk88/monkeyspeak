using Monkeyspeak.Extensions;
using Monkeyspeak.lexical;
using Monkeyspeak.lexical.Expressions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Monkeyspeak
{
    [Serializable]
    public class TriggerReaderException : Exception
    {
        public TriggerReaderException()
        {
        }

        public TriggerReaderException(string message)
            : base(message)
        {
        }

        public TriggerReaderException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected TriggerReaderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// A Reader that is used to get Variables, Strings, and Numbers from Triggers
    /// </summary>
    [CLSCompliant(false)]
    public sealed class TriggerReader
    {
        private Trigger originalTrigger;

        private TriggerBlock currentBlock;

        private object[] args;
        private Page page;
        private MonkeyspeakEngine engine;
        private SourcePosition lastPos;
        private Queue<IExpression> contents;

        private readonly object syncObject = new object();

        /// <summary>
        /// A Reader that is used to get Variables, Strings, and Numbers from Triggers
        /// </summary>
        /// <param name="page"></param>
        /// <param name="trigger"></param>
        public TriggerReader(Page page, TriggerBlock currentBlock)
        {
            this.page = page;
            engine = page.Engine;
            if (currentBlock != null)
                this.currentBlock = new TriggerBlock(currentBlock);
        }

        /// <summary>
        /// Gets the trigger.
        /// </summary>
        /// <value>
        /// The trigger.
        /// </value>
        public Trigger Trigger
        {
            get { return originalTrigger; }
            internal set
            {
                originalTrigger = value;
                contents = new Queue<IExpression>(originalTrigger.contents);
            }
        }

        /// <summary>
        /// Gets the current block of triggers.
        /// </summary>
        /// <value>
        /// The block.
        /// </value>
        public TriggerBlock CurrentBlock
        {
            get { return currentBlock; }
            internal set { currentBlock = value; }
        }

        public int CurrentBlockIndex { get; internal set; }

        /// <summary>
        /// Gets the trigger category.
        /// </summary>
        /// <value>
        /// The trigger category.
        /// </value>
        public TriggerCategory TriggerCategory
        {
            get { return Trigger.Category; }
        }

        /// <summary>
        /// Gets the trigger identifier.
        /// </summary>
        /// <value>
        /// The trigger identifier.
        /// </value>
        public int TriggerId
        {
            get { return Trigger.Id; }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <value>
        /// The page.
        /// </value>
        public Page Page
        {
            get { return page; }
            internal set { page = value; }
        }

        /// <summary>
        /// Resets the reader's indexes to 0
        /// </summary>
        public void Reset()
        {
            if (Trigger != null)
            {
                contents = new Queue<IExpression>(originalTrigger.contents);
            }
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public object[] Parameters { get => args; internal set => args = value; }

        public MonkeyspeakEngine Engine { get => engine; set => engine = value; }

        /// <summary>
        /// Tries the get the parameter at the specified index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> on success; <c>false</c> otherwise</returns>
        public bool TryGetParameter<T>(int index, out T value) where T : struct
        {
            if (args != null && args.Length > index)
            {
                value = args[index].As<T>();
                return true;
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T GetParameter<T>(int index) where T : struct
        {
            if (args != null && args.Length > index)
                return args[index].As<T>();
            return default(T);
        }

        /// <summary>
        /// Reads the next String, throws TriggerReaderException on failure
        /// </summary>
        /// <param name="processVariables">[true] process variables and replace them with their values; [false] do nothing</param>
        /// <returns></returns>
        /// <exception cref="TriggerReaderException"></exception>
        public string ReadString(bool processVariables = true)
        {
            if (contents.Count == 0) throw new TriggerReaderException("Unexpected end of values");
            if (!(contents.Peek() is StringExpression)) throw new TriggerReaderException($"Expected string, got {contents.Peek().GetType().Name} at {contents.Peek().Position}");
            try
            {
                var expr = (contents.Dequeue() as StringExpression);
                lastPos = expr.Position;
                var str = expr.Value;

                if (str[0] == '@')
                {
                    processVariables = false;
                    str = str.Substring(1);
                }

                if (processVariables)
                {
                    for (int i = page.Scope.Count - 1; i >= 0; i--)
                    {
                        var var = page.Scope[i];
                        object value = null;
                        // replaced string.replace with Regex because
                        //  %ListName would replace %ListName2 leaving the 2 at the end
                        //- Gerolkae
                        var pattern = var.Name + @"\b(\[[a-zA-Z_0-9]*\]+)?";
                        str = Regex.Replace(str, pattern, new MatchEvaluator(match =>
                        {
                            if (match.Success)
                            {
                                string val = match.Value;
                                if (val.IndexOf('[') != -1 && val.IndexOf(']') != -1)
                                {
                                    if (var is VariableTable)
                                    {
                                        value = (var as VariableTable)[val.Substring(val.IndexOf('[') + 1).TrimEnd(']')];
                                    }
                                }
                                else
                                    value = var.Value;
                            }
                            return value != null ? value.As<string>() : "null";
                        }), RegexOptions.CultureInvariant);
                    }
                }
                return str;
            }
            catch (Exception ex)
            {
                Logger.Error<TriggerReader>(ex);
                throw new TriggerReaderException($"No value found at {lastPos}");
            }
        }

        /// <summary>
        /// Peeks at the next value
        /// </summary>
        /// <returns></returns>
        public bool PeekString()
        {
            if (contents.Count == 0) return false;
            return contents.Peek() is StringExpression;
        }

        /// <summary>
        /// Tries the read a string from the trigger.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="processVariables">if set to <c>true</c> [process variables].</param>
        /// <returns>true on success; false otherwise</returns>
        public bool TryReadString(out string str, bool processVariables = true)
        {
            if (!PeekString())
            {
                str = String.Empty;
                return false;
            }

            str = ReadString(processVariables);
            return true;
        }

        /// <summary>
        /// Reads the next Variable available, throws TriggerReaderException on failure
        /// </summary>
        /// <param name="addIfNotExist">Add the Variable if it doesn't exist and return that Variable with a Value equal to null.</param>
        /// <returns>Variable</returns>
        /// <exception cref="TriggerReaderException"></exception>
        public IVariable ReadVariable(bool addIfNotExist = false)
        {
            if (contents.Count == 0) throw new TriggerReaderException("Unexpected end of values");
            if (!(contents.Peek() is VariableExpression)) throw new TriggerReaderException($"Expected variable, got {contents.Peek().GetType().Name} at {contents.Peek().Position}");
            try
            {
                IVariable var;
                var expr = contents.Dequeue() as VariableExpression;
                lastPos = expr.Position;
                var varRef = expr.Value;
                if (!page.HasVariable(varRef, out var))
                    if (addIfNotExist)
                        var = page.SetVariable(varRef, null, false);

                if (expr is VariableTableExpression)
                {
                    ((VariableTable)var).ActiveIndexer = ((VariableTableExpression)expr).Indexer;
                }
                return var;
            }
            catch (Exception ex)
            {
                Logger.Error<TriggerReader>(ex);
                throw new TriggerReaderException($"No value found at {lastPos}");
            }
        }

        /// <summary>
        /// Reads the next Variable table available and the key if there is one, throws TriggerReaderException on failure
        /// </summary>
        /// <param name="addIfNotExist">Add the Variable if it doesn't exist and return that Variable with a Value equal to null.</param>
        /// <returns>Variable</returns>
        /// <exception cref="TriggerReaderException"></exception>
        public VariableTable ReadVariableTable(bool addIfNotExist = false)
        {
            if (contents.Count == 0) throw new TriggerReaderException("Unexpected end of values");

            if ((contents.Peek() is VariableExpression))
            {
                try
                {
                    var var = Variable.NoValue;
                    var expr = (contents.Dequeue() as VariableExpression);
                    lastPos = expr.Position;
                    string varRef = expr.Value;
                    if (!page.HasVariable(varRef, out var))
                        if (addIfNotExist)
                        {
                            var = page.SetVariableTable(varRef, false);
                            return var as VariableTable;
                        }
                    return var is VariableTable ? (VariableTable)var : null;
                }
                catch (Exception ex)
                {
                    Logger.Error<TriggerReader>(ex);
                    throw new TriggerReaderException($"No value found at {lastPos}");
                }
            }
            else if (contents.Peek() is VariableTableExpression)
            {
                try
                {
                    IVariable var;
                    var expr = (contents.Dequeue() as VariableTableExpression);
                    lastPos = expr.Position;
                    string varRef = expr.Value;
                    if (!page.HasVariable(varRef, out var))
                        if (addIfNotExist)
                            var = page.SetVariableTable(varRef, false);

                    if (var is VariableTable && expr.HasIndex) ((VariableTable)var).ActiveIndexer = expr.Indexer;
                    return var as VariableTable;
                }
                catch (Exception ex)
                {
                    throw new TriggerReaderException($"No value found at {lastPos}");
                }
            }
            else throw new TriggerReaderException($"Expected variable table, got {contents.Peek().GetType().Name} at {contents.Peek().Position}");
        }

        /// <summary>
        /// Peeks at the next value
        /// </summary>
        /// <returns></returns>
        public bool PeekVariable()
        {
            if (contents.Count == 0) return false;
            return contents.Peek() is VariableExpression;
        }

        /// <summary>
        /// Peeks at the next value, if the next value is not of <typeparamref name="T"/>, returns false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool PeekVariable<T>()
        {
            if (contents.Count == 0) return false;
            var expr = contents.Peek() as VariableExpression;
            return expr != null && page.HasVariable(expr.Value, out IVariable var) ? var.Value is T : false;
        }

        /// <summary>
        /// Peeks at the next value
        /// </summary>
        /// <returns></returns>
        public bool PeekVariableTable()
        {
            if (contents.Count == 0) return false;
            // TODO VariableTableExpression
            return contents.Peek() is VariableTableExpression;
        }

        /// <summary>
        /// Trys to read the next Variable available
        /// </summary>
        /// <param name="var">Variable is assigned on success</param>
        /// <param name="addIfNotExist"></param>
        /// <returns>bool on success</returns>
        public bool TryReadVariable(out IVariable var, bool addIfNotExist = false)
        {
            if (!PeekVariable())
            {
                var = Variable.NoValue;
                return false;
            }
            var = ReadVariable(addIfNotExist);
            return true;
        }

        /// <summary>
        /// Reads the next Number available, throws TriggerReaderException on failure
        /// </summary>
        /// <returns>Number</returns>
        /// <exception cref="TriggerReaderException"></exception>
        public double ReadNumber()
        {
            if (contents.Count == 0) throw new TriggerReaderException("Unexpected end of values");

            if (contents.Peek() is NumberExpression)
            {
                return (contents.Dequeue() as NumberExpression).Value;
            }
            else if (contents.Peek() is VariableExpression)
            {
                return (double)ReadVariable()?.Value?.As<double>();
            }
            else if (contents.Peek() is VariableTableExpression)
            {
                var table = ReadVariableTable();
                return table[table.ActiveIndexer].As<double>();
            }
            else throw new TriggerReaderException($"Expected number, got {contents.Peek().GetType().Name} at {contents.Peek().Position}");
        }

        /// <summary>
        /// Peeks at the next value
        /// </summary>
        /// <returns></returns>
        public bool PeekNumber()
        {
            if (contents.Count == 0) return false;
            return contents.Peek() is NumberExpression;
        }

        /// <summary>
        /// Tries the read a number from the trigger.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>true on success; otherwise false</returns>
        public bool TryReadNumber(out double number)
        {
            if (!PeekNumber())
            {
                number = -1d;
                return false;
            }
            number = ReadNumber();
            return true;
        }
    }
}