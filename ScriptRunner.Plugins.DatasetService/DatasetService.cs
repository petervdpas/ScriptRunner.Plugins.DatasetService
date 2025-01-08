using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using ScriptRunner.Plugins.DatasetService.Interfaces;
using ScriptRunner.Plugins.Models;

namespace ScriptRunner.Plugins.DatasetService;

/// <summary>
///     Provides functionality to process datasets using a schema definition and perform operations
///     such as grouping, filtering, and aggregation.
/// </summary>
public class DatasetService : IDatasetService
{
    private DataTable? _dataTable;
    private List<DynamicPropertyMetadata>? _schema;

    /// <summary>
    ///     Configures the dataset service with a data table and JSON schema.
    /// </summary>
    /// <param name="dataTable">The data table containing the dataset.</param>
    /// <param name="jsonSchema">The JSON schema defining the structure of the dataset.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="dataTable" /> or <paramref name="jsonSchema" /> is
    ///     null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="jsonSchema" /> is invalid or cannot be parsed.</exception>
    public void Setup(DataTable dataTable, string jsonSchema)
    {
        _dataTable = dataTable ?? throw new ArgumentNullException(nameof(dataTable));
        _schema = DeserializeSchema(jsonSchema);
    }

    /// <summary>
    ///     Prepares a dataset for charts by grouping and aggregating data.
    /// </summary>
    /// <param name="groupByField">The field to group by.</param>
    /// <param name="aggregateField">The field to aggregate.</param>
    /// <param name="aggregateFunction">The aggregation function (e.g., "Average", "Sum").</param>
    /// <returns>
    ///     A tuple containing:
    ///     <c>Labels</c> - Group labels, and
    ///     <c>Values</c> - Aggregated values.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="groupByField" /> is not groupable or
    ///     <paramref name="aggregateField" /> is not aggregator.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if <paramref name="aggregateFunction" /> is unsupported.
    /// </exception>
    public (string[] Labels, double[] Values) PrepareChartDataset(string groupByField, string aggregateField,
        string aggregateFunction)
    {
        // Validate that fields exist in the schema
        var groupableField = _schema!.FirstOrDefault(p =>
            p.Name.Equals(groupByField, StringComparison.OrdinalIgnoreCase) &&
            p.DataSetControls?.ContainsKey("IsGroupable") == true);

        var aggregatorField = _schema!.FirstOrDefault(p =>
            p.Name.Equals(aggregateField, StringComparison.OrdinalIgnoreCase) &&
            p.DataSetControls?.ContainsKey("IsAggregator") == true);

        if (groupableField == null)
            throw new ArgumentException($"Field '{groupByField}' is not groupable.");
        if (aggregatorField == null)
            throw new ArgumentException($"Field '{aggregateField}' is not aggregator.");

        // Group data and calculate aggregates
        var groupedData = _dataTable!.AsEnumerable()
            .GroupBy(row => row[groupByField].ToString() ?? "")
            .Select(group =>
            {
                var key = group.Key;
                var result = aggregateFunction switch
                {
                    "Average" => group.Average(r => Convert.ToDouble(r[aggregateField])),
                    "Sum" => group.Sum(r => Convert.ToDouble(r[aggregateField])),
                    "Min" => group.Min(r => Convert.ToDouble(r[aggregateField])),
                    "Max" => group.Max(r => Convert.ToDouble(r[aggregateField])),
                    _ => throw new InvalidOperationException($"Unsupported aggregation function: {aggregateFunction}")
                };
                return new { Label = key, Value = result };
            }).ToList();

        // Extract aligned labels and values
        var labels = groupedData.Select(g => g.Label).ToArray();
        var values = groupedData.Select(g => g.Value).ToArray();

        return (labels, values);
    }

        /// <summary>
    ///     Groups the dataset by the specified field.
    /// </summary>
    /// <param name="groupByField">The field name to group by.</param>
    /// <returns>A <see cref="DataTable" /> containing grouped data with a count of records in each group.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified field is not groupable.</exception>
    public DataTable GroupBy(string groupByField)
    {
        EnsureSetup();

        var groupableField = _schema!.FirstOrDefault(p =>
            p.Name.Equals(groupByField, StringComparison.OrdinalIgnoreCase) &&
            p.DataSetControls?.ContainsKey("IsGroupable") == true);

        if (groupableField == null) throw new ArgumentException($"Field '{groupByField}' is not groupable.");

        var groupedData = _dataTable!.AsEnumerable()
            .GroupBy(row => row[groupByField])
            .Select(group => new
            {
                group.Key,
                Count = group.Count()
            });

        return ConvertToDataTable(groupedData, [groupByField, "Count"]);
    }

