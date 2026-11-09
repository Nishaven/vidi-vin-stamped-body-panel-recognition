// ============================================================
// 文件：SQLiteHelper.cs
// 模块：VisionCore（SQLite 数据访问层）
// 说明：检测结果库（System.Data.SQLite）的通用数据访问封装：打开/自动创建库文件、
//       事务批量写入（ExecuteSqlTran/ExecuteNonQuery，VIN 读码结果落 ResultRecords 表）、
//       查询与增删改；内置按数据库文件粒度的静态读写锁（ClsLock），保证多线程/多实例
//       并发操作同一库文件时的线程安全。
// ============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Data.SQLite;
using System.IO;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// SQLite 数据访问助手类：负责检测结果库的连接管理、SQL 执行与事务写入。
    /// 系统中由各读码相机（VINCodeCamera1~8）持有实例，把每车次的识别结果批量写入
    /// ResultRecords 表；通过静态字典 RWL 为每个数据库文件维护一把读写锁（ClsLock），
    /// 避免多线程同时读写同一 SQLite 文件时触发 "database is locked" 错误。
    /// </summary>
    public class SQLiteHelper
    {
        #region 字段

        /// <summary>
        /// 事务的基类
        /// </summary>
        private DbTransaction DBtrans;
        /// <summary>
        /// 使用静态变量字典解决多线程实例本类，实现一个数据库对应一个clslock
        /// </summary>
        private static readonly Dictionary<string, ClsLock> RWL = new Dictionary<string, ClsLock>();
        /// <summary>
        /// 数据库地址
        /// </summary>
        private readonly string mdataFile;
        /// <summary>
        /// 数据库密码
        /// </summary>
        private readonly string mPassWord;
        // 当前数据库在静态锁字典 RWL 中的键（即数据库文件路径），用于取回本库对应的读写锁
        private readonly string LockName = null;
        /// <summary>
        /// 数据库连接定义
        /// </summary>
        private SQLiteConnection mConn;
        #endregion

        #region 构造函数

        /// <summary>
        /// 根据数据库地址初始化
        /// </summary>
        /// <param name="dataFile">数据库地址</param>
        public SQLiteHelper(string dataFile)
        {
            // 参数空校验：路径为 null 直接抛异常（下面再赋值一次为历史遗留写法，效果等价）
            this.mdataFile = dataFile ?? throw new ArgumentNullException("dataFile=null");
            //this.mdataFile = AppDomain.CurrentDomain.BaseDirectory + dataFile;
            this.mdataFile = dataFile;
            // 静态锁字典按“数据库文件路径”注册读写锁：同一库文件的多个实例共用一把 ClsLock
            if (!RWL.ContainsKey(dataFile))
            {
                LockName = dataFile;
                RWL.Add(dataFile, new ClsLock());
            }
        }

        /// <summary>
        /// 使用密码打开数据库
        /// </summary>
        /// <param name="dataFile">数据库地址</param>
        /// <param name="PassWord">数据库密码</param>
        public SQLiteHelper(string dataFile, string PassWord)
        {
            // 参数空校验：数据库路径与密码任一为 null 直接抛异常
            this.mdataFile = dataFile ?? throw new ArgumentNullException("dataFile is null");
            this.mPassWord = PassWord ?? throw new ArgumentNullException("PassWord is null");
            //this.mdataFile = AppDomain.CurrentDomain.BaseDirectory + dataFile;
            this.mdataFile = dataFile;
            if (!RWL.ContainsKey(dataFile))
            {
                LockName = dataFile;
                RWL.Add(dataFile, new ClsLock());
            }
        }

        #endregion

        #region 打开/关闭 数据库

        /// <summary>  
        /// 打开 SQLiteManager 使用的数据库连接  
        /// </summary>  
        public void Open()
        {
            // 未设置密码走无密码连接，否则带密码打开（加密库）
            if (string.IsNullOrWhiteSpace(mPassWord))
            {
                mConn = OpenConnection(this.mdataFile);
            }
            else
            {
                mConn = OpenConnection(this.mdataFile, mPassWord);
            }
            Console.WriteLine("打开数据库成功");
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (this.mConn != null)
            {
                try
                {
                    this.mConn.Close();
                    // 连接关闭后同步移除静态锁字典中本库的锁记录，避免字典常驻膨胀
                    if (RWL.ContainsKey(LockName))
                    {
                        RWL.Remove(LockName);
                    }
                }
                catch
                {
                    Console.WriteLine("关闭失败");
                }
            }
            Console.WriteLine("关闭数据库成功");
        }

        #endregion

        #region 事务

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTrain()
        {
            EnsureConnection();
            DBtrans = mConn.BeginTransaction();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void DBCommit()
        {
            try
            {
                DBtrans.Commit();
            }
            catch (Exception)
            {
                // 提交失败（连接异常/约束冲突等）时回滚整个事务，避免结果数据落库一半
                DBtrans.Rollback();
            }
        }

        #endregion

        #region 工具

        /// <summary>  
        /// 打开一个SQLite数据库文件，如果文件不存在，则创建（无密码）
        /// </summary>  
        /// <param name="dataFile"></param>  
        /// <returns>SQLiteConnection 类</returns>  
        private SQLiteConnection OpenConnection(string dataFile)
        {
            if (dataFile == null)
            {
                throw new ArgumentNullException("dataFiledataFile=null");
            }
            if (!File.Exists(dataFile))
            {
                SQLiteConnection.CreateFile(dataFile);
            }
            SQLiteConnection conn = new SQLiteConnection();
            SQLiteConnectionStringBuilder conStr = new SQLiteConnectionStringBuilder
            {
                DataSource = dataFile
            };
            conn.ConnectionString = conStr.ToString();
            conn.Open();
            return conn;
        }

        /// <summary>  
        /// 打开一个SQLite数据库文件，如果文件不存在，则创建（有密码）
        /// </summary>  
        /// <param name="dataFile"></param>  
        /// <param name="Password"></param>
        /// <returns>SQLiteConnection 类</returns>  
        private SQLiteConnection OpenConnection(string dataFile, string Password)
        {
            if (dataFile == null)
            {
                throw new ArgumentNullException("dataFile=null");
            }
            if (!File.Exists(Convert.ToString(dataFile)))
            {
                SQLiteConnection.CreateFile(dataFile);
            }
            try
            {
                SQLiteConnection conn = new SQLiteConnection();
                SQLiteConnectionStringBuilder conStr = new SQLiteConnectionStringBuilder
                {
                    DataSource = dataFile,
                    Password = Password
                };
                conn.ConnectionString = conStr.ToString();
                conn.Open();
                return conn;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>  
        /// 读取 或 设置 SQLiteManager 使用的数据库连接  
        /// </summary>  
        public SQLiteConnection Connection
        {
            get
            {
                return mConn;
            }
            private set
            {
                mConn = value ?? throw new ArgumentNullException();
            }
        }

        /// <summary>
        /// 确保数据库是连接状态
        /// </summary>
        /// <exception cref="Exception"></exception>
        protected void EnsureConnection()
        {
            if (this.mConn == null)
            {
                throw new Exception("SQLiteManager.Connection=null");
            }
            if (mConn.State != ConnectionState.Open)
            {
                mConn.Open();
            }
        }

        /// <summary>
        /// 获取数据库文件的路径
        /// </summary>
        /// <returns></returns>
        public string GetDataFile()
        {
            return this.mdataFile;
        }

        /// <summary>  
        /// 判断表 table 是否存在  
        /// </summary>  
        /// <param name="table"></param>  
        /// <returns>存在返回true，否则返回false</returns>  
        public bool TableExists(string table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            EnsureConnection();
            // 查询 SQLite 系统表 sqlite_master 统计同名表数量：1 表示表存在（表名用参数防注入）
            SQLiteDataReader reader = ExecuteReader("SELECT count(*) as c FROM sqlite_master WHERE type='table' AND name=@tableName ", new SQLiteParameter[] { new SQLiteParameter("tableName", table) });
            if (reader == null)
            {
                return false;
            }
            reader.Read();
            int c = reader.GetInt32(0);
            reader.Close();
            reader.Dispose();
            //return false;  
            return c == 1;
        }

        /// <summary>
        /// VACUUM 命令（通过复制主数据库中的内容到一个临时数据库文件，然后清空主数据库，并从副本中重新载入原始的数据库文件）
        /// </summary>
        /// <returns></returns>
        public bool Vacuum()
        {
            try
            {
                using (SQLiteCommand Command = new SQLiteCommand("VACUUM", Connection))
                {
                    Command.ExecuteNonQuery();
                }
                return true;
            }
            catch (System.Data.SQLite.SQLiteException)
            {
                return false;
            }
        }

        #endregion

        #region 执行SQL

        /// <summary>
        /// 执行SQL, 并返回 SQLiteDataReader 对象结果 
        /// </summary>  
        /// <param name="sql"></param>
        /// <param name="paramArr">null 表示无参数</param>
        /// <returns></returns>  
        public SQLiteDataReader ExecuteReader(string sql, SQLiteParameter[] paramArr)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql=null");
            }
            EnsureConnection();
            using (RWL[LockName].Read())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, Connection))
                {
                    if (paramArr != null)
                    {
                        cmd.Parameters.AddRange(paramArr);
                    }
                    try
                    {
                        SQLiteDataReader reader = cmd.ExecuteReader();
                        cmd.Parameters.Clear();
                        return reader;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询，并返回dataset对象
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="paramArr">参数数组</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string sql, SQLiteParameter[] paramArr)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql=null");
            }
            this.EnsureConnection();
            using (RWL[LockName].Read())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, this.Connection))
                {
                    if (paramArr != null)
                    {
                        cmd.Parameters.AddRange(paramArr);
                    }
                    try
                    {
                        SQLiteDataAdapter da = new SQLiteDataAdapter();
                        DataSet ds = new DataSet();
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                        cmd.Parameters.Clear();
                        da.Dispose();
                        return ds;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// 执行SQL查询，并返回dataset对象。
        /// </summary>
        /// <param name="strTable">映射源表的名称</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="paramArr">SQL参数数组</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string strTable, string sql, SQLiteParameter[] paramArr)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql=null");
            }
            this.EnsureConnection();
            using (RWL[LockName].Read())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, this.Connection))
                {
                    if (paramArr != null)
                    {
                        cmd.Parameters.AddRange(paramArr);
                    }
                    try
                    {
                        SQLiteDataAdapter da = new SQLiteDataAdapter();
                        DataSet ds = new DataSet();
                        da.SelectCommand = cmd;
                        da.Fill(ds, strTable);
                        cmd.Parameters.Clear();
                        da.Dispose();
                        return ds;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>  
        /// 执行SQL，返回受影响的行数，可用于执行表创建语句，paramArr == null 表示无参数
        /// </summary>  
        /// <param name="sql"></param>  
        /// <returns></returns>  
        public int ExecuteNonQuery(string sql, SQLiteParameter[] paramArr)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql=null");
            }
            this.EnsureConnection();
            using (RWL[LockName].Read())
            {
                try
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, Connection))
                    {
                        if (paramArr != null)
                        {
                            foreach (SQLiteParameter p in paramArr)
                            {
                                cmd.Parameters.Add(p);
                            }
                        }
                        int c = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return c;
                    }
                }
                catch (SQLiteException)
                {
                    return 0;
                }
            }
        }

        /// <summary>  
        /// 执行SQL，返回结果集第一行，如果结果集为空，那么返回空 List(List.Count=0)， 
        /// rowWrapper = null 时，使用 WrapRowToDictionary  
        /// </summary>  
        /// <param name="sql"></param>  
        /// <param name="paramArr"></param>  
        /// <returns></returns>  
        public object ExecuteScalar(string sql, SQLiteParameter[] paramArr)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql=null");
            }
            this.EnsureConnection();
            using (RWL[LockName].Read())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, Connection))
                {
                    if (paramArr != null)
                    {
                        cmd.Parameters.AddRange(paramArr);
                    }
                    try
                    {
                        object reader = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        cmd.Dispose();
                        return reader;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString, params SQLiteParameter[] cmdParms)
        {
            EnsureConnection();
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                try
                {
                    PrepareCommand(cmd, Connection, null, SQLString, cmdParms);
                    int rows = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return rows;
                }
                catch (SQLiteException E)
                {
                    throw new Exception(E.Message);
                }
            }
        }

        /// <summary>
        /// 执行前的命令准备：为 SQLiteCommand 绑定连接（必要时先打开）、事务、SQL 文本与参数。
        /// ExecuteSql 等方法在真正执行前都会经过此步骤。
        /// </summary>
        /// <param name="cmd">待准备的命令对象</param>
        /// <param name="conn">数据库连接</param>
        /// <param name="trans">所属事务，可为 null（无事务执行）</param>
        /// <param name="cmdText">SQL 语句文本</param>
        /// <param name="cmdParms">SQL 参数数组，可为 null</param>
        private void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, SQLiteTransaction trans, string cmdText, SQLiteParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (SQLiteParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        /// <summary>  
        /// 查询一行记录，无结果时返回 null，conditionCol = null 时将忽略条件，直接执行 select * from table   
        /// </summary>  
        /// <param name="table">表名</param>  
        /// <param name="conditionCol"></param>  
        /// <param name="conditionVal"></param>  
        /// <returns></returns>  
        public object QueryOne(string table, string conditionCol, object conditionVal)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            this.EnsureConnection();
            string sql = "select * from " + table;
            if (conditionCol != null)
            {
                sql += " where " + conditionCol + "=@" + conditionCol;
            }
            object result = ExecuteScalar(sql, new SQLiteParameter[] { new SQLiteParameter(conditionCol, conditionVal) });
            return result;
        }

        #endregion

        #region 增 删 改
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>        
        public void ExecuteSqlTran(ArrayList SQLStringList)
        {
            EnsureConnection();
            // 开启事务：本批 SQL 要么全部生效、要么全部回滚，保证一批检测结果落库的原子性
            using (SQLiteTransaction tran = mConn.BeginTransaction())
            {
                SQLiteCommand cmd = new SQLiteCommand
                {
                    Connection = mConn
                };
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    // 整批执行成功后统一提交
                    tran.Commit();
                }
                catch (SQLiteException E)
                {
                    // 任一条语句失败：回滚整批写入并抛出异常，由上层记录日志
                    tran.Rollback();
                    throw new Exception(E.Message);
                }
            }
        }

        /// <summary>
        /// 按事务批量执行 SQL 语句列表（读码结果批量落库的入口之一）：
        /// 全部语句执行成功才提交，任一语句失败即整体回滚，不向外抛异常。
        /// </summary>
        /// <param name="SQLList">待批量执行的 SQL 语句列表</param>
        public void ExecuteNonQuery(List<string> SQLList)
        {
            EnsureConnection();
            // 开启事务：整批 SQL 原子提交
            using (SQLiteTransaction tran = mConn.BeginTransaction())
            {
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = mConn
                };
                // 记录当前正在执行的 SQL，便于失败时定位问题语句
                string ErrorSQL = "";
                try
                {
                    foreach (string SQL in SQLList)
                    {
                        ErrorSQL = SQL;
                        command.CommandText = SQL;
                        command.ExecuteNonQuery();
                    }
                    tran.Commit();
                }
                catch (Exception)
                {
                    // 任一语句失败：回滚整批写入；下方拼接整批 SQL 的代码为预留排查信息，当前未输出
                    tran.Rollback();
                    string sql = string.Empty;
                    foreach (var temp in SQLList)
                    {
                        sql += temp;
                    }
                }
                command.Dispose();
            }
        }

        /// <summary>  
        /// 执行 insert into 语句 
        /// </summary>  
        /// <param name="table"></param>  
        /// <param name="entity"></param>  
        /// <returns></returns>  
        public int InsertData(string table, Dictionary<string, object> entity)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            this.EnsureConnection();
            string sql = BuildInsert(table, entity);
            return this.ExecuteNonQuery(sql, BuildParamArray(entity));
        }

        /// <summary>  
        /// 执行 update 语句，注意：如果 where = null，那么 whereParams 也为 null，
        /// </summary>  
        /// <param name="table">表名</param>  
        /// <param name="entity">要修改的列名和列名的值</param>  
        /// <param name="where">查找符合条件的列</param>  
        /// <param name="whereParams">where条件中参数的值</param>  
        /// <returns></returns>  
        public int Update(string table, Dictionary<string, object> entity, string where, SQLiteParameter[] whereParams)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            this.EnsureConnection();
            string sql = BuildUpdate(table, entity);
            SQLiteParameter[] parameter = BuildParamArray(entity);
            if (where != null)
            {
                sql += " where " + where;
                if (whereParams != null)
                {
                    SQLiteParameter[] newArr = new SQLiteParameter[(parameter.Length + whereParams.Length)];
                    Array.Copy(parameter, newArr, parameter.Length);
                    Array.Copy(whereParams, 0, newArr, parameter.Length, whereParams.Length);
                    parameter = newArr;
                }
            }
            return this.ExecuteNonQuery(sql, parameter);
        }

        /// <summary>
        /// 将内存 DataTable 中的行状态变更通过 DataAdapter 批量回写数据库。
        /// 处理流程：按 sqlCommand 构造查询适配器并 Fill 填充数据集，再调用 da.Update(table)
        /// 提交变更；CommandBuilder 自动生成增删改命令的代码已注释，执行异常被空 catch 静默吞掉。
        /// </summary>
        /// <param name="table">承载变更数据（行状态）的内存表</param>
        /// <param name="sqlCommand">用于构造适配器 SELECT 命令的查询语句</param>
        public void Update(DataTable table, string sqlCommand)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            this.EnsureConnection();
            using (RWL[LockName].Read())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlCommand, this.Connection))
                {
                    try
                    {
                        SQLiteDataAdapter da = new SQLiteDataAdapter();
                        DataSet ds = new DataSet();
                        da.SelectCommand = cmd;
                        // 先按查询语句填充数据集，为 Update 提供表结构映射信息
                        da.Fill(ds);
                        //SQLiteCommandBuilder builder = new SQLiteCommandBuilder(da);
                        //da.UpdateCommand = builder.GetUpdateCommand();
                        //da.InsertCommand = builder.GetInsertCommand();
                        //da.DeleteCommand = builder.GetDeleteCommand();
                        da.Update(table);
                        cmd.Parameters.Clear();
                        da.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /// <summary>  
        /// 执行 delete from table 语句，where不必包含'where'关键字，where = null 时将忽略 whereParams  
        /// </summary>  
        /// <param name="table"></param>  
        /// <param name="where"></param>  
        /// <param name="whereParams"></param>  
        /// <returns></returns>  
        public int Delete(string table, string where, SQLiteParameter[] whereParams)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            this.EnsureConnection();
            string sql = "delete from " + table + " ";
            if (where != null)
            {
                sql += "where " + where;
            }
            return ExecuteNonQuery(sql, whereParams);
        }

        /// <summary>
        /// 将 Dictionary 类型数据 转换为 SQLiteParameter[] 类型
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private SQLiteParameter[] BuildParamArray(Dictionary<string, object> entity)
        {
            List<SQLiteParameter> list = new List<SQLiteParameter>();
            foreach (string key in entity.Keys)
            {
                list.Add(new SQLiteParameter(key, entity[key]));
            }
            if (list.Count == 0)
            {
                return null;
            }
            return list.ToArray();
        }

        /// <summary>
        /// 将 Dictionary 类型数据 转换为 插入数据 的 SQL语句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="entity">字典</param>
        /// <returns></returns>
        private string BuildInsert(string table, Dictionary<string, object> entity)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("insert into ").Append(table);
            buf.Append(" (");
            foreach (string key in entity.Keys)
            {
                buf.Append(key).Append(",");
            }
            buf.Remove(buf.Length - 1, 1); // 移除最后一个,
            buf.Append(") ");
            buf.Append("values(");
            foreach (string key in entity.Keys)
            {
                buf.Append("@").Append(key).Append(","); // 创建一个参数
            }
            buf.Remove(buf.Length - 1, 1);
            buf.Append(") ");

            return buf.ToString();
        }

        /// <summary>
        /// 将 Dictionary 类型数据 转换为 修改数据 的 SQL语句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="entity">字典</param>
        /// <returns></returns>
        private string BuildUpdate(string table, Dictionary<string, object> entity)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("update ").Append(table).Append(" set ");
            foreach (string key in entity.Keys)
            {
                buf.Append(key).Append("=").Append("@").Append(key).Append(",");
            }
            buf.Remove(buf.Length - 1, 1);
            buf.Append(" ");
            return buf.ToString();
        }

        #endregion
    }
}
