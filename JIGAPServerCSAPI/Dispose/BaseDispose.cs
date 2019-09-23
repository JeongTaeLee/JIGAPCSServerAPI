using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI
{
    public class BaseDispose : IDisposable
    {
        protected bool _disposed = false;

        ~BaseDispose()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool inDisposing)
        {
            if (_disposed) return;

            if (inDisposing)
            {
                // 관리되는 객체
                ManagedDispose();
            }
            // 관리되지 않은 객체.
            UnManageDispose();

            _disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void ManagedDispose() { }
        protected virtual void UnManageDispose() { }
    }
}
