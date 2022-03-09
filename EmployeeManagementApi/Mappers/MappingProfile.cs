using System.Collections.Generic;
using AutoMapper;
using EmployeeManagementApi.Models.EmployeeDto;

namespace EmployeeManagementApi.Mappers
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<Employee, CreateEmployeeResponse>();

            CreateMap<Employee, GetEmployeeResponse>();

            CreateMap<ReplaceEmployeeRequest, Employee>();
        }
    }
}
