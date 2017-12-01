using Monkeyspeak.Extensions;
using Monkeyspeak.Lexical;
using Monkeyspeak.Lexical.Expressions;
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

        public bool HasMore { get => contents != null && contents.Count > 0; }

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
        ///
        /// <para>
        /// See also <seealso cref="GetParameter{T}(int)"/>
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> on success; <c>false</c> otherwise</returns>
        public bool TryGetParameter<T>(int index, out T value)
        {
            if (args != null && args.Length > index)
            {
                value = (T)args[index];
                return true;
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// Gets the parameter.
        ///
        /// <para>
        /// See also <seealso cref="TryGetParameter{T}(int, out T)"/>
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T GetParameter<T>(int index = 0)
        {
            if (args != null && args.Length > index)
                return (T)args[index];
            return default(T);
        }

        /// <summary>
        /// Gets the parameters of a certain type.
        ///
        /// <para>
        /// See also <seealso cref="TryGetParameter{T}(int, out T)"/>
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetParametersOfType<T>()
        {
            if (args != null && args.Length > 0)
                return args.OfType<T>().ToArray();
            return new T[0];
        }

        /// <summary>
        /// Gets the parameters of a certain type.
        ///
        /// <para>
        /// See also <seealso cref="TryGetParameter{T}(int, out T)"/>
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<object> EnumerateParameters()
        {
            if (args != null && args.Length > 0)
                return args.AsEnumerable();
            return Enumerable.Empty<object>();
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
            if (contents.Peek().GetType() != Expressions.Instance[TokenType.STRING_LITERAL]) throw new TriggerReaderException($"Expected string, got {contents.Peek().GetType().Name} at {contents.Peek().Position}");
            return (string)contents.Dequeue().Execute(page, contents, processVariables);
        }

        /// <summary>
        /// Peeks at the next value
        /// </summary>
        /// <returns></returns>
        public bool PeekString()
        {
            if (contents.Count == 0) return false;
            return contents.Peek().GetType() == Expressions.Instance[TokenType.STRING_LITERAL];
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
            if (contents == null || contents.Count == 0) throw new TriggerReaderException("Unexpected end of values");
            if (contents.Peek().GetType() != Expressions.Instance[TokenType.VARIABLE] &&
                contents.Peek().GetType() != Expressions.Instance[TokenType.TABLE]) throw new TriggerReaderException($"Expected variable, got {contents.Peek().GetType().Name} at {contents.Peek().Position}");
            return (IVariable)contents.Dequeue().Execute(page, contents, addIfNotExist);
        }

        /// <summary>
        /// Trys to read the next Variable table available
        /// </summary>
        /// <param name="table">Variable table is assigned on success</param>
        /// <param name="addIfNotExist"></param>
        /// <returns>bool on success</returns>
        public bool TryReadVariableTable(out VariableTable table, bool addIfNotExist = false)
        {
            if (!PeekVariable())
            {
                table = VariableTable.Empty;
                return false;
            }
            table = ReadVariableTable(addIfNotExist);
            return true;
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

            if (contents.Peek().GetType() == Expressions.Instance[TokenType.VARIABLE])
            {
                return ((IVariable)contents.Dequeue().Execute(page, contents, addIfNotExist)).ConvertToTable(page);
            }
            else if (contents.Peek().GetType() == Expressions.Instance[TokenType.TABLE])
            {
                return (VariableTable)contents.Dequeue().Execute(page, contents, addIfNotExist);
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
            return contents.Peek().GetType() == Expressions.Instance[TokenType.VARIABLE];
        }

        /// <summary>
        /// Peeks at the next value, if the next value is not of <typeparamref name="T"/>, returns false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool PeekVariable<T>()
        {
            if (contents.Count == 0) return false;
            var expr = contents.Peek();
            if (expr.GetType() == Expressions.Instance[TokenType.VARIABLE])
                return page.HasVariable(expr.GetValue<string>(), out IVariable var) ? var.Value is T : false;
            return false;
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

            if (contents.Peek().GetType() == Expressions.Instance[TokenType.NUMBER])
            {
                return (double)contents.Dequeue().Execute(page, contents);
            }
            else if (contents.Peek().GetType() == Expressions.Instance[TokenType.VARIABLE])
            {
                return (double)ReadVariable()?.Value?.AsDouble();
            }
            else if (contents.Peek().GetType() == Expressions.Instance[TokenType.TABLE])
            {
                var table = ReadVariableTable();
                return table[table?.ActiveIndexer].AsDouble();
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
            return contents.Peek().GetType() == Expressions.Instance[TokenType.NUMBER] || PeekVariable<double>();
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

        /// <summary>
        /// Reads a undefined amount of values.  This is usually used for variable arguments on the end of a trigger.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> ReadValues()
        {
            while (HasMore)
            {
                if (PeekNumber())
                {
                    double num = 0;
                    try
                    {
                        num = ReadNumber();
                    }
                    catch (Exception ex) { Logger.Error<TriggerReader>(ex); continue; }
                    yield return num;
                }
                else if (PeekString())
                {
                    string str = null;
                    try
                    {
                        str = ReadString();
                    }
                    catch (Exception ex) { Logger.Error<TriggerReader>(ex); continue; }
                    yield return str;
                }
                else if (PeekVariable())
                {
                    IVariable var = null;
                    try
                    {
                        var = ReadVariable();
                    }
                    catch (Exception ex) { Logger.Error<TriggerReader>(ex); continue; }
                    yield return var;
                }
            }
        }
    }
}