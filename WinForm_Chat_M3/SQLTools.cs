using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;


public class SQLTools
{
    private readonly string conString;

    public SQLTools()
    {
        var solutionRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

        var databasePath = Path.Combine(solutionRoot, "AI_Programming.mdf");

        conString = new SqlConnectionStringBuilder
        {
            DataSource = @"(LocalDB)\MSSQLLocalDB",
            AttachDBFilename = databasePath,
            IntegratedSecurity = true,
            ConnectTimeout = 30
        }.ConnectionString;
    }

    public string RetrieveTable(string SQLStr)
    // Gets A table from the data base acording to the SELECT Command in SQLStr;
    // Returns DataTable with the Table.
    {
        // connect to DataBase
        SqlConnection con = new SqlConnection(conString);

        // Build SQL Query
        SqlCommand cmd = new SqlCommand(SQLStr, con);

        // Build DataAdapter
        SqlDataAdapter ad = new SqlDataAdapter(cmd);

        // Build DataSet to store the data
        DataTable dt = new DataTable();

        // Get Data form DataBase into the DataSet
        ad.Fill(dt);

        return DataTableToJson(dt);
    }

    private string DataTableToJson(DataTable dt)
    {
        List<Dictionary<string, object?>> rows = new List<Dictionary<string, object?>>();

        foreach (DataRow dr in dt.Rows)
        {
            Dictionary<string, object?> row = new Dictionary<string, object?>();

            foreach (DataColumn col in dt.Columns)
            {
                row[col.ColumnName] = dr[col] == DBNull.Value ? null : dr[col];
            }

            rows.Add(row);
        }

        return JsonSerializer.Serialize(new
        {
            rowCount = rows.Count,
            rows
        });
    }

    public int ExecuteNonQuery(string SQL)
    {
        // התחברות למסד הנתונים
        SqlConnection con = new SqlConnection(conString);

        // בניית פקודת SQL
        SqlCommand cmd = new SqlCommand(SQL, con);

        // ביצוע השאילתא
        con.Open();
        int n = cmd.ExecuteNonQuery();
        con.Close();

        // return the number of rows affected
        return n;
    }

    public string GetSchema()
    {
        return """
            Database: AI_Programming

            Table: Students
            - Id : INT, PRIMARY KEY, IDENTITY
            - Firstname : NVARCHAR(50), NOT NULL
            - lastname : NVARCHAR(50), NOT NULL
            - Mobile : NVARCHAR(50), NOT NULL
            - Address : NVARCHAR(50), NULL
            - City : NVARCHAR(50), NULL

            Table: Grades
            - Id : INT, PRIMARY KEY, IDENTITY
            - StudentID : INT, NOT NULL, FOREIGN KEY -> Students(Id), ON DELETE CASCADE
            - Subject : NVARCHAR(50), NOT NULL
            - Grade : INT, NOT NULL
            - TestName : NVARCHAR(50), NOT NULL
            - Date : DATE, NOT NULL
            """;
    }
}
