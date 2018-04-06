using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Messageing
{
    internal sealed class Awaiter
    {
        private volatile TaskCompletionSource<byte> _waiting;

        public void Pulse()
        {
            var w = _waiting;
            if (w == null)
            {
                return;
            }
            _waiting = null;
            Task.Run(() => w.TrySetResult(1));
        }

        //This method is not thread safe and can only be called by one thread at a time.
        // To make it thread safe put a lock around the null check and the assignment,
        // you do not need to have a lock on Pulse, "volatile" takes care of that side.
        public Task Wait()
        {
            if (_waiting != null)
                throw new InvalidOperationException("Only one waiter is allowed to exist at a time!");
            _waiting = new TaskCompletionSource<byte>();
            return _waiting.Task;
        }
    }

}
