namespace WebAPI.DataModels
{
    public class CreateRecruitmentRequestDTO
    {
        public string ContractType { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string Department { get; set; }
        public string Yoe { get; set; }
    }
}
