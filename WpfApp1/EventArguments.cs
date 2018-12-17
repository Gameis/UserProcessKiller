using System;
using System.Collections;
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

	public class InvalidRequestEventArgs {
		private string      _toStringResult;
		public  string      Message;
		public  string      Source;
		public  IDictionary Data;
		public  string      MethodName;

        public InvalidRequestEventArgs(Exception info, string methodName) {
			Data = info.Data;
			Message = info.Message;
			_toStringResult = info.ToString();
			Source = info.Source;
			MethodName = methodName;
		}

		public override string ToString() {
			return _toStringResult;
		}
	}
}
