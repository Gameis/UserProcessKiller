using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
	public class LoginEventArgs {
		public LoginEventArgs(string token) {
			this.token = token;
		}

		public string token;
	}
	
	public class PCsInfoEventArgs {
		public PCsInfoEventArgs(PCSRespType PCsCotainer) {
			this.PCsCotainer = PCsCotainer;
		}
		public PCSRespType PCsCotainer;
	}

	public class ConnectedEventArgs {
		public ConnectedEventArgs(string token) {
			this.token = token;
		}

		public string token;
	}

	public class UpdateEventArgs {
		public UpdateEventArgs(bool isUse, long end) {
			this.isUse = isUse;
			this.end = end;
		}

		public UpdateEventArgs(UpdateRespType urt) {
			isUse = urt.isUse;
			end = urt.end;
		}
		public bool isUse;
		public long end;
    }
}
