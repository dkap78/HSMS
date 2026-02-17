using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HSMS.DB.Models
{
    public abstract class BaseModel
    {
        #region Properties
        [Key]
        public Guid Id { get; set; }

        [ScaffoldColumn(false)]
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }

        [ScaffoldColumn(false)]
        [JsonIgnore]
        public Nullable<Guid> CreatedBy { get; set; }

        [ScaffoldColumn(false)]
        [JsonIgnore]
        public Nullable<Guid> UpdatedAt { get; set; }

        [ScaffoldColumn(false)]
        public Nullable<DateTime> LastUpdationDateTime { get; set; }

        public bool IsActive { get; set; }
        #endregion
    }
}
