# ScriptRunner.Plugins.DatasetService

![License](https://img.shields.io/badge/license-MIT-green)  
![Version](https://img.shields.io/badge/version-1.0.0-blue)

A versatile plugin for `ScriptRunner` designed to streamline dataset operations. With `DatasetService`, 
you can group, filter, normalize, and perform statistical analysis on datasets dynamically using schema definitions. 
Ideal for complex data processing tasks in your scripts.

---

## 🚀 Features

- **Dynamic Dataset Schema**: Define datasets with JSON schemas for robust validation and control.
- **Grouping**: Group data by fields and count occurrences with ease.
- **Aggregation**: Perform advanced operations such as `Sum`, `Average`, `Min`, and `Max`.
- **Filtering**: Dynamically filter datasets based on user-defined conditions.
- **Normalization**: Scale numerical data to a range of 0–1.
- **Statistical Analysis**: Compute `Standard Deviation`, `Median`, `Mode`, `Percentiles`, and `Correlation Coefficients`.
- **ScriptRunner Integration**: Seamlessly integrates with the ScriptRunner ecosystem for simplified scripting.

---

## 📦 Installation

### Plugin Activation
1. Place the `ScriptRunner.Plugins.DatasetService` assembly in the `Plugins` folder of your ScriptRunner project.
2. The plugin will be automatically discovered and activated.

---

## 📖 Usage

### Writing a Script

Here’s an example script demonstrating the powerful features of `DatasetService` using a movie ratings dataset:

```csharp
/*
{
    "TaskCategory": "Dynamic",
    "TaskName": "DatasetServiceEnhancedDemo",
    "TaskDetail": "A demo script showcasing DatasetService with advanced dataset operations"
}
*/

using System.Data;
using ScriptRunner.Plugins.DatasetService;

// JSON schema definition
string jsonSchema = @"
[
    { ""Name"": ""Genre"", ""DataSetControls"": { ""IsGroupable"": true } },
    { ""Name"": ""Rating"", ""DataSetControls"": { ""IsAggregator"": true } },
    { ""Name"": ""Year"", ""DataSetControls"": { ""Filterable"": true } }
]";

// Mock dataset
var moviesTable = new DataTable();
moviesTable.Columns.Add("Title", typeof(string));
moviesTable.Columns.Add("Genre", typeof(string));
moviesTable.Columns.Add("Rating", typeof(double));
moviesTable.Columns.Add("Year", typeof(int));

moviesTable.Rows.Add("The Shawshank Redemption", "Drama", 9.3, 1994);
moviesTable.Rows.Add("The Godfather", "Crime", 9.2, 1972);
moviesTable.Rows.Add("The Dark Knight", "Action", 9.0, 2008);

// Initialize DatasetService
var datasetService = PluginLoader.GetPlugin<ScriptRunner.Plugins.DatasetService.IDatasetService>();
datasetService.Setup(moviesTable, jsonSchema);

// Group by Genre
var groupedByGenre = datasetService.GroupBy("Genre");
DumpTable("Movies Grouped by Genre", groupedByGenre);

// Calculate average rating
var averageRating = datasetService.Aggregate("Rating", "Average");
DumpTable("Average Ratings", averageRating);

// Filter movies released after 2000
var recentMovies = datasetService.Filter("Year", year => Convert.ToInt32(year) > 2000);
DumpTable("Movies Released After 2000", recentMovies);

// Compute standard deviation of ratings
var stdDevRating = datasetService.StandardDeviation("Rating");
Dump($"Standard Deviation of Ratings: {stdDevRating:F2}");

// Normalize ratings
var normalizedRatings = datasetService.Normalize("Rating");
DumpTable("Normalized Ratings", normalizedRatings);

return "DatasetService demo completed";
```

---

## 🔧 Configuration

### Initialize the Plugin
Initialize the plugin with a `DataTable` and a JSON schema:
```csharp
datasetService.Setup(dataTable, jsonSchema);
```

### Operations
- **Grouping**: Group data by a specific field:
    ```csharp
    var groupedData = datasetService.GroupBy("Genre");
    ```
- **Aggregation**: Perform operations such as `Sum` or `Average`:
    ```csharp
    var aggregatedData = datasetService.Aggregate("Rating", "Average");
    ```
- **Filtering**: Filter rows based on custom conditions:
    ```csharp
    var filteredData = datasetService.Filter("Year", year => Convert.ToInt32(year) > 2000);
    ```
- **Normalization**: Normalize numerical fields:
    ```csharp
    var normalizedData = datasetService.Normalize("Rating");
    ```
- **Statistical Analysis**:
  - Standard Deviation:
      ```csharp
      var stdDev = datasetService.StandardDeviation("Rating");
      ```
  - Median:
      ```csharp
      var median = datasetService.Median("Rating");
      ```
  - Percentiles:
      ```csharp
      var percentile = datasetService.Percentile("Rating", 90);
      ```

---

## 🌟 Advanced Features

### JSON Schema
Control data processing with JSON schema definitions:
```json
[
    { "Name": "Genre", "DataSetControls": { "IsGroupable": true } },
    { "Name": "Rating", "DataSetControls": { "IsAggregator": true } },
    { "Name": "Year", "DataSetControls": { "Filterable": true } }
]
```

- **IsGroupable**: Specifies that the field supports grouping.
- **IsAggregator**: Enables aggregation operations.
- **Filterable**: Allows filtering of rows based on conditions.

---

## 🧪 Testing

- Use sample datasets to validate the plugin functionality.
- Test statistical operations with a variety of dataset scenarios.
- Ensure schemas match the dataset structure for consistent processing.

---

## 📄 Contributing

1. Fork this repository.
2. Create a feature branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m 'Add YourFeature'`).
4. Push the branch (`git push origin feature/YourFeature`).
5. Open a pull request.

---

## Author

Developed with `🧡 passion` by `Peter van de Pas`.

For any questions or feedback, feel free to open an issue or contact me directly!

---

## 🔗 Links

- [ScriptRunner Plugins Repository](https://github.com/petervdpas/ScriptRunner.Plugins)

---

## License

This project is licensed under the [MIT License](./LICENSE).
