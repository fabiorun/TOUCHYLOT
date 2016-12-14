using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace TOUCH_BOX
{
    class DBHandler
    {
         public SqlConnection _conn;
         public DBHandler()
        {
            _conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TOUCH_BOX_CONN"].ToString());
        }

        public DataTable ExecuteShot(String _query)
        {
            try
            {
                DataSet _dS = new DataSet();
                SqlDataAdapter _adapter = new SqlDataAdapter();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandText = _query;
                _cmd.Connection = _conn;

                _conn.Open();
                _adapter.SelectCommand = _cmd;
                _adapter.Fill(_dS);
                return _dS.Tables[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _conn.Close();
            }

        }

        public object ExecuteScalarShot(string _query)
        {
            SqlCommand _cmd = new SqlCommand();
            _cmd.CommandText = _query;
            _cmd.Connection = _conn;
            try
            {
                _conn.Open();
                _cmd.CommandText = _query;
                return _cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _conn.Close();
            }
        }
        public int ExecuteNonQueryShot(string _query)
        {
            DataSet _dS = new DataSet();
            SqlCommand _cmd = new SqlCommand();
            _cmd.CommandText = _query;
            _cmd.Connection = _conn;
            try
            {
                _conn.Open();
                _cmd.CommandText = _query;
                return _cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _conn.Close();
            }
        }

    }
}
