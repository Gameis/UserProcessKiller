using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class KeyBlockParams {
		public KeyBlockParams(int vkCode, int flags) {
			VkCode = vkCode;
			Flags = flags;
		}
		public int VkCode;
		public int Flags;
		public bool isAcive = true;
	}
}
