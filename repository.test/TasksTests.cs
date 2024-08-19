using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using WebAPI.DataModels;
using WebAPI.Test.DataModels;

namespace WebAPI.Test
{
    public class TasksTests
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
        [Order(0)]
        [TestCase("Admin")]
        [TestCase("SeniorCustomerService")]
        public void PostTaskForbidden(string user)
        {
            SEPTaskCreateDTO sepTask = new()
            {
                AssignTo = "ProductionDepartment",
                Description = "Work",
                Priority = "Medium",
                Reference = "1245b",
            };

            HttpClient client = LogInAs(user);

            StringContent content = new(JsonConvert.SerializeObject(sepTask), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync($"{baseUrl}Tasks/PostTask", content).Result;
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.Forbidden);
        }

        [Test]
        [Order(1)]
        public void PostTaskValid()
        {
            SEPTaskCreateDTO sepTask = new()
            {
                AssignTo = "ProductionDepartment",
                Description = "Work",
                Priority = "Medium",
                Reference = "1245b",
            };

            HttpClient client = LogInAs("ProductionManager");

            StringContent content = new(JsonConvert.SerializeObject(sepTask), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync($"{baseUrl}Tasks/PostTask", content).Result;
            Assert.That(response.IsSuccessStatusCode);
        }

        [Test]
        [Order(2)]
        public void GetTasksInvalid()
        {
            HttpClient client = LogInAs("CustomerService");

            HttpResponseMessage response = client.GetAsync($"{baseUrl}Tasks/GetTasks").Result;
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.Forbidden);
        }

        [Test]
        [Order(3)]
        [TestCase("ProductionManager")]
        [TestCase("ProductionDepartment")]
        public void GetTasks_Valid(string user)
        {
            HttpClient client = LogInAs(user);

            HttpResponseMessage response = client.GetAsync($"{baseUrl}Tasks/GetTasks").Result;
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            SEPTask[] events = JsonConvert.DeserializeObject<SEPTask[]>(jsonContent);
            Assert.That(events.Length == 1);
        }

        [Test]
        [Order(4)]
        public void ApproveTasks_Invalid()
        {
            HttpClient client = LogInAs("ProductionManager");

            HttpResponseMessage response = client.GetAsync($"{baseUrl}Tasks/GetTasks").Result;
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            SEPTask[] events = JsonConvert.DeserializeObject<SEPTask[]>(jsonContent);

            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Tasks/CompletedTask", content).Result;
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [Test]
        [Order(5)]
        public void ApproveTasks_Valid()
        {
            HttpClient client = LogInAs("ProductionDepartment");

            HttpResponseMessage response = client.GetAsync($"{baseUrl}Tasks/GetTasks").Result;
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            SEPTask[] events = JsonConvert.DeserializeObject<SEPTask[]>(jsonContent);

            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Tasks/CompletedTask", content).Result;
            Assert.That(response.IsSuccessStatusCode);

            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Tasks/CompletedTask", content).Result;
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.BadRequest);

            response = client.GetAsync($"{baseUrl}Tasks/GetTasks").Result;
            jsonContent = response.Content.ReadAsStringAsync().Result;
            events = JsonConvert.DeserializeObject<SEPTask[]>(jsonContent);
            Assert.That(events.Length == 0);

            client = LogInAs("ProductionManager");
            response = client.GetAsync($"{baseUrl}Tasks/GetTasks").Result;
            jsonContent = response.Content.ReadAsStringAsync().Result;
            events = JsonConvert.DeserializeObject<SEPTask[]>(jsonContent);

            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Tasks/CompletedTask", content).Result;
            Assert.That(response.IsSuccessStatusCode);

            response = client.GetAsync($"{baseUrl}Tasks/GetTasks").Result;
            jsonContent = response.Content.ReadAsStringAsync().Result;
            events = JsonConvert.DeserializeObject<SEPTask[]>(jsonContent);
            Assert.That(events.Length == 1);
        }

        private HttpClient LogInAs(string user)
        {
            HttpClient client = new();
            StringContent content = new(JsonConvert.SerializeObject(new LoginModel() { Username = user, Password = user + "_1" }), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync($"{baseUrl}Authentication/login", content).Result;
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            LoginResponse tokenResponse = JsonConvert.DeserializeObject<LoginResponse>(jsonContent);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);
            return client;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            webAPI.Kill();
        }
    }
}