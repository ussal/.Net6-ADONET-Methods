using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace Net6_ADONET_Methods.Controllers
{
	public class DataAccess
	{
		public SqlDataReader GetDataReader(string query, Dictionary<string, object>? parameters = null)
		{
			SqlConnection con = new SqlConnection("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;");
			using (SqlCommand cmd = new SqlCommand(query, con))
			{
				if (parameters != null)
				{
					foreach (var parameter in parameters)
					{
						cmd.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
					}
				}
				con.Open();
				var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection); //Reader kapandığında connectionu da kapatır.
				return reader;
			}
		}
		public string GetDataString(string query, Dictionary<string, object>? parameters = null)
		{
			using (var reader = GetDataReader(query, parameters))
			{
				string dataString = String.Empty;
				while (reader.Read())
				{
					dataString = reader.IsDBNull(0) ? "" : reader.GetString(0);
				}
				reader.Close();
				return dataString;
			}
		}

		public T GetDataObject<T>(string query, Dictionary<string, object>? parameters = null) where T : class, new()
		{
			using (var reader = GetDataReader(query, parameters))
			{
				var parser = reader.GetRowParser<T>(typeof(T)); //GetRowParser bir dapper methodudur paketi yüklemeniz gerek
				T myObject = new T();
				while (reader.Read())
				{
					myObject = parser(reader);
				}
				reader.Close();
				return myObject;
			}
		}
		public List<T> GetDataList<T>(string query, Dictionary<string, object>? parameters = null) where T : class, new()
		{
			using (var reader = GetDataReader(query, parameters))
			{
				var parser = reader.GetRowParser<T>(typeof(T)); //GetRowParser bir dapper methodudur paketi yüklemeniz gerek
				List<T> myList = new List<T>();
				while (reader.Read())
				{
					myList.Add(parser(reader));
				}
				reader.Close();
				return myList;
			}
		}
	}
}
