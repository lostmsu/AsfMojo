using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsfMojo.Media
{
    /// <summary>
    /// Fluent interface to create a WaveMemoryStream from a file, start- and end-offset
    /// </summary>
    public interface IWaveMemoryStreamProperties
    {
        string FileName { get; set; }
        double? StartOffset { get; set; }
        double? EndOffset { get; set; }

        /// <summary>
        /// Sets the start offset of the wave stream
        /// </summary>
        IWaveMemoryStreamProperties From(double offset);
    }



    internal class WaveMemoryStreamProperties : IWaveMemoryStreamProperties
    {
        public string FileName { get; set; }
        public double? StartOffset { get; set; }
        public double? EndOffset { get; set; }

        /// <summary>
        /// Sets the start offset of the wave stream
        /// </summary>
        public IWaveMemoryStreamProperties From(double offset)
        {
            StartOffset = offset;
            return this;
        }
    }
}
