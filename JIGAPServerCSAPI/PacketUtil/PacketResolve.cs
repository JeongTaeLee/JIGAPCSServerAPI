using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI
{
    public class PacketResolve
    {
        private byte[]  _buffer = null;
        private int     _maxBufferSize  = 0;
        private int     _writePosition  = 0;
        private bool    _readHeader     = false;
        private int     _packetSize = 0;
        public PacketResolve(int _bufferTotalSize)
        {
            _maxBufferSize = _bufferTotalSize;
            _buffer = new byte[_maxBufferSize];
        }

        public virtual void PacketCheck(byte[] inBuffer, int inOffset, int inBytesTransferred, Action<byte[], int, int> inCompleteAction)
        {
            int paramBufferPosition = 0; 

            while (true)
            {    
                if (_readHeader == false)
                {
                    if ((inBytesTransferred - paramBufferPosition)>= sizeof(Int32))
                    {
                        ConnectPacketToBuffer(inBuffer, inOffset + paramBufferPosition, sizeof(Int32));
                        paramBufferPosition += sizeof(Int32);

                        _packetSize = BitConverter.ToInt32(_buffer, 0);

                        _readHeader = true;
                    }
                    else
                    {
                        ConnectPacketToBuffer(inBuffer, inOffset + paramBufferPosition, inBytesTransferred - paramBufferPosition);
                        break;
                    }
                }

                if (inBytesTransferred >= _packetSize)
                {
                    ConnectPacketToBuffer(inBuffer, inOffset + paramBufferPosition, (_packetSize - sizeof(Int32)));
                    paramBufferPosition += _packetSize - sizeof(Int32);
                }
                else
                {
                    ConnectPacketToBuffer(inBuffer, inOffset + paramBufferPosition, inBytesTransferred - paramBufferPosition);
                    break;
                }

                if (_writePosition == _packetSize)
                {
                    if (inCompleteAction != null)
                        inCompleteAction(_buffer, sizeof(Int32), _writePosition - sizeof(Int32));
                }

                if (paramBufferPosition < inBytesTransferred)
                { 
                    Array.Clear(_buffer, 0, _buffer.Length);

                    _readHeader = false;
                    _writePosition = 0;
                    _packetSize = 0;
                }
                else
                    break;
            }
        }
            
        public virtual void ConnectPacketToBuffer(byte[] inBuffer, int inOffset, int inBytesTransferred)
        {
            Array.Copy(inBuffer, inOffset, _buffer, _writePosition, inBytesTransferred);
            _writePosition += inBytesTransferred;
        }

    }
}
