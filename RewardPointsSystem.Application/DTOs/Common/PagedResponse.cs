using System;
using System.Collections.Generic;

namespace RewardPointsSystem.Application.DTOs.Common
{
    /// <summary>
    /// Paginated response for large datasets
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// The collection of items for the current page
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Current page number (1-indexed)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Indicates if there is a previous page
        /// </summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// Indicates if there is a next page
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;

        /// <summary>
        /// Creates a paginated response
        /// </summary>
        public static PagedResponse<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
        {
            return new PagedResponse<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
