using System;
using System.IO;
using System.Text;

namespace Monkeyspeak
{
    [Serializable()]
    public class SStreamReader : StreamReader
    {
        private long _position;

        #region Constructors

        public SStreamReader(Stream stream) : base(stream)
        {
            if (IsPreamble())
            {
                _position = this.CurrentEncoding.GetPreamble().Length;
            }
        }

        public SStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks) : base(stream, detectEncodingFromByteOrderMarks)
        {
            if (IsPreamble())
            {
                _position = this.CurrentEncoding.GetPreamble().Length;
            }
        }

        public SStreamReader(Stream stream, Encoding encoding) : base(stream, encoding)
        {
            if (IsPreamble())
            {
                _position = this.CurrentEncoding.GetPreamble().Length;
            }
        }

        public SStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(stream, encoding, detectEncodingFromByteOrderMarks)
        {
        }

        public SStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
        }

        public SStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen)
        {
        }

        public SStreamReader(string path) : base(path)
        {
            if (IsPreamble())
            {
                _position = this.CurrentEncoding.GetPreamble().Length;
            }
        }

        public SStreamReader(string path, bool detectEncodingFromByteOrderMarks) : base(path, detectEncodingFromByteOrderMarks)
        {
            if (IsPreamble())
            {
                _position = this.CurrentEncoding.GetPreamble().Length;
            }
        }

        public SStreamReader(string path, Encoding encoding) : base(path, encoding)
        {
            if (IsPreamble())
            {
                _position = this.CurrentEncoding.GetPreamble().Length;
            }
        }

        public SStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(path, encoding, detectEncodingFromByteOrderMarks)
        {
            if (IsPreamble())
            {
                _position = this.CurrentEncoding.GetPreamble().Length;
            }
        }

        public SStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
            if (IsPreamble())
            {
                _position = this.CurrentEncoding.GetPreamble().Length;
            }
        }

        #endregion Constructors

        /// <summary>
        /// Encoding can really haven't preamble
        /// </summary>
        public bool IsPreamble()
        {
            byte[] preamble = this.CurrentEncoding.GetPreamble();
            bool res = true;
            for (int i = 0; i < preamble.Length; i++)
            {
                int dd = base.BaseStream.ReadByte();
                if (preamble[i] != dd)
                {
                    res = false;
                    break;
                }
            }
            Position = 0;
            return res;
        }

        /// <summary>
        /// Use this property for get and set real position in file.
        /// Position in BaseStream can be not right.
        /// </summary>
        public long Position
        {
            get { return _position; }
            set
            {
                _position = base.BaseStream.Seek(value, SeekOrigin.Begin);
                this.DiscardBufferedData();
            }
        }

        public override int Read()
        {
            var ch = base.Read();
            _position += CurrentEncoding.GetByteCount(new char[] { (char)ch });
            return ch;
        }

        public override string ReadLine()
        {
            string line = base.ReadLine();
            if (line != null)
            {
                _position += CurrentEncoding.GetByteCount(line);
            }
            _position += CurrentEncoding.GetByteCount(Environment.NewLine);
            return line;
        }
    }
}