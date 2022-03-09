using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EmployeeManagementApi.Business;
using EmployeeManagementApi.Exceptions;
using EmployeeManagementApi.Models.EmployeeDto;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementApi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeBal employeeBal;

        public EmployeesController(IEmployeeBal employeeBal)
        {
            this.employeeBal = employeeBal;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateEmployeeResponse), 200)]
        public async Task<ActionResult<CreateEmployeeResponse>> CreateEmployee(CreateEmployeeRequest createEmployeeRequest)
        {
            return await employeeBal.CreateEmployeeAsync(createEmployeeRequest);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<GetEmployeeResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<GetEmployeeResponse>>> GetAllEmployees()
        {
            var response = await employeeBal.GetAllEmployeesAsync();

            if (response.Count == 0)
            {
                return NotFound();
            }

            return response;
        }

        [HttpGet("{employeeId}")]
        [ProducesResponseType(typeof(GetEmployeeResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GetEmployeeResponse>> GetEmployeeById([FromRoute] Guid employeeId)
        {
            var response = await employeeBal.GetEmployeeByIdAsync(employeeId);

            if (response == null)
            {
                return NotFound();
            }

            return response;
        }

        [HttpPut("replace/{employeeId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> ReplaceEmployeeById([FromRoute] Guid employeeId, [FromBody] ReplaceEmployeeRequest replaceEmployeeRequest)
        {
            try
            {
                if (employeeId != replaceEmployeeRequest.EmployeeId)
                {
                    return BadRequest();
                }

                var response = await employeeBal.ReplaceEmployeeByIdAsync(employeeId, replaceEmployeeRequest);

                return NoContent();
            }
            catch (Exception ex) when (ex is EmployeeNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("update/{employeeId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> UpdateEmployeeById([FromRoute] Guid employeeId, [FromBody] UpdateEmployeeRequest updateEmployeeRequest)
        {
            try
            {
                var response = await employeeBal.UpdateEmployeeByIdAsync(employeeId, updateEmployeeRequest);

                return NoContent();
            }
            catch (Exception ex) when (ex is EmployeeNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{employeeId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeleteEmployeeById([FromRoute] Guid employeeId)
        {
            try
            {
                var response = await employeeBal.DeleteEmployeeByIdAsync(employeeId);

                return Ok();
            }
            catch (Exception ex) when (ex is EmployeeNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
