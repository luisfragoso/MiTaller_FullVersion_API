namespace MiTaller.DTO.Quotation
{
    public class QuotationResponseListDto
    {
        public int Count { get; set; }
        public List<QuotationResponseDto> Quotations { get; set; }
    }
}
