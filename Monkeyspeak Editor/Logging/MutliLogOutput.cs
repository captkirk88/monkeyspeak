using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Logging
{
    public class MultiLogOutput : ILogOutput
    {
        private List<ILogOutput> outputs;

        public MultiLogOutput(params ILogOutput[] outputs)
        {
            this.outputs = new List<ILogOutput>(outputs);
        }

        public void Log(LogMessage logMsg)
        {
            foreach (var output in outputs) output.Log(logMsg);
        }

        public IEnumerable<ILogOutput> Outputs => outputs;

        public void Add(params ILogOutput[] outputs)
        {
            this.outputs.AddRange(outputs);
        }

        public void Remove(ILogOutput output)
        {
            outputs.Remove(output);
        }
    }
}