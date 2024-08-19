namespace WebAPI.DataModels
{
    public class FinancialRequest
    {
        public int Id { get; set; }
        public string Department { get; set; }
        public string Reference { get; set; }
        public string Amount { get; set; }
        public string Reason { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
    }
}
