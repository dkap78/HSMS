using HSMS.Security;
using Npgsql;

namespace HSMS.DAL
{
    public class HSMSDbContext : BaseDbContext
    {
        #region Private Fields
        private UserDbSet _users;
        private FlatDbSet _flats;
        private NoticeDbSet _notices;
        private PaymentDbSet _payments;
        private AccountDbSet _accounts;
        private EventDbSet _events;
        private AuditDbSet _auditLogs;
        private RoleDbSet _roles;
        #endregion

        #region Properties
        //Fields for all DbSet objects
        public UserDbSet Users
        {
            get { return _users; }
        }
        public FlatDbSet Flats
        {
            get { return _flats; }
        }
        public NoticeDbSet Notices
        {
            get { return _notices; }
        }
        public PaymentDbSet Payments
        {
            get { return _payments; }
        }
        public AccountDbSet Accounts
        {
            get { return _accounts; }
        }
        public EventDbSet Events
        {
            get { return _events; }
        }
        public AuditDbSet AuditLogs 
        {
            get { return _auditLogs; }
        }
        public RoleDbSet Roles
        {
            get { return _roles; }
        }
        #endregion

        #region Constructor
        public HSMSDbContext(ConnectionSettings settings, IAppUserContext context) : base(settings, context)
        {
            Initialize();
        }
        public HSMSDbContext(NpgsqlConnection conn, NpgsqlTransaction? tran = null) : base(conn, tran)
        {
            Initialize();
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            _users = new UserDbSet(this);
            _flats = new FlatDbSet(this);
            _notices = new NoticeDbSet(this);
            _payments = new PaymentDbSet(this);
            _accounts = new AccountDbSet(this);
            _events = new EventDbSet(this);
        }
        #endregion

        #region Get Methods
        #endregion

        #region  Update Methods
        #endregion

        #region Override Methods
        public override void Dispose(bool blnDisposing)
        {
            base.Dispose(blnDisposing);

            //Dispose all DbSet objects
        }
        #endregion

        #region Factory Methods
        public static HSMSDbContext Create(ConnectionSettings settings, IAppUserContext context)
        {
            return new HSMSDbContext(settings, context);
        }
        #endregion
    }
}
