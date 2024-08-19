using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using WebAPI.DataModels;
using WebAPI.Test.DataModels;

namespace WebAPI.Test
{
    public class AuthenticationTests
    {
        private Process webAPI;
        private const string baseUrl = "http://localhost:5287/";

        [OneTimeSetUp]
        public void Setup()
        {
            var processInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                Arguments = "run --project ..\\..\\..\\..\\webapi",
                FileName = "dotnet",
                CreateNoWindow = false,
                
            };
            webAPI = Process.Start(processInfo);
            Task.Delay(7000).Wait();
        }

        [Test]
        public void LoginInvalidMessage()
        {
            HttpClient client = new();
            HttpResponseMessage response = new();
            StringContent content = new(JsonConvert.SerializeObject(new InvalidLoginModel() { Name = "Admin", Pass = "Admin_1" }), Encoding.UTF8, "application/json");
            Task.Run(async () => response = await client.PostAsync($"{baseUrl}Authentication/login", content)).Wait();
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [Test]
        [TestCase("Admin")]
        [TestCase("ProductionManager")]
        [TestCase("Accountant")]
        [TestCase("FinancialManager")]
        [TestCase("VicePresident")]
        public void LoginValid(string user)
        {
            HttpClient client = new();
            HttpResponseMessage response = new();
            StringContent content = new(JsonConvert.SerializeObject(new LoginModel() { Username = user, Password = user + "_1" }), Encoding.UTF8, "application/json");
            Task.Run(async () => response = await client.PostAsync($"{baseUrl}Authentication/login", content)).Wait();
            Assert.That(response.IsSuccessStatusCode);
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            LoginResponse tokenResponse = JsonConvert.DeserializeObject<LoginResponse>(jsonContent);
            Assert.That(tokenResponse.Roles.Length >= 1 && tokenResponse.Roles[0] == user);
        }

        [Test]
        [TestCase("Admin")]
        [TestCase("ProductionManager")]
        [TestCase("Accountant")]
        [TestCase("FinancialManager")]
        [TestCase("VicePresident")]
        [TestCase("CustomerService")]
        public void LoginInvalid(string user)
        {
            HttpClient client = new();
            HttpResponseMessage response = new();
            Random rnd = new();
            StringContent content = new(JsonConvert.SerializeObject(new LoginModel() { Username = user, Password = rnd.NextInt64().ToString("X") }), Encoding.UTF8, "application/json");
            Task.Run(async () => response = await client.PostAsync($"{baseUrl}Authentication/login", content)).Wait();
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            webAPI.Kill();
        }
    }
}