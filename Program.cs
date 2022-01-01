using System.Collections.Generic;
using System.Data.SqlClient;

namespace hello_rds
{
    class Program
    {
        static void Main(string[] args)
        {
            var seatReserved = new Dictionary<string, bool>();

            if (args.Length < 1)
            {
                Console.WriteLine("Specify SQL Server password on the command line: dotnet run <password>");
                Environment.Exit(0);
            }
            var password = args[0];

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.DataSource = "[rds-sql-server-url].rds.amazonaws.com";
                builder.UserID = "Admin";
                builder.Password = password;
                builder.InitialCatalog = "cinema";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("Theater Seating Chart");
                    Console.WriteLine();

                    connection.Open();

                    // Mark ticketed seats as unavailable

                    string sqlTickets = "SELECT row, seat FROM ticket WHERE showing=101";

                    using (SqlCommand command = new SqlCommand(sqlTickets, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = Convert.ToString(reader["row"]);
                                var seat = Convert.ToInt32(reader["seat"]);
                                var seatName = $"{row}{seat}";
                                seatReserved[seatName] = true;
                            }
                        }
                    }

                    // Read rows and display seating chart

                    string sqlSeating = "SELECT row, seatcount FROM seating ORDER BY row";

                    using (SqlCommand command = new SqlCommand(sqlSeating, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = Convert.ToString(reader["row"]);
                                var seatcount = Convert.ToInt32(reader["seatcount"]);

                                for (int i = 10; i > 0; i--)
                                {
                                    Console.Write(i > seatcount ? "        " : "+-------");
                                }
                                Console.WriteLine("+");


                                for (int i = 10; i > 0; i--)
                                {
                                    var seatName = $"{row}{i}";
                                    if (i > seatcount)
                                    {
                                        Console.Write("        ");
                                    }
                                    else
                                    {
                                        Console.Write(seatReserved.ContainsKey(seatName) ? "|       " : $"|  {row}{i,-2}  ");
                                    }
                                }
                                Console.WriteLine("|");


                                for (int i = 10; i > 0; i--)
                                {
                                    Console.Write(i > seatcount ? "        " : "+-------");
                                }
                                Console.WriteLine("+");

                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}