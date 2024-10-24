using System;
using System.Globalization;
using System.IO.Compression;
using Microsoft.Data.Sqlite;
namespace HabitLogger;

class Program
{

	static string connectionString = @"Data Source=HabitLogger.db";

	static void Main(string[] args)
	{
		using (var connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			var tableCmd = connection.CreateCommand();

			tableCmd.CommandText =
				@"CREATE TABLE IF NOT EXISTS drinking_water (
				Id INTEGER PRIMARY KEY AUTOINCREMENT,
				Date TEXT,
				Quantity INTEGER
				)";

			tableCmd.ExecuteNonQuery();

			connection.Close();
		}
		GetUserInput();
	}

	static void GetUserInput()
	{
		Console.Clear();
		bool closeApp = false;
		while (!closeApp)
		{
			Console.WriteLine("MAIN MENU");
			Console.WriteLine("\n\n1. View All Records\n2. Insert Record\n3. Delete Record\n4. Update Record\n\nQ. Close Application");
			Console.WriteLine("Enter Choice: ");
			string commandInput = Console.ReadLine();
			switch (commandInput)
			{
				case "Q":
				case "q":
					Console.WriteLine("\n\nGoodbye!");
					closeApp = true;
					break;
				case "1":
					GetAllRecords();
					break;
				case "2":
					Insert();
					break;
				case "3":
					Delete();
					break;
				case "4":
					Update();
					break;
				default:
					Console.WriteLine("\nInvalid Command.");
					break;
			}
		}
	}

	private static void Insert()
	{
		string date = GetDateInput();

		int quantity = GetNumberInput("\n\nPlease insert number of glasses: ");

		using (var connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			var tableCmd = connection.CreateCommand();
			tableCmd.CommandText =
				$"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity})";

			tableCmd.ExecuteNonQuery();
			connection.Close();
		}

	}

	internal static string GetDateInput()
	{
		Console.WriteLine("Insert the date (dd-mm-yy). Type Q to return to main menu: ");
		string dateInput = Console.ReadLine();

		if (dateInput == "Q" || dateInput == "q") GetUserInput();
		
		while (!DateTime.TryParseExact(dateInput, "dd-mm-yy", new CultureInfo("en-us"), DateTimeStyles.None, out  _))
		{
			Console.WriteLine("\n\nInvalid date (dd-mm-yy).");
			dateInput = Console.ReadLine();
		}

		return dateInput;
	}

	internal static int GetNumberInput(string message)
	{
		Console.WriteLine(message);

		string numberInput = Console.ReadLine();

		if (numberInput == "0") GetUserInput();

		int finalInput = Convert.ToInt32(numberInput);

		return finalInput;
	}

	private static void GetAllRecords()
	{
		Console.Clear();
		using (var connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			var tableCmd = connection.CreateCommand();
			tableCmd.CommandText =
				$"SELECT * FROM drinking_water";

			List<DrinkingWater> tableData = new();

			SqliteDataReader reader = tableCmd.ExecuteReader();

			if (reader.HasRows)
			{
				while (reader.Read())
				{
					tableData.Add(
						new DrinkingWater
						{
							Id = reader.GetInt32(0),
							Date = DateTime.ParseExact(reader.GetString(1), "dd-mm-yy", new CultureInfo("en-US")),
							Quantity = reader.GetInt32(2)
						});
				}
			}
			else
			{
				Console.WriteLine("No rows found");
			}

			connection.Close();

			Console.WriteLine("------------------------\n");
			foreach (var dw in tableData)
			{
				Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-mm-yyyy")} - Quantity: {dw.Quantity}");
			}
			Console.WriteLine("-----------------------\n");
		}
	}

	private static void Delete()
	{
		Console.Clear();
		GetAllRecords();


		int recordID = GetNumberInput("\nEnter ID of record to be deleted. Q to go back: ");

		using (SqliteConnection connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			var tableCmd = connection.CreateCommand();

			tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = '{recordID}'";

			int rowCount = tableCmd.ExecuteNonQuery();

			if (rowCount == 0)
			{
				Console.WriteLine($"\n\nRecord with ID {recordID} doesn't exist. \n\n");
				Delete();
			}
		}
		Console.WriteLine($"\n\nRecord with ID {recordID} was deleted.\n\n");
		GetUserInput();
	}
	
	private static void Update()
	{
		Console.Clear();
		GetAllRecords();


		int recordId = GetNumberInput("\nEnter ID of record to be updated. Q to go back: ");

		using (SqliteConnection connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			
			var checkCmd = connection.CreateCommand();
			checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
			int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());
			
			if (checkQuery == 0)
			{
				Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist.\n\n");
				connection.Close();
				Update();
			}
			
			string date = GetDateInput();
			int quantity = GetNumberInput("\n\nNumber of glasses: ");
			
			var tableCmd = connection.CreateCommand();
			tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";

			tableCmd.ExecuteNonQuery();
			connection.Close();			
			
		}
		Console.WriteLine($"\n\nRecord with ID {recordId} was updated.\n\n");
		GetUserInput();
	}
}