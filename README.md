# IQueryable Filter Extension

A powerful and flexible `IQueryable` filter extension for .NET projects, designed to dynamically apply complex filtering logic to LINQ queries. Supports nested **AND** conditions within groups and **OR** conditions between groups.

## Features

- **Dynamic Filtering:** Apply filters dynamically based on runtime input.
- **Nested Logic:** Supports nested `AND` conditions within groups and `OR` logic between groups.
- **Multiple Data Types:** Works with strings, numbers, dates, and booleans.
- **Extendable:** Easily customizable for additional filter types or logic.
- **Optimized for Performance:** Translates filters to optimized SQL queries when used with Entity Framework.

## Models

### FilterModel
Defines the structure of a single filter.

```csharp
public class FilterModel
{
    public string ColumnName { get; set; }
    public string Value { get; set; }
    public FilterType FilterType { get; set; } = FilterType.Contain;

    public FilterModel(string columnName, string value, FilterType filterType)
    {
        ColumnName = columnName;
        Value = value;
        FilterType = filterType;
    }
}

public enum FilterType
{
    Equal = 1,
    NotEqual,
    Contain,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    StartWith,
    EndWith,
}

var filters = new List<List<FilterModel>>
{
    // Group 1: (Name contains "Laptop" AND Price greater than 1000)
    new List<FilterModel>
    {
        new FilterModel("Name", "Laptop", FilterType.Contain),
        new FilterModel("Price", "1000", FilterType.GreaterThan)
    },
    // Group 2: (CreatedDate less than 2023-04-01 AND Name starts with "M")
    new List<FilterModel>
    {
        new FilterModel("CreatedDate", "2023-04-01", FilterType.LessThan),
        new FilterModel("Name", "M", FilterType.StartWith)
    }
};

using (var context = new AppDbContext())
{
    var query = context.Products.AsQueryable();

    // Apply the filters
    var filteredProducts = query.Filter(filters).ToList();

    // Display results
    foreach (var product in filteredProducts)
    {
        Console.WriteLine($"Id: {product.Id}, Name: {product.Name}, Price: {product.Price}, CreatedDate: {product.CreatedDate}");
    }
}
```
### Example Database

| Id | Name      | Price  | CreatedDate  |
|----|-----------|--------|--------------|
| 1  | Laptop    | 1200.5 | 2023-01-01   |
| 2  | Mouse     | 25.99  | 2023-02-01   |
| 3  | Keyboard  | 49.99  | 2023-03-01   |
| 4  | Monitor   | 250.00 | 2023-04-01   |
| 5  | Phone     | 800.00 | 2023-05-01   |

### Expected Output

```plaintext
Id: 1, Name: Laptop, Price: 1200.5, CreatedDate: 01/01/2023
Id: 2, Name: Mouse, Price: 25.99, CreatedDate: 01/02/2023
