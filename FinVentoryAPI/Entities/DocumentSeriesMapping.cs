// Entities/DocumentSeriesMapping.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class DocumentSeriesMapping : BaseEntity
    {
        [Key]
        public int MappingId { get; set; }

        public int CompanyId { get; set; }
        public int AccountId { get; set; }   // AR / AP / Cash / Bank / Journal account
        public int SeriesId { get; set; }

        [ForeignKey("AccountId")]
        public Account? Account { get; set; }

        [ForeignKey("SeriesId")]
        public DocumentSeries? Series { get; set; }
    }
}