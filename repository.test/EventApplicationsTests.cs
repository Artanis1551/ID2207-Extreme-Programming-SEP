using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using WebAPI.DataModels;
using WebAPI.Test.DataModels;

namespace WebAPI.Test
{
    public class EventApplicationsTests
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
        public void PostEventApplicationForbidden(string user)
        {
            EventApplication eventApplication = new()
            {
                ClientName = "test",
                Attendees = 50,
                Decorations = true,
                Food = false,
                PhotoVideo = true,
                EventType = "party",
                ExpectedBudget = 15000,
                Parties = true,
                From = DateTime.Now,
                To = DateTime.Now,
                Drinks = true,
                RecordNumber = "1252b",
            };

            HttpClient client = LogInAs(user);

            StringContent content = new(JsonConvert.SerializeObject(eventApplication), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync($"{baseUrl}Event/PostEventApplication", content).Result;
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.Forbidden);
        }

        [Test]
        [Order(1)]
        public void PostEventApplicationValid()
        {
            EventApplication eventApplication = new()
            {
                ClientName = "test",
                Attendees = 50,
                Decorations = true,
                Food = false,
                PhotoVideo = true,
                EventType = "party",
                ExpectedBudget = 15000,
                Parties = true,
                From = DateTime.Now,
                To = DateTime.Now,
                Drinks = true,
                RecordNumber = "1252b",
            };

            HttpClient client = LogInAs("CustomerService");

            StringContent content = new(JsonConvert.SerializeObject(eventApplication), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync($"{baseUrl}Event/PostEventApplication", content).Result;
            Assert.That(response.IsSuccessStatusCode);
        }

        [Test]
        [Order(2)]
        public void GetEventApplicationsInvalid()
        {
            HttpClient client = LogInAs("CustomerService");

            HttpResponseMessage response = client.GetAsync($"{baseUrl}Event/GetEventApplications").Result;
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            EventApplication[] events = JsonConvert.DeserializeObject<EventApplication[]>(jsonContent);
            Assert.That(events.Length == 0);
        }

        [Test]
        [Order(3)]
        public void GetEventApplicationsValid()
        {
            HttpClient client = LogInAs("SeniorCustomerService");

            HttpResponseMessage response = client.GetAsync($"{baseUrl}Event/GetEventApplications").Result;
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            EventApplication[] events = JsonConvert.DeserializeObject<EventApplication[]>(jsonContent);
            Assert.That(events.Length == 1);
        }

        [Test]
        [Order(4)]
        public void ApproveEventApplicationsInvalid()
        {
            HttpClient client = LogInAs("SeniorCustomerService");

            HttpResponseMessage response = client.GetAsync($"{baseUrl}Event/GetEventApplications").Result;
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            EventApplication[] events = JsonConvert.DeserializeObject<EventApplication[]>(jsonContent);

            client = LogInAs("AdministrationManager");
            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Event/ApproveEventApplication", content).Result;
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [Test]
        [Order(5)]
        public void ApproveEventApplicationsValid()
        {
            HttpClient client = LogInAs("SeniorCustomerService");

            HttpResponseMessage response = client.GetAsync($"{baseUrl}Event/GetEventApplications").Result;
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            EventApplication[] events = JsonConvert.DeserializeObject<EventApplication[]>(jsonContent);

            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Event/ApproveEventApplication", content).Result;
            Assert.That(response.IsSuccessStatusCode);

            response = client.GetAsync($"{baseUrl}Event/GetEventApplications").Result;
            jsonContent = response.Content.ReadAsStringAsync().Result;
            events = JsonConvert.DeserializeObject<EventApplication[]>(jsonContent);
            Assert.That(events.Length == 0);
        }

        [Test]
        [Order(6)]
        public void ApproveEventApplicationsValidChain()
        {
            HttpClient client = LogInAs("FinancialManager");

            HttpResponseMessage response = client.GetAsync($"{baseUrl}Event/GetEventApplications").Result;
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            EventApplication[] events = JsonConvert.DeserializeObject<EventApplication[]>(jsonContent);

            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Event/ApproveEventApplication", content).Result;
            Assert.That(response.IsSuccessStatusCode);

            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Event/ApproveEventApplication", content).Result;
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.BadRequest);

            client = LogInAs("AdministrationManager");

            response = client.GetAsync($"{baseUrl}Event/GetEventApplications").Result;
            jsonContent = response.Content.ReadAsStringAsync().Result;
            events = JsonConvert.DeserializeObject<EventApplication[]>(jsonContent);

            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Event/ApproveEventApplication", content).Result;
            Assert.That(response.IsSuccessStatusCode);

            jsonContent = JsonConvert.SerializeObject(new IDJSON { Id = events[0].Id });
            content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            response = client.PostAsync($"{baseUrl}Event/ApproveEventApplication", content).Result;
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
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