using System.Collections.Generic;

namespace Monkeyspeak.Lexical.Expressions
{
    public interface IExpression : ICompilable
    {
        /// <summary>
        /// Gets the position in the source code.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        SourcePosition Position { get; }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">The value.</param>
        void SetValue(object value);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetValue<T>();

        /// <summary>
        /// Applies the Expression contents to the specified trigger.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <returns>[true] if contents were applied, [false] otherwise</returns>
        void Apply(Trigger? currentTrigger);

        object Execute(Page page, Queue<IExpression> contents, bool addToPage = false);
    }
}