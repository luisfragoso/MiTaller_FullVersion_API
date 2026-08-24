using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO;
using MiTaller.DTO.Services;
using MiTaller.DTO.Workshop;
using MiTaller.DTO.Workshop.Bill;
using MiTaller.DTO.Workshop.Employee;
using MiTaller.DTO.Workshop.Income;
using MiTaller.DTO.Workshop.Statistics;
using MiTaller.Models.Workshop;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopStatisticsController : ControllerBase
    {
        private readonly DataContext _context;

        public WorkshopStatisticsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("customers-by/{workshopId}")]
        public async Task<ActionResult<CountResponseDto>> GetCustomersByWorkshop(Guid workshopId)
        {
            try
            {
                var customerAppointmentIds = await _context.Appointments
                    .Where(a => a.WorkshopId == workshopId)
                    .Select(a => a.CustomerId)
                    .ToListAsync();
                var customerQuotationIds = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId)
                    .Select(q => q.CustomerId)
                    .ToListAsync();
                var customerVehicleInspectionIds = await _context.WorkshopVehicleInspections
                    .Where(v => v.WorkshopId == workshopId)
                    .Select(v => v.CustomerId)
                    .ToListAsync();
                var customerMotocycleInspectionIds = await _context.WorkshopMotocycleInspections
                    .Where(m => m.WorkshopId == workshopId)
                    .Select(m => m.CustomerId)
                    .ToListAsync();
                var allCustomerIds = customerAppointmentIds
                    .Concat(customerQuotationIds)
                    .Concat(customerVehicleInspectionIds)
                    .Concat(customerMotocycleInspectionIds)
                    .Distinct()
                    .ToList();

                var activeCustomerIds = await _context.Customers
                    .Where(c => allCustomerIds.Contains(c.Id) && c.IsDeleted != true)
                    .Select(c => c.Id)
                    .ToListAsync();

                var totalCount = activeCustomerIds.Count;
                var countDto = new CountResponseDto
                {
                    Count = totalCount
                };
                return Ok(countDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("new-monthly-customers-by/{workshopId}/{monthYear}")]
        public async Task<ActionResult<CountResponseDto>> GetNewMonthlyCustomersByWorkshop(Guid workshopId, string monthYear)
        {
            try
            {
                if (!DateTime.TryParseExact(monthYear, "MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    return BadRequest("invalid-date-format");
                }

                var startDate = new DateTime(parsedDate.Year, parsedDate.Month, 1);
                var endDate = startDate.AddMonths(1).AddTicks(-1); // último momento del último día del mes

                var vehicleInspections = await _context.WorkshopVehicleInspections
                    .Where(v => v.WorkshopId == workshopId &&
                                v.InspectionDate >= startDate &&
                                v.InspectionDate <= endDate)
                    .Select(v => v.CustomerId)
                    .Distinct()
                    .ToListAsync();

                var motocycleInspections = await _context.WorkshopMotocycleInspections
                    .Where(m => m.WorkshopId == workshopId &&
                                m.InspectionDate >= startDate &&
                                m.InspectionDate <= endDate)
                    .Select(m => m.CustomerId)
                    .Distinct()
                    .ToListAsync();

                var totalCustomerIds = vehicleInspections
                    .Concat(motocycleInspections)
                    .Distinct()
                    .ToList();

                var countDto = new CountResponseDto
                {
                    Count = totalCustomerIds.Count
                };

                return Ok(countDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("quotations-by/{workshopId}/{monthYear}")]
        public async Task<ActionResult<QuotationStatusSummaryDto>> GetQuotationsByWorkshop(Guid workshopId, string monthYear)
        {
            try
            {
                if (!DateTime.TryParseExact(monthYear, "MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    return BadRequest("invalid-date-format");
                }

                var startDate = new DateTime(parsedDate.Year, parsedDate.Month, 1);
                var endDate = startDate.AddMonths(1).AddTicks(-1);

                var quotations = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId &&
                                q.CreatedAt >= startDate &&
                                q.CreatedAt <= endDate)
                    .ToListAsync();

                var totalCount = quotations.Count;

                var groupedStatusCounts = new List<QuotationCountResponseDto>();

                var inProgressCount = quotations.Count(q => q.Status == "InProgress" || q.Status == "Quoted");
                groupedStatusCounts.Add(new QuotationCountResponseDto
                {
                    Status = "InProgress",
                    Count = inProgressCount,
                    Percentage = totalCount > 0 ? (float)Math.Round((inProgressCount * 100.0f) / totalCount, 2) : 0
                });

                var canceledCount = quotations.Count(q => q.Status == "Canceled" || q.Status == "Expired");
                groupedStatusCounts.Add(new QuotationCountResponseDto
                {
                    Status = "Canceled",
                    Count = canceledCount,
                    Percentage = totalCount > 0 ? (float)Math.Round((canceledCount * 100.0f) / totalCount, 2) : 0
                });

                var confirmedCount = quotations.Count(q => q.Status == "Confirmed");
                groupedStatusCounts.Add(new QuotationCountResponseDto
                {
                    Status = "Confirmed",
                    Count = confirmedCount,
                    Percentage = totalCount > 0 ? (float)Math.Round((confirmedCount * 100.0f) / totalCount, 2) : 0
                });

                var response = new QuotationStatusSummaryDto
                {
                    Total = totalCount,
                    Statuses = groupedStatusCounts
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("economic-balance-by/{workshopId}/{monthYear}")]
        public async Task<ActionResult<EconomicBalanceResponseDto>> GetEconomicBalanceByWorkshop(Guid workshopId, string monthYear)
        {
            try
            {
                if (!DateTime.TryParseExact(monthYear, "MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    return BadRequest("invalid-date-format");
                }

                var startDate = new DateTime(parsedDate.Year, parsedDate.Month, 1);
                var endDate = startDate.AddMonths(1).AddTicks(-1);

                float sumBills = 0;
                float sumIncomes = 0;
                float sumSalaries = 0;

                var incomes = await _context.WorkshopIncomes
                    .Where(i => i.WorkshopId == workshopId && i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                    .Include(b => b.WorkshopServices)
                    .ThenInclude(b => b.Service)
                    .ToListAsync();

                var bills = await _context.WorkshopBills
                    .Where(b => b.WorkshopId == workshopId)
                    .ToListAsync();

                var salaries = await _context.WorkshopEmployees
                    .Where(s => s.WorkshopId == workshopId && !s.IsDeleted)
                    .ToListAsync();

                var incomesDto = new List<WorkshopIncomeDetailResponseDto>();
                foreach (var income in incomes)
                {
                    var incomeDto = new WorkshopIncomeDetailResponseDto
                    {
                        Id = income.Id,
                        WorkshopIncomeResponseDto = new WorkshopSimpleIncomeResponseDto
                        {
                            WorkshopServiceId = income.WorkshopServices.Id,
                            Name = income.WorkshopServices.Service.Name,
                        },
                        Amount = income.Amount,
                    };
                    sumIncomes += incomeDto.Amount;
                    incomesDto.Add(incomeDto);
                }

                var billsDto = new List<WorkshopBillResponseDto>();
                foreach (var bill in bills)
                {
                    var billDto = new WorkshopBillResponseDto
                    {
                        Id = bill.Id,
                        Description = bill.Description,
                        Amount = bill.Amount,
                    };
                    sumBills += billDto.Amount;
                    billsDto.Add(billDto);
                }

                var salariesDto = new List<WorkshopEmployeeSalaryDto>();
                foreach (var salary in salaries)
                {
                    var salaryDto = new WorkshopEmployeeSalaryDto
                    {
                        Id = salary.Id,
                        FullName = salary.FullName,
                        Salary = salary.Salary,
                    };
                    sumSalaries += salary.Salary;
                    salariesDto.Add(salaryDto);
                }

                var economicBalanceResponseDto = new EconomicBalanceResponseDto
                {
                    Incomes = incomesDto,
                    SumIncomes = sumIncomes,
                    Bills = billsDto,
                    SumBills = sumBills,
                    Salaries = salariesDto,
                    SumSalaries = sumSalaries,
                    Balance = sumIncomes - sumBills - sumSalaries,
                };

                return Ok(economicBalanceResponseDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


    }
}
