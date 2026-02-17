namespace HSMS.Security
{
    public interface IAppUserContext
    {
        #region Properties
        Guid UserId { get; }
        bool IsAuthenticated { get; }
        #endregion

        #region Methods
        bool HasPermission(AppPermission permission);
        //IAppUser GetUserById(Guid userId);
        //IAppUser GetUserByName(string userName);
        //void ChangePassword(IChangePassword detail);
        //void UpdateUserLoginActivity(Guid userId, string log);
        #endregion
    }
}
