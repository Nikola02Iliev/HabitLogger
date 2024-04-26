using System.Data.SQLite;
using System.Globalization;



namespace HabitLogger
{
    internal class Program
    {
        // Define the path to the documents folder
        private static string _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Define the path to the habit logger data folder within the documents folder
        private static string _dbFolderPath = Path.Combine(_documentsPath, @"HabitLogger\HabitLogger\database");

        // Define the path to the habit logger database file
        private static string _dbFilePath = Path.Combine(_dbFolderPath, "HabitLogger.db");

        // Define the connection string for the SQLite database
        private static string _connString = $@"Data Source={_dbFilePath};Version=3";

        static void Main(string[] args)
        {
            // Check if the data folder doesn't exist
            if (!Directory.Exists(_dbFolderPath))
            {
                // Create the data folder
                Directory.CreateDirectory(_dbFolderPath);
            }

            // Check if the database file doesn't exist
            if (!File.Exists(Path.Combine(_dbFolderPath, "HabitLogger.db")))
            {
                // Create a new SQLite database file
                SQLiteConnection.CreateFile(Path.Combine(_dbFolderPath, "HabitLogger.db"));

                using (var connection = new SQLiteConnection(_connString))
                {
                    connection.Open();

                    var createReadingBookTable = connection.CreateCommand();

                    createReadingBookTable.CommandText = @"
                        CREATE TABLE IF NOT EXISTS reading_book (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Date TEXT,
                            Quantity INTEGER);";

                    createReadingBookTable.ExecuteNonQuery();

                    connection.Close();
                }
            }

            // Call the GetUserInput method to display the menu and handle user interaction
            GetUserInput();

        }


        public static void GetUserInput()
        {

            bool closeApp = false;

            // Loop to display the menu and handle user input until the user exits
            while (!closeApp)
            {
                
                Console.WriteLine("----------------");
                Console.WriteLine("MENU");
                Console.WriteLine("----------------");
                Console.WriteLine();
                Console.WriteLine("Type 0 to Exit");
                Console.WriteLine("Type 1 to List all data");
                Console.WriteLine("Type 2 to Insert data");
                Console.WriteLine("Type 3 to Delete data");
                Console.WriteLine("Type 4 to Update data");

             
                string userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "0":
                        closeApp = true; 
                        break;
                    case "1":
                        ListAllData();
                        Console.WriteLine("Press Enter to continue...");
                        Console.ReadLine();
                        Console.Clear();
                        break;
                    case "2":
                        InsertData();
                        Console.Clear();
                        break;
                    case "3":
                        DeleteData();
                        Console.Clear();
                        break;
                    case "4":
                        UpdateData();
                        Console.Clear();
                        break;
                    default:
                        Console.WriteLine("Type numbers from 0 to 4");
                        Console.WriteLine("Press Enter to try again...");
                        Console.ReadLine();
                        Console.Clear();
                        break;

                }


                
            }

          
        }

        // This method prompts the user for a date in the format dd-MM-yy and validates the input.
        public static string GetDateInput(string message)
        {
            Console.WriteLine(message);

            string dateInput = Console.ReadLine();

            // If user enters 0, return to the main menu
            if (dateInput == "0")
            {
                GetUserInput();
            }

            // Loop until the user enters a valid date format
            while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("Invalid format for date! Try again...");
                dateInput = Console.ReadLine();
            }


            return dateInput;

        }

        // This method prompts the user for a number and validates the input.
        public static int GetNumInput(string message)
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();

            // If user enters 0, return to the main menu
            if (numberInput == "0")
            {
                GetUserInput();
            }

            // Loop until the user enters int
            while (!int.TryParse(numberInput, out _))
            {
                Console.WriteLine("Type int value! Try again...");
                numberInput = Console.ReadLine();
            }

            int parsedNumberInput = int.Parse(numberInput);

            return parsedNumberInput;


        }

        // This method handles inserting a new data entry into the habit logger database.
        public static void InsertData()
        {
            string date = GetDateInput("Type date excatly like this: dd-mm-yy or Type 0 to return to main menu...");

            int quantity = GetNumInput("Type quantity or Type 0 to return to main menu...");

            using (var connection = new SQLiteConnection(_connString))
            {
                connection.Open();

                var insertDataQuery = connection.CreateCommand();

                insertDataQuery.CommandText = $"INSERT INTO reading_book(Date, Quantity) VALUES('{date}', '{quantity}')";

                insertDataQuery.ExecuteNonQuery();

                connection.Close();
            }

            

        }

        // This method handles listing all data from the habit logger database.
        public static void ListAllData()
        {

            using (var connection = new SQLiteConnection(_connString))
            {
                connection.Open();

                List<ReadingBook> allData = new List<ReadingBook>();

                var listAllDataQuery = connection.CreateCommand();

                listAllDataQuery.CommandText = @"SELECT * FROM reading_book";

                SQLiteDataReader reader = listAllDataQuery.ExecuteReader();

                if (reader.HasRows)
                {
                    while(reader.Read())
                    {
                        allData.Add(
                            new ReadingBook
                            {
                                Id = reader.GetInt32(0),
                                Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                                Quantity = reader.GetInt32(2)
                            }
                            ); ;
                    }

                    foreach(var data in allData)
                    {
                        Console.WriteLine("-----------------");
                        Console.WriteLine($"Id - {data.Id}");
                        Console.WriteLine($"Date - {data.Date.ToString("dd-MM-yyyy")}");
                        Console.WriteLine($"Quantity - {data.Quantity}");
                        Console.WriteLine("-----------------");
                    }

                }
                else
                {
                    Console.WriteLine("No data...");
                }

                connection.Close();
            }



           

        }
        // This method handles deleting a data by Id from the habit logger database.
        public static void DeleteData()
        {
            ListAllData();

            int id = GetNumInput("Type Id value to delete data");

            using(var connection = new SQLiteConnection(_connString))
            {
                connection.Open();

                var deleteDataByIdQuery = connection.CreateCommand();

                deleteDataByIdQuery.CommandText = $"DELETE FROM reading_book WHERE Id = '{id}'";

                deleteDataByIdQuery.ExecuteNonQuery();

                connection.Close();
            }



        }

        // This method handles updating a data by Id from the habit logger database.
        public static void UpdateData()
        {
            ListAllData();

            int id = GetNumInput("Type Id value to update data");

            string updatedDate = GetDateInput("Type new date");

            int updatedQuantity = GetNumInput("Type new quantity");


            using(var connection = new SQLiteConnection(_connString))
            {
                connection.Open();

                var updateDataByIdQuery = connection.CreateCommand();

                updateDataByIdQuery.CommandText = $"UPDATE reading_book SET Date = '{updatedDate}', Quantity = '{updatedQuantity}' WHERE Id = '{id}'";

                updateDataByIdQuery.ExecuteNonQuery();

                connection.Close();
            }

        }


    }

    
}
