using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class TestPacket : JIGAPServerCSAPI.BasePacket
    {
        public void Initialize()
        {
            string str = "Hello World";

            Array.Copy(BitConverter.GetBytes(15), 0, _buffer.Array, 0, sizeof(Int32));
            Array.Copy(Encoding.UTF8.GetBytes(str.ToArray()), 0, _buffer.Array, 4, str.Length);

            Array.Copy(BitConverter.GetBytes(15), 0, _buffer.Array, 15, sizeof(Int32));
            Array.Copy(Encoding.UTF8.GetBytes(str.ToArray()), 0, _buffer.Array, 19, str.Length);
            _writePosition = 30;
        }
    }
}
