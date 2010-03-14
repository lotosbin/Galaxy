using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Glavesoft.SmartData.DbHelper.Sql
{
    /// <summary>
    /// SQL命令辅助类
    /// </summary>
    public static class SqlDbHelper
    {
        #region Methods

        /// <summary>
        /// 分配参数值
        /// </summary>
        /// <param name="commandParameters">参数列表</param>
        /// <param name="parameterValues">值列表</param>
        /// <exception cref="ArgumentException">Parameter count does not match Parameter Value count.</exception>
        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters != null) && (parameterValues != null))
            {
                if (commandParameters.Length != parameterValues.Length)
                {
                    throw new ArgumentException("Parameter count does not match Parameter Value count.");
                }
                int num1 = 0;
                int num2 = commandParameters.Length;
                while (num1 < num2)
                {
                    commandParameters[num1].Value = parameterValues[num1];
                    num1++;
                }
            }
        }

        /// <summary>
        /// 附加参数到命令
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="commandParameters">参数列表</param>
        private static void AttachParameters(IDbCommand command, SqlParameter[] commandParameters)
        {
            foreach (SqlParameter parameter1 in commandParameters)
            {
                if ((parameter1.Direction == ParameterDirection.InputOutput) && (parameter1.Value == null))
                {
                    parameter1.Value = DBNull.Value;
                }
                if (parameter1.SqlDbType == SqlDbType.Text)
                {
                    parameter1.Size = -1;
                }
                command.Parameters.Add(parameter1);
            }
        }

        /// <summary>
        /// 构造时异常消息
        /// </summary>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="paras">参数列表</param>
        /// <returns></returns>
        private static string BuildExceptionMessage(string commandText, CommandType commandType, SqlParameter[] paras)
        {
            var builder1 = new StringBuilder();
            builder1.AppendFormat("COMMAND_TYPE: {0};", commandType);
            builder1.Append(Environment.NewLine);
            builder1.AppendFormat("COMMAND_TEXT: {0};", commandText);
            builder1.Append(Environment.NewLine);
            if (paras != null)
            {
                builder1.Append("PARAMETERS:");
                foreach (SqlParameter parameter1 in paras)
                {
                    builder1.AppendFormat("{0}='{1}',", parameter1.ParameterName, parameter1.Value);
                }
            }
            return builder1.ToString();
        }

        /// <summary>
        /// 准备命令
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="transaction">事务对象</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">命令参数</param>
        private static void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction,
                                           CommandType commandType, string commandText, SqlParameter[] commandParameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            command.CommandType = commandType;
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
        }


        /// <summary>
        /// 验证数据库出来的值,如果为<c>DBNull.Value</c>返回null，否则返回对象 
        /// </summary>
        /// <param name="val">要验证的值</param>
        /// <returns></returns>
        public static object CheckValue(object val)
        {
            return (DBNull.Value != val) ? val : null;
        }

        #endregion

        #region ExecuteDataset

        #region 自定义连接字符串

        /// <summary>
        /// 运行返回数据集
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>数据集</returns>
        /// <exception cref="ArgumentNullException"><c>connection</c> is null.</exception>
        public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (null == connection)
            {
                throw new ArgumentNullException("connection");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 = SqlDbHelperParameterCache.GetSpParameterSet(
                    connection.ConnectionString,
                    spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
        }

        /// <summary>
        /// 运行返回数据集
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>数据集</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connection, commandType, commandText, null);
        }

        /// <summary>
        /// 运行返回数据集
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>数据集</returns>
        /// <exception cref="SqlDbHelperException"><c>SqlHelperException</c>.</exception>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText,
                                             params SqlParameter[] commandParameters)
        {
            DataSet set2;
            try
            {
                var command1 = new SqlCommand();
                PrepareCommand(command1, connection, null, commandType, commandText, commandParameters);
                var adapter1 = new SqlDataAdapter(command1);
                var set1 = new DataSet();
                set1.Locale = CultureInfo.InvariantCulture;
                adapter1.Fill(set1);
                command1.Parameters.Clear();
                set2 = set1;
            }
            catch (Exception exception1)
            {
                throw new SqlDbHelperException(BuildExceptionMessage(commandText, commandType, commandParameters),
                                               exception1);
            }
            return set2;
        }

        #endregion

        #region 事务

        /// <summary>
        /// 运行返回数据集
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="spName">存储过程</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>数据集</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 =
                    SqlDbHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
        }

        /// <summary>
        /// 运行返回数据集
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>数据集</returns>
        public static DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(transaction, commandType, commandText, null);
        }

        /// <summary>
        /// 运行返回数据集
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>数据集</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText,
                                             params SqlParameter[] commandParameters)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            var command1 = new SqlCommand();
            PrepareCommand(command1, transaction.Connection, transaction, commandType, commandText,
                           commandParameters);
            var adapter1 = new SqlDataAdapter(command1);
            var set1 = new DataSet();
            adapter1.Fill(set1);
            command1.Parameters.Clear();
            return set1;
        }

        #endregion

        #endregion

        #region ExecuteNonQuery

        #region 自定义连接字符串

        /// <summary>
        /// 运行返回影响数据行
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>影响的数据行</returns>
        /// <exception cref="ArgumentNullException"><c>connection</c> is null.</exception>
        public static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (null == connection)
            {
                throw new ArgumentNullException("connection");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 = SqlDbHelperParameterCache.GetSpParameterSet(
                    connection.ConnectionString,
                    spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        }

        /// <summary>
        /// 运行返回影响数据行
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>影响的数据行</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connection, commandType, commandText, null);
        }


        /// <summary>
        /// 运行返回影响数据行
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>影响的数据行</returns>
        /// <exception cref="SqlException"><c>SqlException</c>.</exception>
        /// <exception cref="SqlDbHelperException"><c>SqlHelperException</c>.</exception>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText,
                                          params SqlParameter[] commandParameters)
        {
            int num2;
            try
            {
                using (var command1 = new SqlCommand())
                {
                    PrepareCommand(command1, connection, null, commandType, commandText, commandParameters);


                    int num1 = command1.ExecuteNonQuery();
                    command1.Parameters.Clear();
                    num2 = num1;
                }
            }
            catch (Exception exception1)
            {
                throw new SqlDbHelperException(BuildExceptionMessage(commandText, commandType, commandParameters),
                                               exception1);
            }
            return num2;
        }

        #endregion

        #region 事务

        /// <summary>
        /// 运行返回影响数据行
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>影响的数据行</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 =
                    SqlDbHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
        }

        /// <summary>
        /// 运行返回影响数据行
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>影响的数据行</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(transaction, commandType, commandText, null);
        }


        /// <summary>
        /// 运行返回影响数据行
        /// </summary>
        /// <param name="transaction">事务对象</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>影响的数据行</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText,
                                          params SqlParameter[] commandParameters)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            int num1;
            using (var command1 = new SqlCommand())
            {
                PrepareCommand(command1, transaction.Connection, transaction, commandType, commandText,
                               commandParameters);
                num1 = command1.ExecuteNonQuery();
                command1.Parameters.Clear();
            }
            return num1;
        }

        #endregion

        #endregion

        #region ExecuteReader

        #region 自定义连接字符串

        /// <summary>
        /// 运行返回数据行
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>数据行</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(connection, commandType, commandText, null);
        }


        /// <summary>
        /// 运行返回数据行
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="spName">存储过程</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>数据行</returns>
        /// <exception cref="ArgumentNullException"><c>connection</c> is null.</exception>
        public static SqlDataReader ExecuteReader(SqlConnection connection, string spName,
                                                  params object[] parameterValues)
        {
            if (null == connection)
            {
                throw new ArgumentNullException("connection");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 = SqlDbHelperParameterCache.GetSpParameterSet(
                    connection.ConnectionString,
                    spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteReader(connection, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteReader(connection, CommandType.StoredProcedure, spName);
        }

        /// <summary>
        /// 运行返回数据行
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>数据行</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText,
                                                  params SqlParameter[] commandParameters)
        {
            SqlConnectionOwnership connectionOwnership = SqlConnectionOwnership.External;
            SqlDataReader reader1;
            using (var command1 = new SqlCommand())
            {
                PrepareCommand(command1, connection, null, commandType, commandText, commandParameters);


                if (connectionOwnership == SqlConnectionOwnership.External)
                {
                    reader1 = command1.ExecuteReader();
                }
                else
                {
                    reader1 = command1.ExecuteReader(CommandBehavior.CloseConnection);
                }
                command1.Parameters.Clear();
            }
            return reader1;
        }

        #endregion

        #region 事务

        /// <summary>
        /// 运行返回数据行
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="spName">存储过程</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>数据行</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName,
                                                  params object[] parameterValues)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 =
                    SqlDbHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
        }


        /// <summary>
        /// 运行返回数据行
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>数据行</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType,
                                                  string commandText)
        {
            return ExecuteReader(transaction, commandType, commandText, null);
        }


        /// <summary>
        /// 运行返回数据行
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>数据行</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType,
                                                  string commandText, params SqlParameter[] commandParameters)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            SqlConnection connection = transaction.Connection;
            SqlConnectionOwnership connectionOwnership = SqlConnectionOwnership.External;
            SqlDataReader reader1;
            using (var command1 = new SqlCommand())
            {
                PrepareCommand(command1, connection, transaction, commandType, commandText, commandParameters);


                if (connectionOwnership == SqlConnectionOwnership.External)
                {
                    reader1 = command1.ExecuteReader();
                }
                else
                {
                    reader1 = command1.ExecuteReader(CommandBehavior.CloseConnection);
                }
                command1.Parameters.Clear();
            }
            return reader1;
        }

        #endregion

        //#region 默认连接字符串

        ///// <summary>
        ///// 运行返回数据行
        ///// </summary>
        ///// <param name="session">The session.</param>
        ///// <param name="spName">存储过程</param>
        ///// <param name="parameterValues">参数</param>
        ///// <returns>数据行</returns>
        //public static SqlDataReader ExecuteReader(ISmartDataSession session, string spName, params object[] parameterValues)
        //{
        //    if (null == session)
        //    {
        //        throw new ArgumentNullException("session");
        //    }
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        SqlParameter[] parameterArray1 = SqlDbHelperParameterCache.GetSpParameterSet(session.ConnectionString,
        //                                                                                   spName);
        //        AssignParameterValues(parameterArray1, parameterValues);
        //        return ExecuteReader(session, CommandType.StoredProcedure, spName, parameterArray1);
        //    }
        //    return ExecuteReader(session, CommandType.StoredProcedure, spName);
        //}

        ///// <summary>
        ///// 运行返回数据行
        ///// </summary>
        ///// <param name="session">The session.</param>
        ///// <param name="commandType">命令类型</param>
        ///// <param name="commandText">命令文本</param>
        ///// <returns>数据行</returns>
        //public static SqlDataReader ExecuteReader(ISmartDataSession session, CommandType commandType, string commandText)
        //{
        //    return ExecuteReader(session, commandType, commandText, null);
        //}


        ///// <summary>
        ///// 运行返回数据行
        ///// </summary>
        ///// <param name="session">The session.</param>
        ///// <param name="commandType">命令类型</param>
        ///// <param name="commandText">命令文本</param>
        ///// <param name="commandParameters">参数</param>
        ///// <returns>数据行</returns>
        //public static SqlDataReader ExecuteReader(ISmartDataSession session, CommandType commandType, string commandText,
        //                                          params SqlParameter[] commandParameters)
        //{
        //    SqlDataReader reader1;
        //    try
        //    {
        //        using (SqlConnection connection1 = new SqlConnection(session.ConnectionString))
        //        {
        //            connection1.Open();
        //            SqlConnectionOwnership connectionOwnership = SqlConnectionOwnership.Internal;
        //            SqlDataReader reader11;
        //            using (SqlCommand command1 = new SqlCommand())
        //            {
        //                PrepareCommand(command1, connection1, null, commandType, commandText, commandParameters);


        //                if (connectionOwnership == SqlConnectionOwnership.External)
        //                {
        //                    reader11 = command1.ExecuteReader();
        //                }
        //                else
        //                {
        //                    reader11 = command1.ExecuteReader(CommandBehavior.CloseConnection);
        //                }
        //                command1.Parameters.Clear();
        //                reader1 = reader11;
        //            }
        //        }
        //    }
        //    catch (Exception exception1)
        //    {
        //        throw new SqlDbHelperException(BuildExceptionMessage(commandText, commandType, commandParameters),
        //                                     exception1);
        //    }
        //    return reader1;
        //}

        //#endregion

        #endregion

        #region ExecuteScalar

        #region 自定义连接字符串

        /// <summary>
        /// 运行返回对象
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回对象</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connection, commandType, commandText, null);
        }


        /// <summary>
        /// 运行返回对象
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>返回对象</returns>
        /// <exception cref="ArgumentNullException"><c>connection</c> is null.</exception>
        public static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (null == connection)
            {
                throw new ArgumentNullException("connection");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 = SqlDbHelperParameterCache.GetSpParameterSet(
                    connection.ConnectionString,
                    spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
        }

        /// <summary>
        /// 运行返回对象
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>返回对象</returns>
        /// <exception cref="SqlDbHelperException"><c>SqlHelperException</c>.</exception>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText,
                                           params SqlParameter[] commandParameters)
        {
            object obj2;
            try
            {
                using (var command1 = new SqlCommand())
                {
                    PrepareCommand(command1, connection, null, commandType, commandText, commandParameters);


                    object obj1 = command1.ExecuteScalar();
                    command1.Parameters.Clear();
                    obj2 = obj1;
                }
            }
            catch (Exception exception1)
            {
                throw new SqlDbHelperException(BuildExceptionMessage(commandText, commandType, commandParameters),
                                               exception1);
            }
            return obj2;
        }

        #endregion

        #region 事务

        /// <summary>
        /// 运行返回对象
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回对象</returns>
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteScalar(transaction, commandType, commandText, null);
        }


        /// <summary>
        /// 运行返回对象
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>返回对象</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 =
                    SqlDbHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
        }


        /// <summary>
        /// 运行返回对象
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>返回对象</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText,
                                           params SqlParameter[] commandParameters)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            using (var command1 = new SqlCommand())
            {
                PrepareCommand(command1, transaction.Connection, transaction, commandType, commandText,
                               commandParameters);
                object obj1 = command1.ExecuteScalar();
                command1.Parameters.Clear();
                return obj1;
            }
        }

        #endregion

        #endregion

        #region ExecuteXmlReader

        #region 自定义连接字符串

        /// <summary>
        /// 运行返回 XmlReader
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>XmlReader</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(connection, commandType, commandText, null);
        }


        /// <summary>
        /// 运行返回 XmlReader
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>XmlReader</returns>
        /// <exception cref="ArgumentNullException"><c>connection</c> is null.</exception>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName,
                                                 params object[] parameterValues)
        {
            if (null == connection)
            {
                throw new ArgumentNullException("connection");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 = SqlDbHelperParameterCache.GetSpParameterSet(
                    connection.ConnectionString,
                    spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
        }


        /// <summary>
        /// 运行返回 XmlReader
        /// </summary>
        /// <param name="connection">数据连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>XmlReader</returns>
        /// <exception cref="SqlDbHelperException"><c>SqlHelperException</c>.</exception>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText,
                                                 params SqlParameter[] commandParameters)
        {
            XmlReader reader2;
            try
            {
                using (var command1 = new SqlCommand())
                {
                    PrepareCommand(command1, connection, null, commandType, commandText, commandParameters);

                    XmlReader reader1 = command1.ExecuteXmlReader();
                    command1.Parameters.Clear();
                    reader2 = reader1;
                }
            }
            catch (Exception exception1)
            {
                throw new SqlDbHelperException(BuildExceptionMessage(commandText, commandType, commandParameters),
                                               exception1);
            }
            return reader2;
        }

        #endregion

        #region 事务

        /// <summary>
        /// 运行返回 XmlReader
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>XmlReader</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(transaction, commandType, commandText, null);
        }


        /// <summary>
        /// 运行返回 XmlReader
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>XmlReader</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName,
                                                 params object[] parameterValues)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] parameterArray1 =
                    SqlDbHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(parameterArray1, parameterValues);
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, parameterArray1);
            }
            return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
        }


        /// <summary>
        /// 运行返回 XmlReader
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>XmlReader</returns>
        /// <exception cref="ArgumentNullException"><c>transaction</c> is null.</exception>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText,
                                                 params SqlParameter[] commandParameters)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            XmlReader reader1;
            using (var command1 = new SqlCommand())
            {
                PrepareCommand(command1, transaction.Connection, transaction, commandType, commandText,
                               commandParameters);
                reader1 = command1.ExecuteXmlReader();
                command1.Parameters.Clear();
            }
            return reader1;
        }

        #endregion

        #endregion

        #region Nested type: SqlConnectionOwnership

        /// <summary>
        /// SQL命令连接类型,包括内连接和外连接
        /// </summary>
        private enum SqlConnectionOwnership
        {
            /// <summary>
            /// 内连接
            /// </summary>
            Internal,
            /// <summary>
            /// 外连接
            /// </summary>
            External
        }

        #endregion
    }
}