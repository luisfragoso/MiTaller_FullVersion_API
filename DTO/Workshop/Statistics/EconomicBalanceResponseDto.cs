namespace MiTaller.DTO.Workshop.Statistics
{
    public class EconomicBalanceResponseDto
    {
        public List<Income.WorkshopIncomeDetailResponseDto>? Incomes { get; set; }
        public List<Bill.WorkshopBillResponseDto>? Bills { get; set; }
        public List<Employee.WorkshopEmployeeSalaryDto>? Salaries { get; set; }
        public float SumIncomes { get; set; }
        public float SumBills { get; set; }
        public float SumSalaries { get; set; }
        public float Balance { get; set; }
    }
}
