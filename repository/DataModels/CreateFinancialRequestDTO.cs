namespace WebAPI.DataModels
{
    public class CreateFinancialRequestDTO
    {
        public string Department { get; set; }
        public string Reference { get; set; }
        public string Amount { get; set; }
        public string Reason { get; set; }
    }
}
