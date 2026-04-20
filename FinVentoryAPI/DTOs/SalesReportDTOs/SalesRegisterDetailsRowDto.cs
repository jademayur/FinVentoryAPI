namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class SalesRegisterDetailsRowDto : SalesRegisterRowDto
    {

        public List<SalesRegisterItemLineDto> Items { get; set; } = new();
    }
}
