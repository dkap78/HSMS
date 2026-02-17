using System.Data;
using System.Security.Claims;

namespace HSMS.Utility
{
    public static class Utility
    {
        public static string GetUserId(List<Claim> claims)
        {
            string strUserId = "";

            foreach (Claim cl in claims)
            {
                string strClaimType = cl.Type.ToLower();
                if (strClaimType == "uid" || strClaimType == "userid")
                {
                    strUserId = cl.Value;
                    break;
                }
            }

            return strUserId;
        }

        public static bool HasColumn(this IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
