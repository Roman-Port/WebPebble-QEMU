using System;
using System.Collections.Generic;
using System.Text;

namespace WebPebble_QEMU.Ws
{
    class WebRequest
    {
        public WebRequestType type;
        public Dictionary<string, object> data = new Dictionary<string, object>();
    }

    enum WebRequestType
    {
        KeyPress,
        BootNew,
        ContinueOld
    }
}
