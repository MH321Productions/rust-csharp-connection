namespace RustConnectionTest;

public static class ArrayExtensions
{
    public static string Print<T>(this IEnumerable<T> arr)
    {
        var items = arr
            .Select(t => t?.ToString() ?? "null")
            .Aggregate((a, b) => $"{a}, {b}");

        return $"[{items}]";
    }
}