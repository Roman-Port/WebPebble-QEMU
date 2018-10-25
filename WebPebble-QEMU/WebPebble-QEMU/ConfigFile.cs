using System;
using System.Collections.Generic;
using System.Text;

namespace WebPebble_QEMU
{
    class ConfigFile
    {
        public string qemu_binary;

        public Dictionary<string, FlashPair> flash_bins;
    }

    class FlashPair
    {
        public string micro_flash;
        public string spi_flash;
    }
}
