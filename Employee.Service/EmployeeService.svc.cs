﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Integration.Wcf;

using Employee.BusinessLogic.Abstractions;
using Employee.Mapping.Abstraction;
using Employee.Model;
using Employee.Service.Contracts.DataContracts;
using Employee.Service.Contracts.ServiceContracts;

namespace Employee.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EmployeeService : IEmployeeService
    {
        private IMapper<Model.Employee, EmployeeDto> employeeToDtoMapper;
        private readonly IMapper<EmployeeDto, Model.Employee> dtoToEmployeeMapper;

        public EmployeeService(
            IMapper<Model.Employee, EmployeeDto> employeeToDtoMapper, 
            IMapper<EmployeeDto, Model.Employee> dtoToEmployeeMapper)
        {
            this.employeeToDtoMapper = employeeToDtoMapper;
            this.dtoToEmployeeMapper = dtoToEmployeeMapper;
        }

        public void CreateEmployee(EmployeeDto employeeDto)
        {
            try
            {
                using (var httpRequestScope = AutofacHostFactory.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
                {
                    var employeeService = httpRequestScope.Resolve<IEmployeeManager>();

                    var employee = this.dtoToEmployeeMapper.Map(employeeDto);
                    employeeService.CreateEmployee(employee);
                }
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        [WebGet(UriTemplate = "/getallemployees", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public IEnumerable<EmployeeDto> GetAllEmployees()
        {
            try
            {
                using (var httpRequestScope = AutofacHostFactory.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
                {
                    var employeeService = httpRequestScope.Resolve<IEmployeeManager>();

                    var allEmployees = employeeService.GetAllEmployees();
                    return this.ConvertToDto(allEmployees);
                }
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }


        public void CreateDepartment(DepartmentDto departmentDto)
        {
            try
            {
                using (var httpRequestScope = AutofacHostFactory.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
                {
                    var employeeService = httpRequestScope.Resolve<IEmployeeManager>();

                    var department = this.ConvertToEntity(departmentDto);
                    employeeService.CreateDepartment(department);
                }
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        private Department ConvertToEntity(DepartmentDto departmentDto)
        {
            return new Department
                       {
                           Name = departmentDto.Name,
                           Leader = this.dtoToEmployeeMapper.Map(departmentDto.Leader),
                           Employees = departmentDto.Employees.Select(this.dtoToEmployeeMapper.Map).ToList()
                       };
        }

        public IEnumerable<DepartmentDto> GetAllDepartments()
        {
            try
            {
                using (var httpRequestScope = AutofacHostFactory.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
                {
                    var employeeService = httpRequestScope.Resolve<IEmployeeManager>();

                    var allDepartments = employeeService.GetAllDepartments();
                    return this.ConvertToDto(allDepartments);
                }
            }
            catch (Exception ex)
            {

                throw new FaultException(ex.Message);
            }
        }

        private IEnumerable<EmployeeDto> ConvertToDto(IEnumerable<Model.Employee> allEmployees)
        {
            return allEmployees.Select(this.employeeToDtoMapper.Map).ToList();
        }

        private IEnumerable<DepartmentDto> ConvertToDto(IEnumerable<Model.Department> allDepartments)
        {
            return allDepartments.Select(department =>
                new DepartmentDto
                {
                    Name = department.Name,
                    Leader = this.employeeToDtoMapper.Map(department.Leader),
                    Employees = department.Employees.Select(this.employeeToDtoMapper.Map).ToList()
                }).ToList();
        }
    }
}