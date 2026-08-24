namespace MiTaller.DTO.Address
{
    public class AccountAddressDto
    {
        public Guid AccountId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string TownName { get; set; } = string.Empty;
        public int SuburbId { get; set; }
        public string SuburbName { get; set; } = string.Empty;
        public string Zipcode { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;

    }
}
