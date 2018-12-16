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

		/// <summary>
        /// Событие, возникающее при ответе на запрос авторизации
        /// </summary>
		public event LoginSuccess Logged;
		/// <summary>
        /// Событие, возникающее при ответе на запрос получения списка пк
        /// </summary>
		public event GetPCsInfo PCsInfoReceived;
		/// <summary>
        /// Событие, возникающее при ответе на запрос подключения пк
        /// </summary>
		public event ConnectPCSuccess PCConnected;
		/// <summary>
        /// Событие, возникающее при ответе на запрос обновления состояния
        /// </summary>
		public event UpdateEvent Updated;

        /// <summary>
        /// Запрос на авторизацию администратора
        /// </summary>
        /// <param name="email"> Логин для авторизации на сервере (тестовый = log)</param>
        /// <param name="password"> Пароль для авторизации на сервере (тестовый = pass)</param>
        /// <returns> Вызывает событие Logged, в которое передает Токен полученный с сервера</returns>
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

        /// <summary>
        /// Запрос на получение списка ПК
        /// </summary>
        /// <param name="token"> Токен администратора </param>
        /// <returns> Вызывает событие PCsInfoReceived, в котором передает список ПК</returns>
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

        /// <summary>
        /// Запрос на подключение ПК
        /// </summary>
        /// <param name="token"> Токен администратора</param>
        /// <param name="id"> Id компьютера, который необходимо подключить</param>
        /// <returns> Вызывает событие PCConnected, в котором возвращается токен компьютера </returns>
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

        /// <summary>
        /// Обновление параметорв состояния ПК
        /// </summary>
        /// <param name="pcToken"> Токен компьютера </param>
        /// <returns> Вызывает событие Updated, в котором возвращаются новые значения состояния</returns>
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
