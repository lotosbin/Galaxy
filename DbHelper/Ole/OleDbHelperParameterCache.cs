using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;

namespace Glavesoft.SmartData.DbHelper.Ole
{
    /// <summary>
    /// 参数缓冲类
    /// </summary>
    internal class OleDbHelperParameterCache
    {
        /// <summary>
        /// 参数缓冲
        /// </summary>
        private static readonly Hashtable ParamCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 初始化一个 <see cref="OleDbHelperParameterCache"/> class类型的实例
        /// </summary>
        private OleDbHelperParameterCache()
        {
        }


        /// <summary>
        /// 设定参数缓冲
        /// </summary>
        /// <param name="connectionString">数据连接</param>
        /// <param name="commandText">命令类型</param>
        /// <param name="commandParameters">参数</param>
        public static void CacheParameterSet(string connectionString, string commandText,
                                             params OleDbParameter[] commandParameters)
        {
            string text1 = connectionString + ":" + commandText;
            ParamCache[text1] = commandParameters;
        }

        /// <summary>
        /// 克隆参数
        /// </summary>
        /// <param name="originalParameters">参数列表</param>
        /// <returns></returns>
        private static OleDbParameter[] CloneParameters(OleDbParameter[] originalParameters)
        {
            var parameterArray1 = new OleDbParameter[originalParameters.Length];
            int num1 = 0;
            int num2 = originalParameters.Length;
            while (num1 < num2)
            {
                parameterArray1[num1] = (OleDbParameter) ((ICloneable) originalParameters[num1]).Clone();
                num1++;
            }
            return parameterArray1;
        }

        /// <summary>
        /// 从存储过程填充参数的命令
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="spName">Name of the sp.</param>
        /// <param name="includeReturnValueParameter">if set to <c>true</c> [include return value parameter].</param>
        /// <returns></returns>
        private static OleDbParameter[] DiscoverSpParameterSet(string connectionString, string spName,
                                                               bool includeReturnValueParameter)
        {
            OleDbParameter[] parameterArray2;
            using (var connection1 = new OleDbConnection(connectionString))
            {
                using (var command1 = new OleDbCommand(spName, connection1))
                {
                    connection1.Open();
                    command1.CommandType = CommandType.StoredProcedure;
                    OleDbCommandBuilder.DeriveParameters(command1);
                    if (!includeReturnValueParameter)
                    {
                        command1.Parameters.RemoveAt(0);
                    }
                    var parameterArray1 = new OleDbParameter[command1.Parameters.Count];
                    command1.Parameters.CopyTo(parameterArray1, 0);
                    parameterArray2 = parameterArray1;
                }
            }
            return parameterArray2;
        }


        /// <summary>
        /// 取得参数
        /// </summary>
        /// <param name="connectionString">数据连接</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>所有参数</returns>
        public static OleDbParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            string text1 = connectionString + ":" + commandText;
            var parameterArray1 = (OleDbParameter[]) ParamCache[text1];
            if (parameterArray1 == null)
            {
                return null;
            }
            return CloneParameters(parameterArray1);
        }


        /// <summary>
        /// 取得参数
        /// </summary>
        /// <param name="connectionString">数据连接</param>
        /// <param name="spName">存储过程</param>
        /// <returns>所有参数</returns>
        public static OleDbParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }


        /// <summary>
        /// 取得参数
        /// </summary>
        /// <param name="connectionString">数据连接</param>
        /// <param name="spName">存储过程</param>
        /// <param name="includeReturnValueParameter">是否包括返回参数</param>
        /// <returns>所有参数</returns>
        public static OleDbParameter[] GetSpParameterSet(string connectionString, string spName,
                                                         bool includeReturnValueParameter)
        {
            string text1 = connectionString + ":" + spName +
                           (includeReturnValueParameter ? ":include ReturnValue Parameter" : string.Empty);
            var parameterArray1 = (OleDbParameter[]) ParamCache[text1];
            if (parameterArray1 == null)
            {
                object obj1;
                ParamCache[text1] =
                    obj1 =
                    DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter);
                parameterArray1 = (OleDbParameter[]) obj1;
            }
            return CloneParameters(parameterArray1);
        }
    }
}