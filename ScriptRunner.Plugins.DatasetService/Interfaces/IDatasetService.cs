using System;
using System.Collections.Generic;
using System.Data;

namespace ScriptRunner.Plugins.DatasetService.Interfaces;

/// <summary>
///     Defines the contract for a dataset service that performs operations
///     such as grouping, filtering, aggregation, and statistical analysis
///     on datasets based on a schema definition.
/// </summary>
public interface IDatasetService
{
    /// <summary>
    ///     Configures the dataset service with a dataset and schema.
    /// </summary>
    /// <param name="dataTable">The dataset represented as a <see cref="DataTable" />.</param>
    /// <param name="jsonSchema">A JSON schema defining the dataset's structure and controls.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="dataTable" /> or <paramref name="jsonSchema" /> is
    ///     null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="jsonSchema" /> is invalid or cannot be parsed.</exception>
    void Setup(DataTable dataTable, string jsonSchema);

    /// <summary>
    ///     Prepares data for charting by grouping and aggregating values.
    /// </summary>
    /// <param name="groupByField">The field to group by.</param>
    /// <param name="aggregateField">The field to aggregate.</param>
    /// <param name="aggregateFunction">The aggregation function (e.g., "Average", "Sum").</param>
    /// <returns>
    ///     A tuple containing:
    ///     <c>Labels</c> - The group labels, and
    ///     <c>Values</c> - The aggregated values.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="groupByField" /> is not groupable or <paramref name="aggregateField" /> is not
    ///     aggregatable.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="aggregateFunction" /> is unsupported.</exception>
    (string[] Labels, double[] Values) PrepareChartDataset(
        string groupByField, string aggregateField, string aggregateFunction);

    /// <summary>
    ///     Groups the dataset by a specified field.
    /// </summary>
    /// <param name="groupByField">The name of the field to group by.</param>
    /// <returns>A <see cref="DataTable" /> containing grouped data and a count of records in each group.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified field is not groupable.</exception>
    DataTable GroupBy(string groupByField);

    /// <summary>
    ///     Applies an aggregation function (e.g., "Average", "Sum", "Min", "Max") to a specific field.
    /// </summary>
    /// <param name="aggregateField">The name of the field to aggregate.</param>
    /// <param name="aggregateFunction">The aggregation function to apply.</param>
    /// <returns>A <see cref="DataTable" /> containing the aggregated results.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified field is not an aggregator.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the aggregation function is unsupported.</exception>
    DataTable Aggregate(string aggregateField, string aggregateFunction);

    /// <summary>
    ///     Filters the dataset based on a condition applied to a specific field.
    /// </summary>
    /// <param name="filterField">The name of the field to filter on.</param>
    /// <param name="filterCondition">A function defining the filter condition.</param>
    /// <returns>A <see cref="DataTable" /> containing rows that satisfy the filter condition.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified field is not filterable.</exception>
    DataTable Filter(string filterField, Func<object, bool> filterCondition);

    /// <summary>
    ///     Calculates the standard deviation of a numerical field in the dataset.
    /// </summary>
    /// <param name="field">The name of the numerical field.</param>
    /// <returns>The standard deviation of the field values.</returns>
    double StandardDeviation(string field);

    /// <summary>
    ///     Calculates the median value of a numerical field in the dataset.
    /// </summary>
    /// <param name="field">The name of the numerical field.</param>
    /// <returns>The median value of the field.</returns>
    double Median(string field);

    /// <summary>
    ///     Finds the mode(s) of a field in the dataset.
    /// </summary>
    /// <param name="field">The name of the field.</param>
    /// <returns>A list of the most frequent value(s) in the field.</returns>
    List<object> Mode(string field);

    /// <summary>
    ///     Calculates the nth percentile of a numerical field in the dataset.
    /// </summary>
    /// <param name="field">The name of the numerical field.</param>
    /// <param name="percentile">The percentile to calculate (0-100).</param>
    /// <returns>The nth percentile value of the field.</returns>
    /// <exception cref="ArgumentException">Thrown if the percentile is not between 0 and 100.</exception>
    double Percentile(string field, double percentile);

    /// <summary>
    ///     Calculates the Pearson correlation coefficient between two numerical fields.
    /// </summary>
    /// <param name="fieldX">The name of the first numerical field.</param>
    /// <param name="fieldY">The name of the second numerical field.</param>
    /// <returns>The Pearson correlation coefficient between the two fields.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the fields have differing lengths or are empty.</exception>
    double Correlation(string fieldX, string fieldY);

    /// <summary>
    ///     Normalizes a numerical field in the dataset to a range of 0 to 1.
    /// </summary>
    /// <param name="field">The name of the numerical field to normalize.</param>
    /// <returns>A new <see cref="DataTable" /> with the normalized field values.</returns>
    DataTable Normalize(string field);
}