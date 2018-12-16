using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace WpfApp1
{
    class ServerController {
		private string serverUrl = "https://white-crow-api.herokuapp.com";

		public delegate void LoginSuccess(object sender, LoginEventArgs e);
		public delegate void GetPCsInfo(object sender, PCsInfoEventArgs e);
		public delegate void ConnectPCSuccess(object sender, ConnectedEventArgs e);
		public delegate void UpdateEvent(object sender, UpdateEventArgs e);

		public event LoginSuccess Logged;
		public event GetPCsInfo PCsInfoReceived;
		public event ConnectPCSuccess PCConnected;
		public event UpdateEvent Updated;

		public async Task LoginPost(string email, string password) {
			var request = WebRequest.Create(serverUrl + "/api/v0.0/login");
			request.Method = "POST";

			var loginParam = new LoginPostType(email, password);
		

			string rawData = JsonConvert.SerializeObject(loginParam);
			Console.WriteLine(rawData);

            byte[] data = Encoding.UTF8.GetBytes(rawData);
			request.ContentType = "application/json";
			request.ContentLength = data.Length;

			using (Stream dataStream = request.GetRequestStream()) {
				dataStream.Write(data, 0, data.Length);
            }

			WebResponse response = await request.GetResponseAsync();
			string token;
			using (Stream stream = response.GetResponseStream()) {
				using (StreamReader reader = new StreamReader(stream)) {
					string responseData = reader.ReadToEnd();
					token = JsonConvert.DeserializeObject<LoginRespType>(responseData).token;
					
				}
			}
			response.Close();
			
			Logged?.Invoke(this, new LoginEventArgs(token));
			
        }

		public async Task GetPCPost(string token) {
			var request = WebRequest.Create(serverUrl + "/api/v0.0/pc/get");
			request.Method = "POST";

			string rawData = "{\"token\":\"" + token + "\"}";
			Console.WriteLine(rawData);

			byte[] data = Encoding.UTF8.GetBytes(rawData);
			request.ContentType = "application/json";
			request.ContentLength = data.Length;

			using (Stream dataStream = request.GetRequestStream()) {
				dataStream.Write(data, 0, data.Length);
			}

			WebResponse response = await request.GetResponseAsync();
			PCSRespType responsePCS;
			using (Stream stream = response.GetResponseStream())
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string responseData = reader.ReadToEnd();
					Console.WriteLine(responseData);
					responsePCS = JsonConvert.DeserializeObject<PCSRespType>(responseData);
				}
			}
			response.Close();
			if (responsePCS != null) {
				PCsInfoReceived?.Invoke(this, new PCsInfoEventArgs(responsePCS));
			}
        }

		public async Task AddPCPost(string token, string name, int type) {
			var request = WebRequest.Create(serverUrl + "/api/v0.0/pc/add");
			request.Method = "POST";

			string rawData = JsonConvert.SerializeObject(new AddPCPostType(token, name, type));
			Console.WriteLine(rawData);

			byte[] data = Encoding.UTF8.GetBytes(rawData);
			request.ContentType = "application/json";
			request.ContentLength = data.Length;

			using (Stream dataStream = request.GetRequestStream())
			{
				dataStream.Write(data, 0, data.Length);
			}

			WebResponse response = await request.GetResponseAsync();
			response.Close();
        }

		public async Task ConnectPost(string token, long id) {
			var request = WebRequest.Create(serverUrl + "/api/v0.0/pc/connect");
			request.Method = "POST";

			string rawData = JsonConvert.SerializeObject(new ConnectPostType(token, id));
			Console.WriteLine(rawData);

			byte[] data = Encoding.UTF8.GetBytes(rawData);
			request.ContentType = "application/json";
			request.ContentLength = data.Length;

			using (Stream dataStream = request.GetRequestStream())
			{
				dataStream.Write(data, 0, data.Length);
			}

            WebResponse response = await request.GetResponseAsync();
			string pcToken;
			using (Stream stream = response.GetResponseStream())
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string responseData = reader.ReadToEnd();
					Console.WriteLine(responseData);
					pcToken = JsonConvert.DeserializeObject<ConnectRespType>(responseData).token;
				}
			}
			response.Close();

			PCConnected?.Invoke(this, new ConnectedEventArgs(pcToken));
        }

		public async Task Update(string pcToken) {
			var request = WebRequest.Create(serverUrl + "/api/v0.0/pc/update");
			request.Method = "POST";

			string rawData = $"{{\"token\":\"{pcToken}\"}}";
			Console.WriteLine(rawData);

			byte[] data = Encoding.UTF8.GetBytes(rawData);
			request.ContentType = "application/json";
			request.ContentLength = data.Length;

			using (Stream dataStream = request.GetRequestStream())
			{
				dataStream.Write(data, 0, data.Length);
			}

			WebResponse response = await request.GetResponseAsync();
			
			UpdateRespType resp;
			using (Stream stream = response.GetResponseStream())
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string responseData = reader.ReadToEnd();
					Console.WriteLine(responseData);
					resp = JsonConvert.DeserializeObject<UpdateRespType>(responseData);
				}
			}
			Updated?.Invoke(this, new UpdateEventArgs(resp));
        }
	}

	
}
