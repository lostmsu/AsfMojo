using System;

namespace AsfMojo.Media
{
    /// <summary>
    /// Create Image from an underlying ASF stream
    /// </summary>
    public sealed class AsfImage : IDisposable
    {
        private AsfStream _asfStream;
        private AsfIStream _asfMemoryStream;

        public static IAsfImageProperties FromFile(string fileName)
        {
            return new AsfImageProperties() { FileName = fileName };
        }

        public AsfImage(AsfStream asfStream) 
        {
            if (asfStream.StreamType != AsfStreamType.asfImage)
                throw new ArgumentException();

            _asfStream = asfStream;
            _asfMemoryStream = null;
        }
        
        public void Dispose()
        {
            if (_asfMemoryStream != null)
            {
                _asfMemoryStream.Close();
                _asfMemoryStream = null;
            }

            if (_asfStream != null)
                _asfStream.Close();
        }
    }
}
