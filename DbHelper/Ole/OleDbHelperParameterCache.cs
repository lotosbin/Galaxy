using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;

namespace Glavesoft.SmartData.DbHelper.Ole
{
    /// <summary>
    /// ����������
    /// </summary>
    internal class OleDbHelperParameterCache
    {
        /// <summary>
        /// ��������
        /// </summary>
        private static readonly Hashtable ParamCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// ��ʼ��һ�� <see cref="OleDbHelperParameterCache"/> class���͵�ʵ��
        /// </summary>
        private OleDbHelperParameterCache()
        {
        }


        /// <summary>
        /// �趨��������
        /// </summary>
        /// <param name="connectionString">��������</param>
        /// <param name="commandText">��������</param>
        /// <param name="commandParameters">����</param>
        public static void CacheParameterSet(string connectionString, string commandText,
                                             params OleDbParameter[] commandParameters)
        {
            string text1 = connectionString + ":" + commandText;
            ParamCache[text1] = commandParameters;
        }

        /// <summary>
        /// ��¡����
        /// </summary>
        /// <param name="originalParameters">�����б�</param>
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
        /// �Ӵ洢����������������
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
        /// ȡ�ò���
        /// </summary>
        /// <param name="connectionString">��������</param>
        /// <param name="commandText">�����ı�</param>
        /// <returns>���в���</returns>
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
        /// ȡ�ò���
        /// </summary>
        /// <param name="connectionString">��������</param>
        /// <param name="spName">�洢����</param>
        /// <returns>���в���</returns>
        public static OleDbParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }


        /// <summary>
        /// ȡ�ò���
        /// </summary>
        /// <param name="connectionString">��������</param>
        /// <param name="spName">�洢����</param>
        /// <param name="includeReturnValueParameter">�Ƿ�������ز���</param>
        /// <returns>���в���</returns>
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