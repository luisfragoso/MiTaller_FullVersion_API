namespace MiTaller.DTO.Workshop.Bill
{
    public class ExpenseCategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
        public int BillCount { get; set; }
    }

    public class PostExpenseCategoryDto
    {
        public Guid WorkshopId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PutExpenseCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
    }
}
