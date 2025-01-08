/*
{
    "TaskCategory": "Plugins",
    "TaskName": "DatasetServiceEnhancedDemo",
    "TaskDetail": "A demo script showcasing DatasetService with advanced statistical operations and a movie ratings dataset"
}
*/

// Initialize the database using SqliteDatabase
var db = new SqliteDatabase();
db.Setup("Data Source=:memory:");
db.OpenConnection();

// Create the Movies table
string createTableQuery = @"
CREATE TABLE IF NOT EXISTS Movies (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT,
    Genre TEXT,
    Rating REAL,
    Year INTEGER
);";
db.ExecuteNonQuery(createTableQuery);

// Insert mock data into the Movies table
string insertMockData = @"
INSERT INTO Movies (Title, Genre, Rating, Year) VALUES
('The Shawshank Redemption', 'Drama', 9.3, 1994),
('The Godfather', 'Crime', 9.2, 1972),
('The Dark Knight', 'Action', 9.0, 2008),
('12 Angry Men', 'Drama', 9.0, 1957),
('Schindler''s List', 'Biography', 8.9, 1993),
('Pulp Fiction', 'Crime', 8.9, 1994),
('The Lord of the Rings: The Return of the King', 'Fantasy', 8.9, 2003),
('Forrest Gump', 'Romance', 8.8, 1994),
('Inception', 'Sci-Fi', 8.8, 2010),
('Fight Club', 'Drama', 8.8, 1999),
('The Matrix', 'Sci-Fi', 8.7, 1999),
('Goodfellas', 'Crime', 8.7, 1990),
('The Silence of the Lambs', 'Thriller', 8.6, 1991),
('Se7en', 'Thriller', 8.6, 1995),
('Interstellar', 'Sci-Fi', 8.6, 2014);
";
db.ExecuteNonQuery(insertMockData);

// Fetch data into a DataTable
var moviesTable = db.ExecuteQuery("SELECT * FROM Movies");

// Define the JSON schema for the DatasetService
string jsonSchema = @"
[
    {
        ""Name"": ""Genre"",
        ""TypeName"": ""string"",
        ""ControlType"": ""groupable"",
        ""DataSetControls"": { ""IsGroupable"": true }
    },
    {
        ""Name"": ""Rating"",
        ""TypeName"": ""number"",
        ""ControlType"": ""aggregator"",
        ""DataSetControls"": { ""IsAggregator"": true }
    },
    {
        ""Name"": ""Year"",
        ""TypeName"": ""number"",
        ""ControlType"": ""filterable"",
        ""DataSetControls"": { ""Filterable"": true }
    }
]";


// Initialize DatasetService
var datasetService = new DatasetService();
datasetService.Setup(moviesTable, jsonSchema);

// Perform dataset operations

// 1. Group movies by genre and count how many movies per genre
var groupedByGenre = datasetService.GroupBy("Genre");
DumpTable("Movies Grouped by Genre", groupedByGenre);

// 2. Calculate the average rating for each genre
var averageRatingByGenre = datasetService.Aggregate("Rating", "Average");
DumpTable("Average Rating by Genre", averageRatingByGenre);

// 3. Filter movies released after the year 2000
var recentMovies = datasetService.Filter("Year", year => Convert.ToInt32(year) > 2000);
DumpTable("Movies Released After 2000", recentMovies);

// 4. Compute advanced statistics

// Calculate standard deviation of ratings
var stdDevRating = datasetService.StandardDeviation("Rating");
Dump($"Standard Deviation of Ratings: {stdDevRating:F2}");

// Calculate median of ratings
var medianRating = datasetService.Median("Rating");
Dump($"Median Rating: {medianRating:F1}");

// Calculate mode(s) of genres
var genreModes = datasetService.Mode("Genre");
Dump($"Mode(s) of Genre: {string.Join(", ", genreModes)}");

// Calculate the 90th percentile of ratings
var rating90thPercentile = datasetService.Percentile("Rating", 90);
Dump($"90th Percentile of Ratings: {rating90thPercentile:F1}");

// Calculate correlation between Rating and Year
var correlationRatingYear = datasetService.Correlation("Rating", "Year");
Dump($"Correlation between Rating and Year: {correlationRatingYear:F2}");

// Normalize ratings to a scale of 0–1
var normalizedRatings = datasetService.Normalize("Rating");
DumpTable("Normalized Ratings", normalizedRatings);

// Clean up
db.CloseConnection();

return "DatasetService demo completed";
