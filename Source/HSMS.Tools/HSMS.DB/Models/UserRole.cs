namespace HSMS.DB.Models
{
    public class UserRole : BaseModel
    {
        #region Properties
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        public DateTime AssignedAt { get; set; }
        public Guid? AssignedByUserId { get; set; }
        #endregion
    }
}
