using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace NetCore.Utils.Interfaces
{
    public interface IDBHelper
    {
        string GetConnectionString();
        #region ExcuteDataReader
        void SetConnectionString(string connStr);
        /// <summary>
        /// Trả về datareader với trường hợp CommandType=CommandType.StoreProcedure
        /// </summary>
        /// <param name="connection">connection</param>
        /// <param name="commandText"></param>
        /// <param name="sqlparam">sqlparametrer</param>
        /// <returns></returns>
        IDataReader ExecuteReader(string commandText, SqlParameter[] sqlparam);

        IDataReader ExecuteReader(string commandText, SqlParameter[] sqlparam, out SqlCommand comx);

        /// <summary>
        /// Trả về datareader với trường hợp CommandType=CommandType.Textr
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commndText"></param>
        /// <returns></returns>
        IDataReader ExecuteReader(string commndText);

        #endregion ExcuteDataReader

        #region ExecuteScalar

        object ExecuteScalar(SqlCommand sqlCommand);

        object ExecuteScalar(SqlCommand sqlCommand, params SqlParameter[] Parameters);

        object ExecuteScalar(string strSQL);

        object ExecuteScalar(string strSQL, params SqlParameter[] Parameters);

        #endregion ExecuteScalar

        #region ExecuteNonQuery

        int ExecuteNonQuery(SqlCommand sqlCommand, int timeout);

        int ExecuteNonQuery(SqlCommand sqlCommand);

        int ExecuteNonQuery(SqlCommand sqlCommand, int timeout, params SqlParameter[] Parameters);

        int ExecuteNonQuery(SqlCommand sqlCommand, params SqlParameter[] Parameters);

        int ExecuteNonQuery(string strSQL);

        int ExecuteNonQuery(string strSQL, params SqlParameter[] Parameters);

        #endregion ExecuteNonQuery

        #region ExecuteScalarSP

        object ExecuteScalarSP(string SPName);

        object ExecuteScalarSP(string SPName, params SqlParameter[] Parameters);

        #endregion ExecuteScalarSP

        #region ExecuteNonQuerySP

        int ExecuteNonQuerySP(string SPName);

        int ExecuteNonQuerySP(string SPName, int timeout, params SqlParameter[] Parameters);

        int ExecuteNonQuerySP(string SPName, params SqlParameter[] Parameters);

        int ExecuteNonQuerySP(string connectionString, string SPName, params SqlParameter[] Parameters);
        int ExecuteNonQuery(string commandText, SqlParameter[] sqlparam, out SqlCommand comx);

        #endregion ExecuteNonQuerySP

        #region[ListGenerate]

        List<T> GetList<T>(SqlCommand sqlCommand);

        List<T> GetList<T>(SqlCommand sqlCommand, params SqlParameter[] Parameters);

        List<T> GetList<T>(string strSQL);

        List<T> GetList<T>(string strSQL, params SqlParameter[] Parameters);

        #endregion

        #region[ListGenerateSP]

        List<T> GetListSP<T>(string SPName);

        List<T> GetListSP<T>(string SPName, params SqlParameter[] Parameters);

        #endregion

        #region[GetInstance]

        T GetInstance<T>(SqlCommand sqlCommand);

        T GetInstance<T>(SqlCommand sqlCommand, params SqlParameter[] Parameters);

        T GetInstance<T>(string strSQL);

        T GetInstance<T>(string strSQL, params SqlParameter[] Parameters);

        #endregion

        #region[GetInstanceSP]

        T GetInstanceSP<T>(string SPName);

        T GetInstanceSP<T>(string SPName, params SqlParameter[] Parameters);

        #endregion

        void Close();
    }
}