using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Utils.Interfaces
{
    public interface ILog
    {
        void LogError(Exception ex);
        /// <summary>
        /// Writes an Error to the log.
        /// </summary>
        /// <param name="ex"></param>
        void LogInformation(string info);
    }
}
