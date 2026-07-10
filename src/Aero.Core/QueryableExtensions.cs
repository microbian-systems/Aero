namespace Aero.Core;

/// <summary>
/// Represents a class for QueryableExtensions.
/// </summary>
public static class QueryableExtensions
{
        /// <summary>
    /// ToPaginatedListAsync method.
    /// </summary>
public static Task<PaginatedResult<T>> ToPaginatedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize) where T : class
    {
        if (source == null) 
            throw new ArgumentNullException();
            
        pageNumber = pageNumber == 0 ? 1 : pageNumber;
        pageSize = pageSize == 0 ? 10 : pageSize;
        var count = source.Count();
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            
        var items =  source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsEnumerable();
            
        return Task.FromResult(PaginatedResult<T>
            .Success(items, count, pageNumber, pageSize));
    }
}
    
/// <summary>
/// Represents a class for PaginatedResult.
/// </summary>
public class PaginatedResult<T> //: Result
{
        /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedResult"/> class.
    /// </summary>
public PaginatedResult() { }
        /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedResult"/> class.
    /// </summary>
public PaginatedResult(List<T> data)
    {
        Data = data;
    }

        /// <summary>
    /// Gets or sets the Data.
    /// </summary>
public IEnumerable<T> Data { get; set; } = new List<T>();
        /// <summary>
    /// Gets or sets the Succeeded.
    /// </summary>
public bool Succeeded { get; set; }
        /// <summary>
    /// Gets or sets the Current Page.
    /// </summary>
public int CurrentPage { get; set; }
        /// <summary>
    /// Gets or sets the Total Pages.
    /// </summary>
public int TotalPages { get; set; }
        /// <summary>
    /// Gets or sets the Total Count.
    /// </summary>
public int TotalCount { get; set; }
        /// <summary>
    /// Gets or sets the Page Size.
    /// </summary>
public int PageSize { get; set; }
        /// <summary>
    /// Gets or sets the Has Previous Page.
    /// </summary>
public bool HasPreviousPage => CurrentPage > 1;
        /// <summary>
    /// Gets or sets the Has Next Page.
    /// </summary>
public bool HasNextPage => CurrentPage < TotalPages;

    internal PaginatedResult(bool succeeded, IEnumerable<T> data = default, IEnumerable<string> messages = null, int count = 0, int page = 1, int pageSize = 10)
    {
        Data = data;
        CurrentPage = page;
        Succeeded = succeeded;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
    }
        
        /// <summary>
    /// Failure method.
    /// </summary>
public static PaginatedResult<T> Failure(IEnumerable<string> messages)
    {
        return new PaginatedResult<T>(false, default, messages);
    }

        /// <summary>
    /// Success method.
    /// </summary>
public static PaginatedResult<T> Success(IEnumerable<T> data, int count, int page, int pageSize)
    {
        return new PaginatedResult<T>(true, data, null, count, page, pageSize);
    }
}