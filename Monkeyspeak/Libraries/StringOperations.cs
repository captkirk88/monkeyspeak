using System;

namespace Monkeyspeak.Libraries
{
    public class StringOperations : BaseLibrary
    {
        public override void Initialize(params object[] args)
        {
            Add(TriggerCategory.Effect, 400, PutWordCountIntoVariable,
                "with {...} get word count and set it to variable %.");
            Add(TriggerCategory.Effect, 401, AddStringToVar,
                "with {...} add it to variable %.");
            Add(TriggerCategory.Effect, 402, SubStringToVar,
                "with {...} get words starting at # to # and set it to variable %.");
            Add(TriggerCategory.Effect, 403, IndexOfStringToVar,
                "with {...} get index of {...} and set it to variable %.");
        }

        [TriggerDescription("Gets the words between a range and puts them into a variable")]
        [TriggerStringParameter]
        [TriggerNumberParameter]
        [TriggerNumberParameter]
        [TriggerVariableParameter]
        private bool SubStringToVar(TriggerReader reader)
        {
            var str = reader.ReadString();
            var start = reader.ReadNumber();
            var end = reader.ReadNumber();
            var var = reader.ReadVariable(true);
            var subStr = str.Substring((int)start, (int)end);
            var.Value = subStr;
            return true;
        }

        [TriggerDescription("Gets the position of the specified string in the original string")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        private bool IndexOfStringToVar(TriggerReader reader)
        {
            var str = reader.ReadString();
            var search = reader.ReadString();
            var var = reader.ReadVariable(true);
            var index = str.IndexOf(search);
            var.Value = index;
            return true;
        }

        [TriggerDescription("Adds the string to the variable")]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool AddStringToVar(TriggerReader reader)
        {
            string str = reader.ReadString();
            var var = reader.ReadVariable();
            var.Value = var.Value + str;
            return true;
        }

        [TriggerDescription("Gets the word count and puts it into the variable")]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool PutWordCountIntoVariable(TriggerReader reader)
        {
            string[] words = reader.ReadString().Split(' ');
            var var = reader.ReadVariable(true);
            var.Value = words.Length;
            return true;
        }

        public override void Unload(Page page)
        {
        }
    }
}