using LibraryLending.Application.Common.Models;

namespace LibraryLending.Application.Common.Extensions;

public static class PagedResponseExtensions
{
    public static PagedResponse<TDestination> Map<TSource, TDestination>(
        this PagedResponse<TSource> source,
        Func<TSource, TDestination> mapper)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);

        return new PagedResponse<TDestination>
        {
            Items = source.Items.Select(mapper).ToArray(),
            Page = source.Page,
            PageSize = source.PageSize,
            TotalCount = source.TotalCount
        };
    }
}