    /// <summary>
    ///     Applies an aggregation function (e.g., Average, Sum, Min, Max) to a specific field.
    /// </summary>
    /// <param name="aggregateField">The field to aggregate.</param>
    /// <param name="aggregateFunction">The aggregation function to apply (e.g., "Average", "Sum").</param>
    /// <returns>A <see cref="DataTable" /> containing the aggregated results.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified field is not an aggregator.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified aggregation function is unsupported.</exception>
    public DataTable Aggregate(string aggregateField, string aggregateFunction)
    {
        EnsureSetup();

        var aggregatorField = _schema!.FirstOrDefault(p =>
            p.Name.Equals(aggregateField, StringComparison.OrdinalIgnoreCase) &&
            p.DataSetControls?.ContainsKey("IsAggregator") == true);

        if (aggregatorField == null) throw new ArgumentException($"Field '{aggregateField}' is not an aggregator.");

        var aggregatedData = _dataTable!.AsEnumerable()
            .GroupBy(row => row[aggregateField])
            .Select(group =>
            {
                var result = aggregateFunction switch
                {
                    "Average" => group.Average(r => Convert.ToDouble(r[aggregateField])),
                    "Sum" => group.Sum(r => Convert.ToDouble(r[aggregateField])),
                    "Min" => group.Min(r => Convert.ToDouble(r[aggregateField])),
                    "Max" => group.Max(r => Convert.ToDouble(r[aggregateField])),
                    _ => throw new InvalidOperationException($"Unsupported aggregation function: {aggregateFunction}")
                };

                return new
                {
                    Field = group.Key,
                    Result = result
                };
            });

        return ConvertToDataTable(aggregatedData, ["Field", "Result"]);
    }

    /// <summary>
    ///     Filters the dataset based on a condition.
    /// </summary>
    /// <param name="filterField">The field to filter on.</param>
    /// <param name="filterCondition">A function specifying the condition to filter by.</param>
    /// <returns>A <see cref="DataTable" /> containing rows that satisfy the filter condition.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified field is not filterable.</exception>
    public DataTable Filter(string filterField, Func<object, bool> filterCondition)
    {
        EnsureSetup();

        var filterableField = _schema!.FirstOrDefault(p =>
            p.Name.Equals(filterField, StringComparison.OrdinalIgnoreCase) &&
            p.DataSetControls?.ContainsKey("Filterable") == true);

        if (filterableField == null) throw new ArgumentException($"Field '{filterField}' is not filterable.");

        var filteredRows = _dataTable!.AsEnumerable()
            .Where(row => filterCondition(row[filterField]));

        return filteredRows.CopyToDataTable();
    }
    
