namespace MiTaller.DTO.Workshop.Income
{
    public class WorkshopIncomeDetailResponseDto
    {
        public int Id { get; set; }
        public WorkshopSimpleIncomeResponseDto? WorkshopIncomeResponseDto { get; set; }
        public float Amount { get; set; }

    }
}
