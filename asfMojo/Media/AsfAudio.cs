using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace AsfMojo.Media
{
    /// <summary>
    /// A single PCM audio sample
    /// </summary>
    public class AudioSample
    {
        private float[] _sample;

        public AudioSample(float[] sample)
        {
            _sample = new float[sample.Length];
            Array.Copy(sample, _sample, sample.Length);
        }

        public float Left { get { return _sample[0]; } }
        public float Right { get { return _sample[1]; } }
    }

    /// <summary>
    /// Audio extractor based on an underlying ASF stream
    /// </summary>
    public sealed class AsfAudio : IDisposable
    {
        public AsfStream BaseStream { get { return _asfStream; } }

        private bool _disposed = false;
        private AsfStream _asfStream;
        private AsfIStream? _asfMemoryStream;

        Queue<AudioSample> _sampleBuffer;

        public AsfAudio(AsfStream asfStream)
        {
            _asfStream = asfStream;
            _asfMemoryStream = null;
            _sampleBuffer = new Queue<AudioSample>();
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) //managed resources
                {
                    if (_asfMemoryStream != null)
                    {
                        _asfMemoryStream.Close();
                        _asfMemoryStream = null;
                    }

                    if (_asfStream != null)
                        _asfStream.Close();
                }
                _disposed = true;
            }
        }

        ~AsfAudio()
        {
            Dispose(false);
        }
    }
}
