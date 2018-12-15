using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
	public class LoginPostType
	{
		public LoginPostType(string mail, string pass)
		{
			email = mail;
			password = pass;
		}
		public string email    { get; set; }
		public string password { get; set; }
	}

	public class LoginRespType {
		public string token;
	}

	public class PCInfoType {
		public long id;
		public string name;
		public int type;
	}

	public class PCSRespType {
		public PCInfoType[] pc;
	}

	public class AddPCPostType {
		public AddPCPostType(string token, string name, int type) {
			this.token = token;
			this.name = name;
			this.type = type;
		}

		public string token, name;
		public int type;
	}

	public class ConnectPostType {
		public ConnectPostType(string token, long id) {
			this.token = token;
			this.id = id;
		}

		public string token;
		public long id;
	}

	public class ConnectRespType {
		public string token;
	}

	public class UpdateRespType {
		public bool isUse;
		public long end;
	}
}
