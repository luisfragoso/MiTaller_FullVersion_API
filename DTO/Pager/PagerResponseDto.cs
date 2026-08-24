namespace MiTaller.DTO.Pager
{
    public class PagerResponseDto<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<T>? Elements { get; set; }
    }
}
