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
 *开发者：王佩佩
 *日  期：2008-10-19
 *功  能：数据库操作类
 ************************************************/
public class SqlDB
{
    /// <summary>
    /// 声明数据库连接对象
    /// </summary>
    protected SqlConnection m_conn;

    /// <summary>
    /// 声明数据库命令对象
    /// </summary>
    protected SqlCommand m_comm;

    /// <summary>
    /// 声明事务对象
    /// </summary>
    protected SqlTransaction m_trans;

    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    protected string m_strConn;

    /// <summary>
    /// 是否执行事务
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
    #region IDataBase 成员

    #region 类属性

    /// <summary>
    /// 数据库连接字符串
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
    /// 获取数据库连接是否已打开
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
    /// 打开数据连接
    /// </summary>
    public void ConnectionOpen()
    {
        //检测是否已打开连接
        if (ConnectionState.Closed == m_conn.State)
        {
            this.m_conn.Open();
        }
    }

    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public void ConnectionClose()
    {
        //数据库连接未关闭情况下关闭
        if (ConnectionState.Closed != m_conn.State && false == m_bolIsTransaction)
        {
            m_conn.Close();
        }
    }

    /// <summary>
    /// 执行无返回类型数据查询(返回影响行数)
    /// </summary>
    /// <param name="p_strCommandText">查询命令(SQL语句)</param>
    /// <returns>返回执行结果影响行数</returns>
    public int ExecuteNonQuery(string p_strCommandText)
    {
        return ExecuteNonQuery(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// 重载-无返回类型数据查询(适应存储过程调用需求且返回影响行数)
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <param name="p_objType">查询命令类型</param>
    /// <returns>返回执行结果影响行数</returns>
    public int ExecuteNonQuery(string p_strCommandText, CommandType p_objType)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;

        try
        {
            ConnectionOpen();    //打开数据库
            return m_comm.ExecuteNonQuery();
        }
        finally
        {
            ConnectionClose();
        }
    }
    /// <summary>
    /// 重载-无返回类型数据查询(适应存储过程调用需求且返回影响行数)
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <param name="p_objType">查询命令类型</param>
    /// <param name="IsCloseConnection">是否关闭数据连结,true: 关闭,false: 不关闭</param>
    /// <returns>返回执行结果影响行数</returns>
    public int ExecuteNonQuery(string p_strCommandText, CommandType p_objType, bool IsCloseConnection)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;

        try
        {
            ConnectionOpen();    //打开数据库
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
    /// 返回第一行第一列结果的数据查询
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <returns>返回查询结果</returns>
    public object ExecuteScalar(string p_strCommandText)
    {
        return ExecuteScalar(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// 重载-返回第一行第一列结果的数据查询(适应存储过程调用需求)
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <param name="p_objType">查询命令类型</param>
    /// <returns>返回查询结果</returns>
    public object ExecuteScalar(string p_strCommandText, CommandType p_objType)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;

        ConnectionOpen();    //打开数据库

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
    /// 返回向前只读数据集查询
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <returns>返回执行结果</returns>
    public IDataReader ExecuteReader(string p_strCommandText)
    {
        return ExecuteReader(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// 重载-返回向前只读数据集查询(适应存储过程调用需求)
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <param name="p_objType">查询命令类型</param>
    /// <returns>返回执行结果</returns>
    public IDataReader ExecuteReader(string p_strCommandText, CommandType p_objType)
    {
        return ExecuteReader(p_strCommandText, p_objType, CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// 重载-返回向前只读数据集查询(适应存储过程调用需求)
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <param name="p_objType">查询命令类型</param>
    /// <param name="p_objBehaviour">提供对查询结果和查询对数据库的影响的说明</param>
    /// <returns>返回执行结果</returns>
    public IDataReader ExecuteReader(string p_strCommandText, CommandType p_objType, CommandBehavior p_objBehaviour)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;

        ConnectionOpen();    //打开数据库
        return m_comm.ExecuteReader(p_objBehaviour);
    }

    /// <summary>
    /// 返回数据适配器
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <returns>返回执行结果</returns>
    public IDbDataAdapter ExecuteAdapter(string p_strCommandText)
    {
        return ExecuteAdapter(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// 重载-返回数据适配器(适应存储过程调用需求)
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <param name="p_objType">查询命令类型</param>
    /// <returns>返回执行结果</returns>
    public IDbDataAdapter ExecuteAdapter(string p_strCommandText, CommandType p_objType)
    {
        m_comm.CommandText = p_strCommandText;
        m_comm.CommandType = p_objType;
        return new SqlDataAdapter(m_comm);
    }

    /// <summary>
    /// 返回数据集
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <returns>返回执行结果</returns>
    public DataSet ExecuteDataSet(string p_strCommandText)
    {
        return ExecuteDataSet(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// 重载-返回数据集(适应存储过程调用需求)
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <param name="p_objType">查询命令类型</param>
    /// <returns>返回执行结果</returns>
    public DataSet ExecuteDataSet(string p_strCommandText, CommandType p_objType)
    {
        DataSet dstResult = new DataSet();
        IDbDataAdapter dadSqlAdapter = ExecuteAdapter(p_strCommandText, p_objType);
        dadSqlAdapter.Fill(dstResult);
        return dstResult;
    }

    /// <summary>
    /// 返回数据表
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <returns>返回执行结果</returns>
    public DataTable ExecuteDataTable(string p_strCommandText)
    {
        return ExecuteDataTable(p_strCommandText, CommandType.Text);
    }

    /// <summary>
    /// 重载-返回数据表
    /// </summary>
    /// <param name="p_strCommandText">查询命令</param>
    /// <param name="p_objType">查询命令类型</param>
    /// <returns>返回执行结果</returns>
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
    /// 添加参数
    /// </summary>
    /// <param name="p_strParamName">参数名称</param>
    /// <param name="p_objParamType">参数类型</param>
    /// <param name="p_objDirection">参数方向</param>
    public void AddParameter(string p_strParamName, DbType p_objParamType, System.Data.ParameterDirection p_objDirection)
    {
        SqlParameter spParam = new SqlParameter();
        spParam.ParameterName = p_strParamName;
        spParam.DbType = p_objParamType;
        spParam.Direction = p_objDirection;
        m_comm.Parameters.Add(spParam);
    }
    /// <summary>
    /// 重载-添加参数
    /// </summary>
    /// <param name="p_strParamName">参数名称</param>
    /// <param name="p_strParamType">参数类型</param>
    /// <param name="p_objDirection">参数方向</param>
    /// <param name="p_objValue">参数值</param>
    public void AddParameter(string p_strParamName, DbType p_strParamType, System.Data.ParameterDirection p_objDirection, object p_objValue)
    {
        AddParameter(p_strParamName, p_strParamType, p_objDirection);
        ModifyParameter(p_strParamName, p_objValue);
    }
    /// <summary>
    /// 添加有长度的参数
    /// </summary>
    /// <param name="p_strParamName">参数名称</param>
    /// <param name="p_objParamType">参数类型</param>
    /// <param name="p_intParamSize">参数长度</param>
    /// <param name="p_objDirection">参数方向</param>
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
    /// 重载-添加有长度的参数
    /// </summary>
    /// <param name="p_strParamName">参数名称</param>
    /// <param name="p_strParamType">参数类型</param>
    /// <param name="p_intParamSize">参数长度</param>
    /// <param name="p_objDirection">参数方向</param>
    /// <param name="p_objValue">参数值</param>
    public void AddParameter(string p_strParamName, DbType p_strParamType, int p_intParamSize, System.Data.ParameterDirection p_objDirection, object p_objValue)
    {
        AddParameter(p_strParamName, p_strParamType, p_intParamSize, p_objDirection);
        ModifyParameter(p_strParamName, p_objValue);
    }

    /// <summary>
    /// 提取参数值
    /// </summary>
    /// <param name="p_strParamName">参数名称</param>
    /// <returns>返回执行结果状态码</returns>
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
    /// 修改参数值
    /// </summary>
    /// <param name="p_strParamName">参数名称</param>
    /// <param name="p_objValue">参数值</param>
    public void ModifyParameter(string p_strParamName, object p_objValue)
    {
        //如果参数类型为全局唯一标识符，并且值为字符串型，则进行下列转换
        if (m_comm.Parameters[p_strParamName].SqlDbType == SqlDbType.UniqueIdentifier)
        {
            if (p_objValue.GetType() == typeof(System.SByte))
            {
                p_objValue = new System.Guid(p_objValue.ToString());
            }
        }
        //判断该参数是否存在
        if (-1 != m_comm.Parameters.IndexOf(p_strParamName))
        {
            m_comm.Parameters[p_strParamName].Value = p_objValue;
        }
    }

    /// <summary>
    /// 移除参数
    /// </summary>
    /// <param name="p_strParamName">参数名称</param>
    public void RemoveParameter(string p_strParamName)
    {
        //判断该参数是否存在
        //			if (m_comm.Parameters.IndexOf(p_strParamName) !=0)
        //			{
        m_comm.Parameters.RemoveAt(p_strParamName);
        //			}
    }

    /// <summary>
    /// 启动事务处理
    /// </summary>
    /// <returns>返回执行结果状态码</returns>
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
    /// 提交事务处理
    /// </summary>
    /// <returns>返回执行结果状态码</returns>
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
    /// 回滚事务处理
    /// </summary>
    /// <returns>返回执行结果状态码</returns>
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
    /// 清楚参数
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

