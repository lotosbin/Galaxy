using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Caching;
using CommonClass.Common;
using System.Web;
/*************************************************
 *�����ߣ�������
 *��  �ڣ�2008-10-19
 *��  �ܣ����ݿ������
 ************************************************/
public class SqlDB
{
    /// <summary>
    /// �������ݿ����Ӷ���
    /// </summary>
    protected SqlConnection m_conn;

    /// <summary>
    /// �������ݿ��������
    /// </summary>
    protected SqlCommand m_comm;

    /// <summary>
    /// �����������
    /// </summary>
    protected SqlTransaction m_trans;

    /// <summary>
    /// ���ݿ������ַ���
    /// </summary>
    protected string m_strConn;

    /// <summary>
    /// �Ƿ�ִ������
    /// </summary>
    protected bool m_bolIsTransaction = false;

    public SqlDB()
    {
        //Cache c = HttpContext.Current.Cache;
        //string strtemp = c.Get("connstring") == null ? string.Empty : c.Get("connstring").ToString();
        //if (string.IsNullOrEmpty(strtemp))
        //{
        //    strtemp = DESEncrypt.Decrypt(PtContextInstance.ConnString, "wangshijie");
        //    c.Insert("connstring", strtemp);
        //}
        //m_strConn = DESEncrypt.Decrypt(PtContextInstance.ConnString, "wangshijie");
        m_strConn = PtContextInstance.ConnString;
        m_conn = new SqlConnection(m_strConn);
        m_comm = m_conn.CreateCommand();
    }
    #region IDataBase ��Ա

    #region ������

    /// <summary>
    /// ���ݿ������ַ���
    /// </summary>
    public string ConnectionString
    {
        get
        {
            Cache c = HttpContext.Current.Cache;
            string strtemp = c.Get("connstring") == null ? string.Empty : c.Get("connstring").ToString();
            if (string.IsNullOrEmpty(strtemp))
            {
                strtemp = DESEncrypt.Decrypt(ConfigurationManager.AppSettings["SqlConnString"].ToString(), "wangshijie");
                c.Insert("connstring", strtemp);
            }

            m_strConn = strtemp;
            return this.m_strConn;
        }
    }

    /// <summary>
    /// ��ȡ���ݿ������Ƿ��Ѵ�
    /// </summary>
    public bool IsOpen
    {
        get
        {
            return m_conn.State == ConnectionState.Open ? true : false;
        }
    }
    #endregion

    /// <summary>
    /// ����������
    /// </summary>
    public void ConnectionOpen()
    {
        //����Ƿ��Ѵ�����
        if (ConnectionState.Closed == m_conn.State)
        {
            this.m_conn.Open();
        }
    }

    /// <summary>
    /// �ر����ݿ�����
    /// </summary>
    public void ConnectionClose()
    {
        //���ݿ�����δ�ر�����¹ر�
        if (ConnectionState.Closed != m_conn.State && false == m_bolIsTransaction)
        {
            m_conn.Close();
        }
    }

    /// <summary>
    /// ִ���޷����������ݲ�ѯ(����Ӱ������)
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����(SQL���)</param>
    /// <returns>����ִ�н��Ӱ������</returns>
    public int ExecuteNonQuery(string p_strCommandText)
    {
        return ExecuteNonQuery(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// ����-�޷����������ݲ�ѯ(��Ӧ�洢���̵��������ҷ���Ӱ������)
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <param name="p_objType">��ѯ��������</param>
    /// <returns>����ִ�н��Ӱ������</returns>
    public int ExecuteNonQuery(string p_strCommandText, CommandType p_objType)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;

        try
        {
            ConnectionOpen();    //�����ݿ�
            return m_comm.ExecuteNonQuery();
        }
        finally
        {
            ConnectionClose();
        }
    }
    /// <summary>
    /// ����-�޷����������ݲ�ѯ(��Ӧ�洢���̵��������ҷ���Ӱ������)
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <param name="p_objType">��ѯ��������</param>
    /// <param name="IsCloseConnection">�Ƿ�ر���������,true: �ر�,false: ���ر�</param>
    /// <returns>����ִ�н��Ӱ������</returns>
    public int ExecuteNonQuery(string p_strCommandText, CommandType p_objType, bool IsCloseConnection)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;

        try
        {
            ConnectionOpen();    //�����ݿ�
            return m_comm.ExecuteNonQuery();
        }
        finally
        {
            if (IsCloseConnection == true)
            {
                ConnectionClose();
            }
        }
    }

