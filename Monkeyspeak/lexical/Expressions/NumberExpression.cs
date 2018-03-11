using System;
using System.Collections.Generic;
using System.IO;

namespace Monkeyspeak.Lexical.Expressions
{
    public sealed class NumberExpression : Expression<double>
    {
        public NumberExpression(SourcePosition pos, string value) : base(pos, ParseNumber(value))
        {
        }

        public NumberExpression()
        {
        }

        private static double ParseNumber(string value)
        {
            double val = double.NaN;
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(value.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int iVal))
                    val = iVal;
            }
            else
            {
                double.TryParse(value, System.Globalization.NumberStyles.AllowDecimalPoint
                 | System.Globalization.NumberStyles.AllowLeadingSign
                 | System.Globalization.NumberStyles.AllowThousands
                 | System.Globalization.NumberStyles.AllowExponent, null, out val);
            }
            return val;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(GetValue<double>());
        }

        public override void Read(BinaryReader reader)
        {
            SetValue(reader.ReadDouble());
        }

        public override object Execute(Page page, Queue<IExpression> contents, bool addToPage = false)
        {
            return GetValue<double>();
        }

        public override string ToString()
        {
            return GetValue<double>().ToString();
        }
    }
}