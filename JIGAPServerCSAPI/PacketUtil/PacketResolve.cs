using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace JIGAPServerCSAPI
{
    public class PacketResolve
    {
        private byte[]  _buffers            = null;
        private int     _totalBufSize       = 0;
        private int     _writingPosition    = 0;
        private int     _packetSize         = 0;
        private bool    _isReadHeader        = false;

        public PacketResolve(int _bufferTotalSize)
        {
            _totalBufSize = _bufferTotalSize;
            _buffers = new byte[_totalBufSize];
        }

        public virtual void PacketCheck(SocketAsyncEventArgs inArgs, byte[] inBuffer, int inOffset, int inBytesTransferred, Action<SocketAsyncEventArgs, byte[], int, int> inCompleteAction)
        {
            int readBufSize = 0;
           
            while (true)
            {    
                if (_isReadHeader == false)
                {
                    if ((inBytesTransferred - readBufSize) >= sizeof(Int32))
                    {
                        ConnectPacketToBuffer(inBuffer, inOffset + readBufSize, sizeof(Int32));
                        readBufSize += sizeof(Int32);

                        _packetSize = BitConverter.ToInt32(_buffers, 0);

                        _isReadHeader = true;
                    }
                    else
                    {
                        ConnectPacketToBuffer(inBuffer, inOffset + readBufSize, inBytesTransferred - readBufSize);
                        break;
                    }
                }

                if (inBytesTransferred - readBufSize >= (_packetSize - sizeof(Int32)) )
                {
                    ConnectPacketToBuffer(inBuffer, inOffset + readBufSize, (_packetSize - sizeof(Int32)));
                    readBufSize += _packetSize - sizeof(Int32);
                }
                else
                {
                    ConnectPacketToBuffer(inBuffer, inOffset + readBufSize, inBytesTransferred - readBufSize);
                    break;
                }

                if (_writingPosition == _packetSize)
                {
                    if (inCompleteAction != null)
                        inCompleteAction(inArgs, _buffers, sizeof(Int32), _writingPosition - sizeof(Int32));
                }

                Array.Clear(_buffers, 0, _buffers.Length);

                _isReadHeader = false;
                _writingPosition = 0;
                _packetSize = 0;

                
                if (readBufSize >= inBytesTransferred)
                    break;
            }
        }
            
        public virtual void ConnectPacketToBuffer(byte[] inBuffer, int inOffset, int inBytesTransferred)
        {
            if (inBuffer == null)
                return;

            if (inOffset < 0 || inBytesTransferred < 0)
                return;

            if (inBuffer.Length < inBytesTransferred)
                return;

            Array.Copy(inBuffer, inOffset, _buffers, _writingPosition, inBytesTransferred);
            _writingPosition += inBytesTransferred;
        }
        public void Clear()
        {
            Array.Clear(_buffers, 0, _buffers.Length);

            _isReadHeader = false;
            _writingPosition = 0;
            _packetSize = 0;
        }

    }
}
