using System;

namespace EmployeeManagementApi.Models.EmployeeDto
{
    public class GetEmployeeResponse
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string Role { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
    }
}
