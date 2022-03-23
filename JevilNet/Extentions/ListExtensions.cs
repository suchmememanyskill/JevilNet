namespace JevilNet.Extentions;

public static class ListExtensions
{
    public static IEnumerable<IEnumerable<T>> SplitInParts<T>(this IEnumerable<T> list, int partLength)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));
        if (partLength <= 0)
            throw new ArgumentException("Part length has to be positive", nameof(partLength));

        var enumerable = list.ToList();
        for (var i = 0; i < enumerable.Count; i += partLength)
        {
            yield return enumerable.Skip(i).Take(partLength);
        }
    }
}