using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EmployeeManagementApi.Exceptions;
using EmployeeManagementApi.Models.EmployeeDto;
using EmployeeManagementApi.Repositories.MongoDb;

namespace EmployeeManagementApi.Business
{
    public class EmployeeBal: IEmployeeBal
    {
        private readonly IDocumentRepository<Employee> employeeRepository;
        private readonly IMapper mapper;

        private const string employeeIdKey = "employeeId";

        public EmployeeBal(IMongoDocumentRepositoryFactory factory, IMapper mapper)
        {
            employeeRepository = factory.GetDocumentRepository<Employee>();
            this.mapper = mapper;
        }

        public async Task<CreateEmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest createEmployeeRequest)
        {
            var employeeId = Guid.NewGuid();
            var dateTimeOffsetNow=DateTimeOffset.UtcNow;

            var employee = new Employee()
            {
                EmployeeId = employeeId,
                EmployeeName = createEmployeeRequest.EmployeeName,
                DepartmentName = createEmployeeRequest.DepartmentName,
                Role = createEmployeeRequest.Role,
                Created = dateTimeOffsetNow,
                LastModified = dateTimeOffsetNow
            };

            await employeeRepository.InsertOneDocumentAsync(employee);

            var createEmployeeResponse = mapper.Map<CreateEmployeeResponse>(employee);

            return createEmployeeResponse;
        }

        public async Task<GetEmployeeResponse> GetEmployeeByIdAsync(Guid employeeId)
        {
            var employee = await employeeRepository.FindDocumentByIdAsync(employeeIdKey, employeeId);

            if (employee == null)
            {
                return null;
            }

            var getEmployeeResponse = mapper.Map<GetEmployeeResponse>(employee);

            return getEmployeeResponse;
        }

        public async Task<List<GetEmployeeResponse>> GetAllEmployeesAsync()
        {
            var employees = new List<Employee>();

            employees = await employeeRepository.FindDocumentsAsync();

            if (employees.Count == 0)
            {
                return null;
            }

            var getEmployeeResponses = mapper.Map<List<GetEmployeeResponse>>(employees);

            return getEmployeeResponses;
        }

        public async Task<bool> ReplaceEmployeeByIdAsync(Guid employeeId, ReplaceEmployeeRequest replaceEmployeeRequest)
        {
            var employee = mapper.Map<Employee>(replaceEmployeeRequest);
            employee.EmployeeId = employeeId;

            var isReplaced = await employeeRepository.ReplaceDocumentByIdAsync(employeeIdKey, employeeId, employee);

            return isReplaced;
        }

        public async Task<bool> UpdateEmployeeByIdAsync(Guid employeeId, UpdateEmployeeRequest updateEmployeeRequest)
        {
            var employee = await employeeRepository.FindDocumentByIdAsync(employeeIdKey, employeeId);

            if (employee == null)
            {
                throw new EmployeeNotFoundException($"Given employee with employee Id {employeeId} not found.");
            }

            var isUpdated =
                await employeeRepository.UpdateDocumentByIdAsync(employeeIdKey, employeeId, updateEmployeeRequest);

            return isUpdated;
        }

        public async Task<bool> DeleteEmployeeByIdAsync(Guid employeeId)
        {
            var employee = await employeeRepository.FindDocumentByIdAsync(employeeIdKey, employeeId);

            if (employee == null)
            {
                throw new EmployeeNotFoundException($"Given employee with employee Id {employeeId} not found.");
            }

            var isDeleted = await employeeRepository.DeleteOneDocumentAsync(employeeIdKey, employeeId);

            return isDeleted;
        }
    }
}
