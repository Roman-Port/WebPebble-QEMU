using System;
using System.Collections.Generic;
using System.Text;

namespace WebPebble_QEMU
{
    class ConfigFile
    {
        public string qemu_binary;
        public string pypkjs_binary;

        public string persist_dir;

        public Dictionary<string, FlashPair> flash_bins;

        public int private_port_start;
    }

    class FlashPair
    {
        public string micro_flash;
        public string spi_flash;
        public string layouts;
        public string[] args;
    }
}
