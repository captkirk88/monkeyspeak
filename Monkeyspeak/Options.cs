using Monkeyspeak.Logging;
using System;

namespace Monkeyspeak
{
    public class Options
    {
        public Options()
        {
            // defaults
            CanOverrideTriggerHandlers = false;
            StringBeginSymbol = '{';
            StringEndSymbol = '}';
            VariableDeclarationSymbol = '%';
            LineCommentSymbol = '*';
            BlockCommentBeginSymbol = "/*";
            BlockCommentEndSymbol = "*/";
            TriggerLimit = 6000;
            VariableCountLimit = 1000;
            StringLengthLimit = 1000;
            VariableTableCountLimit = 1000;
            LoopLimit = 10000;
            Version = typeof(MonkeyspeakEngine).Assembly.GetName().Version;
        }

        /// <summary>
        /// Current assembly version used internally by the compiler to handle versioning
        /// </summary>
        public Version Version { get; internal set; }

        /// <summary>
        /// If set to TRUE then an existing TriggerHandler can be overridden by newer TriggerHandler
        /// <para>Default: false</para>
        /// </summary>
        public bool CanOverrideTriggerHandlers { get; set; }

        /// <summary>
        /// Beginning string literal symbol
        /// <para>Default: {</para>
        /// </summary>
        public char StringBeginSymbol { get; set; }

        /// <summary>
        /// Ending string literal symbol
        /// <para>Default: }</para>
        /// </summary>
        public char StringEndSymbol { get; set; }

        /// <summary>
        /// Variable literal symbol
        /// <para>Default: %</para>
        /// </summary>
        public char VariableDeclarationSymbol { get; set; }

        /// <summary>
        /// Comment literal symbol
        /// <para>Default: *</para>
        /// </summary>
        public char LineCommentSymbol { get; set; }

        /// <summary>
        /// Block Comment beginning literal symbol
        /// <para>Default: *</para>
        /// </summary>
        public string BlockCommentBeginSymbol { get; set; }

        /// <summary>
        /// Block Comment ending literal symbol
        /// <para>Default: *</para>
        /// </summary>
        public string BlockCommentEndSymbol { get; set; }

        /// <summary>
        /// Used to limit the maximum amount of triggers per page
        /// <para>Default: 6000</para>
        /// </summary>
        public int TriggerLimit { get; set; }

        /// <summary>
        /// Used to limit the maximum length of a string
        /// <para>Default: Int32.MaxValue</para>
        /// </summary>
        public int StringLengthLimit { get; set; }

        /// <summary>
        /// Used to limit the maximum amount of variables per page
        /// <para>Default: 1000</para>
        /// </summary>
        public int VariableCountLimit { get; set; }

        /// <summary>
        /// Gets or sets the variable table count limit.
        /// </summary>
        /// <value>
        /// The variable table count limit.
        /// </value>
        public int VariableTableCountLimit { get => VariableTable.Limit; set => VariableTable.Limit = value; }

        /// <summary>
        /// Gets or sets the loop limit.
        /// </summary>
        /// <value>
        /// The loop limit.
        /// </value>
        public int LoopLimit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether debug log output is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if debug; otherwise, <c>false</c>.
        /// </value>
        public bool Debug { get => Logger.DebugEnabled; set => Logger.DebugEnabled = value; }
    }
}