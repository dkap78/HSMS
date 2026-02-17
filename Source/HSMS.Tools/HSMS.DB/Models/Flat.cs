namespace HSMS.DB.Models
{
    public class Flat : BaseModel
    {
        #region Properties
        public string FlatNumber { get; set; } = null!;
        public string? Wing { get; set; }
        public int? Floor { get; set; }
        #endregion
    }
}
