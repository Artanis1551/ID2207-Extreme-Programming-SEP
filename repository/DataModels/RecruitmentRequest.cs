namespace WebAPI.DataModels
{
    public class RecruitmentRequest
    {
        public int Id { get; set; }
        public string ContractType { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string Department { get; set; }
        public string Yoe { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
    }
}
