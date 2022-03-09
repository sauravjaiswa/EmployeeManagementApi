using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeManagementApi.Models.EmployeeDto;

namespace EmployeeManagementApi.Business
{
    public interface IEmployeeBal
    {
        Task<CreateEmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest createEmployeeRequest);

        Task<GetEmployeeResponse> GetEmployeeByIdAsync(Guid employeeId);

        Task<List<GetEmployeeResponse>> GetAllEmployeesAsync();

        Task<bool> ReplaceEmployeeByIdAsync(Guid employeeId, ReplaceEmployeeRequest replaceEmployeeRequest);

        Task<bool> UpdateEmployeeByIdAsync(Guid employeeId, UpdateEmployeeRequest updateEmployeeRequest);

        Task<bool> DeleteEmployeeByIdAsync(Guid employeeId);
    }
}
