using System;
using System.Collections.Generic;
using System.Text;

namespace WebPebble_QEMU.Ws
{
    class WebReply<T>
    {
        public T data;
        public WebReplyType type;
    }

    enum WebReplyType
    {
        OnStatusChange,
        OnFatalError,
        OnEmulatorBoot
    }
}
