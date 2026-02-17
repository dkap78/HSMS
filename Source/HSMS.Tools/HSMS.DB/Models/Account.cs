namespace HSMS.DB.Models
{
    public class Account : BaseModel
    {
        #region Properites
        public string Name { get; set; } = null!;
        public AccountType Type { get; set; }
        #endregion
    }
}
