using WebAPI.DatabaseContexts;
using WebAPI.DataModels;
using WebAPI.EnumTypes;

namespace WebAPI
{
    public class SEPRepository
    {
        SEPDbContext context;

        public SEPRepository(SEPDbContext dbContext)
        { 
            context = dbContext;
        }

        public bool AddEventApplication(EventApplication eventApplication)
        {
            if (!context.EventApplications.Any(x => x.RecordNumber == eventApplication.RecordNumber)) 
            {
                context.EventApplications.Add(eventApplication);
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool AddTask(SEPTask task) 
        {
            context.Tasks.Add(task);
            context.SaveChanges();
            return true;
        }

        public bool AddRecruitmentRequest(RecruitmentRequest request)
        {
            context.RecruitmentRequests.Add(request);
            context.SaveChanges();
            return true;
        }

        public bool AddFinancialRequest(FinancialRequest request)
        {
            context.FinancialRequests.Add(request);
            context.SaveChanges();
            return true;
        }

        public bool UpdateEventApplication(EventApplication eventApplication) 
        {
            if (context.EventApplications.Any(x => x.Id == eventApplication.Id))
            { 
                var target = context.EventApplications.FirstOrDefault(x => x.Id == eventApplication.Id);
                target = eventApplication;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool UpdateTask(SEPTask task)
        {
            if (context.Tasks.Any(x => x.Id == task.Id))
            {
                var target = context.Tasks.FirstOrDefault(x => x.Id == task.Id);
                target = task;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool UpdateRecruitmentRequest(RecruitmentRequest request)
        {
            if (context.RecruitmentRequests.Any(x => x.Id == request.Id))
            {
                var target = context.RecruitmentRequests.FirstOrDefault(x => x.Id == request.Id);
                target = request;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool UpdateFinancialRequest(FinancialRequest request)
        {
            if (context.FinancialRequests.Any(x => x.Id == request.Id))
            {
                var target = context.FinancialRequests.FirstOrDefault(x => x.Id == request.Id);
                target = request;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public List<EventApplication> GetEventApplicationsForRole(UserRoles role, EventApplicationState status)
        {
            return context.EventApplications.Where(x => x.Assignee == role && x.Status == status).ToList();
        }

        public EventApplication? GetEventApplication(int id)
        {
            return context.EventApplications.DefaultIfEmpty(null).First(x => x.Id == id);
        }

        public List<SEPTask> GetTasksForName(string name)
        {
            return context.Tasks.Where(x => x.AssignTo == name).ToList();
        }

        public List<SEPTask> GetAllTasks()
        {
            return context.Tasks.ToList();
        }

        public SEPTask? GetTask(int id)
        {
            return context.Tasks.DefaultIfEmpty(null).First(x => x.Id == id);
        }

        public List<RecruitmentRequest> GetRecruitments() 
        {
            return context.RecruitmentRequests.ToList();
        }

        public RecruitmentRequest? GetRecruitment(int id)
        {
            return context.RecruitmentRequests.DefaultIfEmpty(null).First(x => x.Id == id);
        }

        public List<FinancialRequest> GetFinancialRequests()
        {
            return context.FinancialRequests.ToList();
        }

        public FinancialRequest? GetFinancialRequest(int id)
        {
            return context.FinancialRequests.DefaultIfEmpty(null).FirstOrDefault(x => x.Id == id);
        }
    }
}