    /// <summary>
    /// ���ص�һ�е�һ�н�������ݲ�ѯ
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <returns>���ز�ѯ���</returns>
    public object ExecuteScalar(string p_strCommandText)
    {
        return ExecuteScalar(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// ����-���ص�һ�е�һ�н�������ݲ�ѯ(��Ӧ�洢���̵�������)
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <param name="p_objType">��ѯ��������</param>
    /// <returns>���ز�ѯ���</returns>
    public object ExecuteScalar(string p_strCommandText, CommandType p_objType)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;

        ConnectionOpen();    //�����ݿ�

        try
        {
            return m_comm.ExecuteScalar();
        }
        finally
        {
            ConnectionClose();
        }
    }

    /// <summary>
    /// ������ǰֻ�����ݼ���ѯ
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <returns>����ִ�н��</returns>
    public IDataReader ExecuteReader(string p_strCommandText)
    {
        return ExecuteReader(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// ����-������ǰֻ�����ݼ���ѯ(��Ӧ�洢���̵�������)
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <param name="p_objType">��ѯ��������</param>
    /// <returns>����ִ�н��</returns>
    public IDataReader ExecuteReader(string p_strCommandText, CommandType p_objType)
    {
        return ExecuteReader(p_strCommandText, p_objType, CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// ����-������ǰֻ�����ݼ���ѯ(��Ӧ�洢���̵�������)
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <param name="p_objType">��ѯ��������</param>
    /// <param name="p_objBehaviour">�ṩ�Բ�ѯ����Ͳ�ѯ�����ݿ��Ӱ���˵��</param>
    /// <returns>����ִ�н��</returns>
    public IDataReader ExecuteReader(string p_strCommandText, CommandType p_objType, CommandBehavior p_objBehaviour)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;

        ConnectionOpen();    //�����ݿ�
        return m_comm.ExecuteReader(p_objBehaviour);
    }

    /// <summary>
    /// ��������������
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <returns>����ִ�н��</returns>
    public IDbDataAdapter ExecuteAdapter(string p_strCommandText)
    {
        return ExecuteAdapter(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// ����-��������������(��Ӧ�洢���̵�������)
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <param name="p_objType">��ѯ��������</param>
    /// <returns>����ִ�н��</returns>
    public IDbDataAdapter ExecuteAdapter(string p_strCommandText, CommandType p_objType)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;
        return new SqlDataAdapter(m_comm);
    }

    /// <summary>
    /// �������ݼ�
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <returns>����ִ�н��</returns>
    public DataSet ExecuteDataSet(string p_strCommandText)
    {
        return ExecuteDataSet(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// ����-�������ݼ�(��Ӧ�洢���̵�������)
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <param name="p_objType">��ѯ��������</param>
    /// <returns>����ִ�н��</returns>
    public DataSet ExecuteDataSet(string p_strCommandText, CommandType p_objType)
    {
        DataSet dstResult = new DataSet();
        IDbDataAdapter dadSqlAdapter = ExecuteAdapter(p_strCommandText, p_objType);
        dadSqlAdapter.Fill(dstResult);
        return dstResult;
    }

    /// <summary>
    /// �������ݱ�
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <returns>����ִ�н��</returns>
    public DataTable ExecuteDataTable(string p_strCommandText)
    {
        return ExecuteDataTable(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// ����-�������ݱ�
    /// </summary>
    /// <param name="p_strCommandText">��ѯ����</param>
    /// <param name="p_objType">��ѯ��������</param>
    /// <returns>����ִ�н��</returns>
    public DataTable ExecuteDataTable(string p_strCommandText, CommandType p_objType)
    {
        DataTable dtblResult = new DataTable();
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;
        try
        {
            ConnectionOpen();
            SqlDataAdapter dadFillTable = new SqlDataAdapter(m_comm);
            dadFillTable.Fill(dtblResult);
            return dtblResult;
        }
        finally
        {
            ConnectionClose();
        }
    }

    /// <summary>
    /// ��Ӳ���
    /// </summary>
    /// <param name="p_strParamName">��������</param>
    /// <param name="p_objParamType">��������</param>
    /// <param name="p_objDirection">��������</param>
    public void AddParameter(string p_strParamName, DbType p_objParamType, System.Data.ParameterDirection p_objDirection)
    {
        SqlParameter spParam = new SqlParameter();
        spParam.ParameterName = p_strParamName;
        spParam.DbType = p_objParamType;
        spParam.Direction = p_objDirection;
        m_comm.Parameters.Add(spParam);
    }
    /// <summary>
    /// ����-��Ӳ���
    /// </summary>
    /// <param name="p_strParamName">��������</param>
    /// <param name="p_strParamType">��������</param>
    /// <param name="p_objDirection">��������</param>
    /// <param name="p_objValue">����ֵ</param>
    public void AddParameter(string p_strParamName, DbType p_strParamType, System.Data.ParameterDirection p_objDirection, object p_objValue)
    {
        AddParameter(p_strParamName, p_strParamType, p_objDirection);
        ModifyParameter(p_strParamName, p_objValue);
    }
    /// <summary>
    /// ����г��ȵĲ���
    /// </summary>
    /// <param name="p_strParamName">��������</param>
    /// <param name="p_objParamType">��������</param>
    /// <param name="p_intParamSize">��������</param>
    /// <param name="p_objDirection">��������</param>
    public void AddParameter(string p_strParamName, DbType p_objParamType, int p_intParamSize, System.Data.ParameterDirection p_objDirection)
    {
        SqlParameter spParam = new SqlParameter();
        spParam.ParameterName = p_strParamName;
        spParam.DbType = p_objParamType;
        spParam.Size = p_intParamSize;
        spParam.Direction = p_objDirection;
        m_comm.Parameters.Add(spParam);
    }
    /// <summary>
    /// ����-����г��ȵĲ���
    /// </summary>
    /// <param name="p_strParamName">��������</param>
    /// <param name="p_strParamType">��������</param>
    /// <param name="p_intParamSize">��������</param>
    /// <param name="p_objDirection">��������</param>
    /// <param name="p_objValue">����ֵ</param>
    public void AddParameter(string p_strParamName, DbType p_strParamType, int p_intParamSize, System.Data.ParameterDirection p_objDirection, object p_objValue)
    {
        AddParameter(p_strParamName, p_strParamType, p_intParamSize, p_objDirection);
        ModifyParameter(p_strParamName, p_objValue);
    }

    /// <summary>
    /// ��ȡ����ֵ
    /// </summary>
    /// <param name="p_strParamName">��������</param>
    /// <returns>����ִ�н��״̬��</returns>
    public object GetParameter(string p_strParamName)
    {
        if (m_comm.Parameters.IndexOf(p_strParamName) != -1)
        {
            return m_comm.Parameters[p_strParamName].Value;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// �޸Ĳ���ֵ
    /// </summary>
    /// <param name="p_strParamName">��������</param>
    /// <param name="p_objValue">����ֵ</param>
    public void ModifyParameter(string p_strParamName, object p_objValue)
    {
        //�����������Ϊȫ��Ψһ��ʶ��������ֵΪ�ַ����ͣ����������ת��
        if (m_comm.Parameters[p_strParamName].SqlDbType == SqlDbType.UniqueIdentifier)
        {
            if (p_objValue.GetType() == typeof(System.SByte))
            {
                p_objValue = new System.Guid(p_objValue.ToString());
            }
        }
        //�жϸò����Ƿ����
        if (-1 != m_comm.Parameters.IndexOf(p_strParamName))
        {
            m_comm.Parameters[p_strParamName].Value = p_objValue;
        }
    }

    /// <summary>
    /// �Ƴ�����
    /// </summary>
    /// <param name="p_strParamName">��������</param>
    public void RemoveParameter(string p_strParamName)
    {
        //�жϸò����Ƿ����
        //			if (m_comm.Parameters.IndexOf(p_strParamName) !=0)
        //			{
        m_comm.Parameters.RemoveAt(p_strParamName);
        //			}
    }

    /// <summary>
    /// ����������
    /// </summary>
    /// <returns>����ִ�н��״̬��</returns>
    public void BeginTransaction()
    {
        ConnectionOpen();
        if (null == m_trans)
        {
            m_trans = m_conn.BeginTransaction(IsolationLevel.ReadCommitted);
        }
        m_comm.Transaction = m_trans;
        m_bolIsTransaction = true;
    }

    /// <summary>
    /// �ύ������
    /// </summary>
    /// <returns>����ִ�н��״̬��</returns>
    public void CommitTransaction()
    {
        try
        {
            m_trans.Commit();
        }
        finally
        {
            m_bolIsTransaction = false;
            ConnectionClose();
        }
    }

    /// <summary>
    /// �ع�������
    /// </summary>
    /// <returns>����ִ�н��״̬��</returns>
    public void RollbackTransaction()
    {
        try
        {
            m_trans.Rollback();
        }
        finally
        {
            m_bolIsTransaction = false;
            ConnectionClose();
        }
    }

    /// <summary>
    /// �������
    /// </summary>
    public void Clear()
    {
        m_comm.Parameters.Clear();
    }

    ~SqlDB()
    {
        if (m_comm != null)
            m_comm.Dispose();
        if (m_conn != null)
            m_conn.Dispose();
        if (m_trans != null)
            m_trans.Dispose();
    }
    #endregion

}