    /// <summary>
    /// Calculates the standard deviation of a numerical field in the dataset.
    /// </summary>
    /// <param name="field">The name of the numerical field to calculate the standard deviation for.</param>
    /// <returns>The standard deviation of the field values.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the dataset has not been properly set up.</exception>
    public double StandardDeviation(string field)
    {
        EnsureSetup();

        var values = _dataTable!.AsEnumerable()
            .Select(row => Convert.ToDouble(row[field]))
            .ToArray();

        var mean = values.Average();
        var variance = values.Average(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(variance);
    }

    /// <summary>
    /// Calculates the median of a numerical field in the dataset.
    /// </summary>
    /// <param name="field">The name of the numerical field to calculate the median for.</param>
    /// <returns>The median value of the field.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the dataset has not been properly set up or if the dataset is empty.
    /// </exception>
    public double Median(string field)
    {
        EnsureSetup();

        var values = _dataTable!.AsEnumerable()
            .Select(row => Convert.ToDouble(row[field]))
            .OrderBy(v => v)
            .ToArray();

        var count = values.Length;
        if (count == 0) throw new InvalidOperationException("Cannot compute median on an empty dataset.");

        return count % 2 == 0
            ? (values[count / 2 - 1] + values[count / 2]) / 2.0
            : values[count / 2];
    }

    /// <summary>
    /// Finds the mode(s) of a field in the dataset.
    /// </summary>
    /// <param name="field">The name of the field to calculate the mode for.</param>
    /// <returns>A list of the most frequent value(s) in the field.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the dataset has not been properly set up.</exception>
    public List<object> Mode(string field)
    {
        EnsureSetup();

        // Group data and materialize into a list to avoid multiple enumeration
        var grouped = _dataTable!.AsEnumerable()
            .GroupBy(row => row[field])
            .Select(group => new { Value = group.Key, Count = group.Count() })
            .ToList(); // Materialize to avoid multiple enumeration

        // Find the maximum count
        var maxCount = grouped.Max(g => g.Count);

        // Select the modes (values with the maximum count)
        return grouped.Where(g => g.Count == maxCount).Select(g => g.Value).ToList();
    }

    /// <summary>
    /// Calculates the nth percentile of a numerical field in the dataset.
    /// </summary>
    /// <param name="field">The name of the numerical field to calculate the percentile for.</param>
    /// <param name="percentile">The percentile to calculate (0-100).</param>
    /// <returns>The nth percentile value of the field.</returns>
    /// <exception cref="ArgumentException">Thrown if the percentile is not between 0 and 100.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the dataset has not been properly set up.</exception>
    public double Percentile(string field, double percentile)
    {
        if (percentile is < 0 or > 100)
            throw new ArgumentException("Percentile must be between 0 and 100.");

        EnsureSetup();

        var values = _dataTable!.AsEnumerable()
            .Select(row => Convert.ToDouble(row[field]))
            .OrderBy(v => v)
            .ToArray();

        var rank = (percentile / 100) * (values.Length - 1);
        var lowerIndex = (int)Math.Floor(rank);
        var upperIndex = (int)Math.Ceiling(rank);

        if (lowerIndex == upperIndex) return values[lowerIndex];

        var weight = rank - lowerIndex;
        return values[lowerIndex] * (1 - weight) + values[upperIndex] * weight;
    }

    /// <summary>
    /// Calculates the Pearson correlation coefficient between two numerical fields in the dataset.
    /// </summary>
    /// <param name="fieldX">The name of the first numerical field.</param>
    /// <param name="fieldY">The name of the second numerical field.</param>
    /// <returns>The Pearson correlation coefficient between the two fields.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the dataset has not been properly set up or if the fields have different lengths.
    /// </exception>
    public double Correlation(string fieldX, string fieldY)
    {
        EnsureSetup();

        var xValues = _dataTable!.AsEnumerable().Select(row => Convert.ToDouble(row[fieldX])).ToArray();
        var yValues = _dataTable!.AsEnumerable().Select(row => Convert.ToDouble(row[fieldY])).ToArray();

        if (xValues.Length != yValues.Length || xValues.Length == 0)
            throw new InvalidOperationException("Fields must have the same number of values.");

        var meanX = xValues.Average();
        var meanY = yValues.Average();

        var covariance = xValues.Zip(yValues, (x, y) => (x - meanX) * (y - meanY)).Sum();
        var stdDevX = Math.Sqrt(xValues.Sum(x => Math.Pow(x - meanX, 2)));
        var stdDevY = Math.Sqrt(yValues.Sum(y => Math.Pow(y - meanY, 2)));

        return covariance / (stdDevX * stdDevY);
    }

    /// <summary>
    /// Normalizes a numerical field in the dataset to a range of 0 to 1.
    /// </summary>
    /// <param name="field">The name of the numerical field to normalize.</param>
    /// <returns>A new <see cref="DataTable"/> with the normalized field values.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the dataset has not been properly set up.</exception>
    public DataTable Normalize(string field)
    {
        EnsureSetup();

        var min = _dataTable!.AsEnumerable().Min(row => Convert.ToDouble(row[field]));
        var max = _dataTable!.AsEnumerable().Max(row => Convert.ToDouble(row[field]));

        var normalizedTable = _dataTable!.Copy();
        foreach (DataRow row in normalizedTable.Rows)
        {
            var value = Convert.ToDouble(row[field]);
            row[field] = (value - min) / (max - min);
        }

        return normalizedTable;
    }
        
    /// <summary>
    ///     Ensures that the service has been properly set up with a dataset and schema.
    /// </summary>
    private void EnsureSetup()
    {
        if (_dataTable == null || _schema == null)
            throw new InvalidOperationException(
                "DatasetService has not been properly set up. Call the Setup method first.");
    }

    /// <summary>
    ///     Deserializes the JSON schema into a list of <see cref="DynamicPropertyMetadata" /> objects.
    /// </summary>
    /// <param name="jsonSchema">The JSON schema to deserialize.</param>
    /// <returns>A list of <see cref="DynamicPropertyMetadata" /> objects.</returns>
    /// <exception cref="ArgumentException">Thrown if the JSON schema is invalid, empty, or cannot be parsed.</exception>
    private static List<DynamicPropertyMetadata> DeserializeSchema(string jsonSchema)
    {
        if (string.IsNullOrWhiteSpace(jsonSchema))
            throw new ArgumentException("JSON schema cannot be null or empty.", nameof(jsonSchema));

        try
        {
            var schema = JsonSerializer.Deserialize<List<DynamicPropertyMetadata>>(jsonSchema);

            if (schema == null || schema.Count == 0)
                throw new ArgumentException("The provided JSON schema is invalid or empty.");

            return schema;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Error parsing JSON schema: {ex.Message}", nameof(jsonSchema), ex);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Unexpected error while processing JSON schema: {ex.Message}",
                nameof(jsonSchema), ex);
        }
    }

    /// <summary>
    ///     Converts a collection of grouped or aggregated results into a <see cref="DataTable" />.
    /// </summary>
    /// <typeparam name="T">The type of the grouped or aggregated results.</typeparam>
    /// <param name="data">The data to convert.</param>
    /// <param name="columnNames">The column names for the resulting <see cref="DataTable" />.</param>
    /// <returns>A <see cref="DataTable" /> containing the converted data.</returns>
    private static DataTable ConvertToDataTable<T>(IEnumerable<T> data, string[] columnNames)
    {
        var dataTable = new DataTable();

        foreach (var col in columnNames) dataTable.Columns.Add(col);

        foreach (var item in data)
        {
            var values = item?.GetType().GetProperties()
                .Select(prop => prop.GetValue(item))
                .ToArray();

            if (values != null) dataTable.Rows.Add(values);
        }

        return dataTable;
    }
}