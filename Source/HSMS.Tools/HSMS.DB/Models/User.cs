using HSMS.Security;

namespace HSMS.DB.Models
{
    public class User : BaseModel, IAppUser
    {
        #region Properties
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; } = null!;
        public string? Password { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        #endregion
    }
}
