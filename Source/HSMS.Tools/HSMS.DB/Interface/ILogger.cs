using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSMS.DB.Interface
{
    public interface ILogger
    {
        bool DebugEnabled { get; set; }
        bool InfoEnabled { get; set; }
        bool ErrorEnabled { get; set; }

        void Info(string strMessage);
        void Debug(string strMessage);
        void Error(string strMessage);
    }
}
