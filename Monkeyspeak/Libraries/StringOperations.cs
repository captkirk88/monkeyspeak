using System;
using Monkeyspeak.Extensions;

namespace Monkeyspeak.Libraries
{
    public class StringOperations : AutoIncrementBaseLibrary
    {
        public override int BaseId => 400;

        public override void Initialize(params object[] args)
        {
            Add(TriggerCategory.Effect, PutWordCountIntoVariable,
                "with {...} get word count and put it into variable %.");
            Add(TriggerCategory.Effect, SubStringToVar,
                "with {...} get words starting at # to # and put it into variable %.");
            Add(TriggerCategory.Effect, IndexOfStringToVar,
                "with {...} get index of {...} and put it into variable %.");
            Add(TriggerCategory.Effect, ReplaceStringToVar,
                "with {...} replace all occurances of {...} with {...} and put it into variable %.");
            Add(TriggerCategory.Effect, LeftOfStringToVar,
                "with {...} get everything left of {...} and put it into variable %");
            Add(TriggerCategory.Effect, RightMostLeftOfStringToVar,
                "with {...} get everything right most left of {...} and put it into variable %");
            Add(TriggerCategory.Effect, RightOfStringToVar,
                "with {...} get everything right of {...} and put it into variable %");
            Add(TriggerCategory.Effect, RightMostRightOfStringToVar,
                "with {...} get everything right most right of {...} and put it into variable %");
            Add(TriggerCategory.Effect, SplitStringIntoTable,
                "with {...} split it at each {...} and put it into table %");
            Add(TriggerCategory.Effect, AddToVariable,
                "take variable % and add {...} to the end,");
        }

        [TriggerDescription("Takes a variable and adds a string to the end of it")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool AddToVariable(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var str = reader.ReadString();
            var newVal = $"{var.AsString("")}{str}";
            var.Value = newVal;
            return true;
        }

        [TriggerDescription("Splits the specified string with the delimiter and puts the result into variable")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool SplitStringIntoTable(TriggerReader reader)
        {
            var str = reader.ReadString();
            var search = reader.ReadString();
            var split = str.Split(new string[] { search }, StringSplitOptions.RemoveEmptyEntries);

            var var = reader.ReadVariableTable(true);
            var.AddRange(split);
            return true;
        }

        [TriggerDescription("Gets everything right most right of the specified search string and puts the result into variable")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool RightMostRightOfStringToVar(TriggerReader reader)
        {
            var str = reader.ReadString();
            var search = reader.ReadString();
            var newStr = str.RightMostRightOf(search);

            var var = reader.ReadVariable(true);
            var.Value = newStr;
            return true;
        }

        [TriggerDescription("Gets everything right of the specified search string and puts the result into variable")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool RightOfStringToVar(TriggerReader reader)
        {
            var str = reader.ReadString();
            var search = reader.ReadString();
            var newStr = str.RightOf(search);

            var var = reader.ReadVariable(true);
            var.Value = newStr;
            return true;
        }

        [TriggerDescription("Gets everything right most left of the specified search string and puts the result into variable")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool RightMostLeftOfStringToVar(TriggerReader reader)
        {
            var str = reader.ReadString();
            var search = reader.ReadString();
            var newStr = str.RightMostLeftOf(search);

            var var = reader.ReadVariable(true);
            var.Value = newStr;
            return true;
        }

        [TriggerDescription("Gets everything left of the specified search string and puts the result into variable")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool LeftOfStringToVar(TriggerReader reader)
        {
            var str = reader.ReadString();
            var search = reader.ReadString();
            var newStr = str.LeftOf(search);

            var var = reader.ReadVariable(true);
            var.Value = newStr;
            return true;
        }

        [TriggerDescription("Replaces all occurances of the specified string and puts the result into variable")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool ReplaceStringToVar(TriggerReader reader)
        {
            var str = reader.ReadString();
            var search = reader.ReadString();
            var replace = reader.ReadString();
            var newStr = new System.Text.RegularExpressions.Regex($@"\b{search}\b").Replace(str, replace);

            var var = reader.ReadVariable(true);
            var.Value = newStr;
            return true;
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
            var subStr = str.Slice((int)start, (int)end);
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
            var.Value = index.AsDouble();
            return true;
        }

        [TriggerDescription("Gets the word count and puts it into the variable")]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool PutWordCountIntoVariable(TriggerReader reader)
        {
            string[] words = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var var = reader.ReadVariable(true);
            var.Value = words.Length.AsDouble();
            return true;
        }

        public override void Unload(Page page)
        {
        }
    }
}