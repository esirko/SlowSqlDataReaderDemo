using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SlowSqlDataReaderDemo
{
    class Program
    {
        static string connectionString = "Server=localhost;Integrated Security=True;Database=VssfSdkSample_Configuration";

        static async Task Main(string[] args)
        {
            PrintHelp();
            Console.Write("> ");
            string input = Console.ReadLine();
            while (input != "exit")
            {
                if (!string.IsNullOrWhiteSpace(input))
                {
                    string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    switch (parts[0])
                    {
                        case "help":
                            PrintHelp();
                            break;

                        case "write":
                            if (int.TryParse(parts[1], out int n))
                            {
                                Stopwatch stopwatch = Stopwatch.StartNew();
                                string longString = new string('x', n);
                                string command = $"update tbl_RegistryItems set RegValue='{longString}' where ParentPath = 'SqlDataReader'";
                                await ExecuteNonQueryAsync(command);
                                Console.WriteLine($"Wrote {n} characters in {stopwatch.ElapsedMilliseconds} ms");
                            }
                            break;

                        case "read":
                            {
                                Stopwatch stopwatch = Stopwatch.StartNew();
                                int charactersRead = 0;
                                string command = "select RegValue from tbl_RegistryItems where ParentPath = 'SqlDataReader'";
                                if (parts[1] == "sync")
                                {
                                    charactersRead = Read(command);
                                }
                                else if (parts[1] == "async")
                                {
                                    charactersRead = await ReadAsync(command, parts.Length > 2 && parts[2] == "sa");
                                }
                                Console.WriteLine($"Read {charactersRead} characters in {stopwatch.ElapsedMilliseconds} ms");
                            }
                            break;

                        default:
                            Console.WriteLine($"Unrecognized command: {input}");
                            break;
                    }
                }
                Console.Write("> ");
                input = Console.ReadLine();
            }
        }

        private static int Read(string s)
        {
            int numCharactersTransferred = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(s, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        numCharactersTransferred += reader[0].ToString().Length;
                    }
                }
            }
            return numCharactersTransferred;
        }

        private static async Task<int> ReadAsync(string s, bool sequentialAccess)
        {
            int numCharactersTransferred = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(s, connection);
                await connection.OpenAsync();
                using (SqlDataReader reader = await command.ExecuteReaderAsync(sequentialAccess ? CommandBehavior.SequentialAccess : CommandBehavior.Default))
                {
                    while (await reader.ReadAsync())
                    {
                        numCharactersTransferred += reader[0].ToString().Length;
                    }
                }
            }
            return numCharactersTransferred;
        }

        private static async Task ExecuteNonQueryAsync(string s)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(s, connection);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Slow SqlDataReader demo");
            Console.WriteLine("Commands:");
            Console.WriteLine("    exit");
            Console.WriteLine("    help");
            Console.WriteLine("    write <n>");
            Console.WriteLine("    read async");
            Console.WriteLine("    read sync");
            Console.WriteLine("    read async sa (for sequential access)");
        }
    }
}
