using System;
using System.Collections.Generic;

namespace EmployeeManagementApi.Models.EmployeeDto
{
    public class Employee
    {
        public Guid EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public string DepartmentName { get; set; }

        public string Role { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    }
}
