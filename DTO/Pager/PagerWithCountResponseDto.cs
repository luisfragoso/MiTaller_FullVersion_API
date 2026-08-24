namespace MiTaller.DTO.Pager
{
    public class PagerWithCountResponseDto<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalElements { get; set; }
        public List<T>? Elements { get; set; }
    }
}
