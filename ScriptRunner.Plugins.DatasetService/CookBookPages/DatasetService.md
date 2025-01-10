---
title: "DatasetService Plugin"
description: "Learn how to use the DatasetService Plugin to process datasets with advanced statistical operations, filtering, grouping, and aggregation."
author: "Peter van de Pas"
keywords: [Data, Processing]
table-use-row-colors: true
table-row-color: "D3D3D3"
toc: true
toc-title: Table of Content
toc-own-page: true
---

# DatasetService Plugin

## Purpose

The **DatasetService Plugin** provides robust functionality for processing datasets with schema-based configurations.
Its capabilities include grouping, filtering, aggregation, and statistical analysis, 
making it ideal for preparing data for visualization, analysis, or reporting.

---

## Features

- **Schema-based dataset control**:
  - Define dataset structure using JSON schemas.
- **Data operations**:
  - Grouping: Organize data by specified fields.
  - Filtering: Apply custom conditions to extract data.
  - Aggregation: Calculate metrics like **Sum**, **Average**, **Min**, and **Max**.
- **Advanced statistics**:
  - Compute **Standard Deviation**, **Median**, **Mode**, and **Percentiles**.
  - Correlation analysis between two numerical fields.
- **Data normalization**:
  - Scale numerical fields to a [0, 1] range.

---

## Setup

1. **Define your dataset**: Use a **DataTable** for structured data storage.
2. **Create a JSON schema**: Specify field attributes and controls:
   - **IsGroupable**: Enables grouping.
   - **IsAggregator**: Enables aggregation.
   - **Filterable**: Allows filtering.

Example schema:
```json
[
  {
    "Name": "Genre",
    "TypeName": "string",
    "ControlType": "groupable",
    "DataSetControls": { "IsGroupable": true }
  },
  {
    "Name": "Rating",
    "TypeName": "number",
    "ControlType": "aggregator",
    "DataSetControls": { "IsAggregator": true }
  },
  {
    "Name": "Year",
    "TypeName": "number",
    "ControlType": "filterable",
    "DataSetControls": { "Filterable": true }
  }
]
```

3. **Initialize the plugin**:
```csharp
var datasetService = new DatasetService();
datasetService.Setup(dataTable, jsonSchema);
```

---

## Usage Examples

### 1. Grouping Data
Group data by a specific field, such as `Genre`:
```csharp
var groupedData = datasetService.GroupBy("Genre");
```

### 2. Aggregating Data
Compute the average value of a field:
```csharp
var averageRatings = datasetService.Aggregate("Rating", "Average");
```

### 3. Filtering Data
Filter rows based on a condition, such as movies released after 2000:
```csharp
var recentMovies = datasetService.Filter("Year", year => Convert.ToInt32(year) > 2000);
```

### 4. Statistical Calculations
Perform advanced analysis:
- **Standard Deviation**:
  ```csharp
  var stdDev = datasetService.StandardDeviation("Rating");
  ```
- **Median**:
  ```csharp
  var median = datasetService.Median("Rating");
  ```
- **90th Percentile**:
  ```csharp
  var percentile = datasetService.Percentile("Rating", 90);
  ```
- **Correlation**:
  ```csharp
  var correlation = datasetService.Correlation("Rating", "Year");
  ```

### 5. Normalizing Data
Scale numerical values to a [0, 1] range:
```csharp
var normalizedData = datasetService.Normalize("Rating");
```

---

## Example Script

This script demonstrates the **DatasetService Plugin** with a movie dataset:

```csharp
/*
{
    "TaskCategory": "Plugins",
    "TaskName": "DatasetServiceDemo",
    "TaskDetail": "A demo script showcasing DatasetService with advanced statistical operations and a movie ratings dataset",
    "RequiredPlugins": ["DatasetService"]
}
*/

var db = new SqliteDatabase();
db.Setup("Data Source=:memory:");
db.OpenConnection();

// Create Movies table
db.ExecuteNonQuery(@"
CREATE TABLE Movies (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT,
    Genre TEXT,
    Rating REAL,
    Year INTEGER
);
");

// Insert data
db.ExecuteNonQuery(@"
INSERT INTO Movies (Title, Genre, Rating, Year) VALUES
('The Shawshank Redemption', 'Drama', 9.3, 1994),
('The Godfather', 'Crime', 9.2, 1972),
('The Dark Knight', 'Action', 9.0, 2008),
('Inception', 'Sci-Fi', 8.8, 2010);
");

var moviesTable = db.ExecuteQuery("SELECT * FROM Movies");

var jsonSchema = @"
[
    { ""Name"": ""Genre"", ""DataSetControls"": { ""IsGroupable"": true } },
    { ""Name"": ""Rating"", ""DataSetControls"": { ""IsAggregator"": true } }
]";

var datasetService = new DatasetService();
datasetService.Setup(moviesTable, jsonSchema);

// Group by Genre
var groupedData = datasetService.GroupBy("Genre");
DumpTable("Grouped by Genre", groupedData);

// Average Ratings
var avgRatings = datasetService.Aggregate("Rating", "Average");
DumpTable("Average Ratings", avgRatings);

// Filter Movies
var recentMovies = datasetService.Filter("Year", year => Convert.ToInt32(year) > 2000);
DumpTable("Recent Movies", recentMovies);

db.CloseConnection();
```

---

## Expected Output

1. **Grouped Data**: A table showing movies grouped by their genres and a count of movies in each genre.
2. **Average Ratings**: A table with average ratings per genre.
3. **Filtered Movies**: A table listing movies released after the year 2000.
4. **Advanced Statistics**:
    - **Standard Deviation**: A numerical value indicating the spread of ratings.
    - **Median Rating**: The median rating value.
    - **Mode(s) of Genres**: The most common genres.
    - **90th Percentile**: The rating below which 90% of movies fall.
    - **Correlation**: The correlation coefficient between **Rating** and **Year**.

---

## Tips & Notes

- **Schema Design**:
    - Ensure fields have the correct controls (**IsGroupable**, **IsAggregator**, etc.) for operations.
    - Invalid or incomplete schemas will result in exceptions.
- **Error Handling**:
    - Use **Try-Catch** for methods like **Filter**, which may throw errors if the schema is incorrect.
- **Performance**:
    - For large datasets, pre-filter the data in SQL queries before loading it into the **DatasetService**.
- **Extensibility**:
    - The plugin supports additional statistical methods if implemented in the **DatasetService** class.

