using System;

namespace EmployeeManagementApi.Models.EmployeeDto
{
    public class UpdateEmployeeRequest
    {
        public string EmployeeName { get; set; }

        public string DepartmentName { get; set; }

        public string Role { get; set; }
    }
}
