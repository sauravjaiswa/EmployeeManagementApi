using System;
using System.Text.Json.Serialization;

namespace EmployeeManagementApi.Models.EmployeeDto
{
    public class ReplaceEmployeeRequest
    {
        public string EmployeeName { get; set; }

        public string DepartmentName { get; set; }

        public string Role { get; set; }
    }
}
