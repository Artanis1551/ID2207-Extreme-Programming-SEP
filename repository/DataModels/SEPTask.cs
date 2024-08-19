using System.Diagnostics.CodeAnalysis;

namespace WebAPI.DataModels
{
    public class SEPTask
    {
        public int Id { get; set; }
        public string AssignTo { get; set; }
        public string Reference { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        [AllowNull]
        public string Status { get; set; }
    }
}
