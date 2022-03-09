using System;

namespace EmployeeManagementApi.Models.EmployeeDto
{
    public class CreateEmployeeResponse
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string Role { get; set; }
    }
}
