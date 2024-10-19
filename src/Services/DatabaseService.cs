using MySql.Data.MySqlClient;

namespace social_media_backend.Services;

public static class DatabaseService
{
	public static MySqlConnection Connection { get; set; }
	
	public static void Initialize(string connectionString)
	{
		Connection = new MySqlConnection(connectionString);
	}

	public static void OpenConnection()
	{
		if (Connection.State != System.Data.ConnectionState.Open)
		{
			Connection.Open();
		}
	}

	public static void CloseConnection()
	{
		if (Connection.State != System.Data.ConnectionState.Closed)
		{
			Connection.Close();
		}
	}
}