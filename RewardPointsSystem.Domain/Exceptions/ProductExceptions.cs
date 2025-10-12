using System;

namespace RewardPointsSystem.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a product is not found.
    /// </summary>
    public class ProductNotFoundException : DomainException
    {
        public Guid ProductId { get; }

        public ProductNotFoundException(Guid productId) 
            : base($"Product with ID '{productId}' was not found.")
        {
            ProductId = productId;
        }
    }

    /// <summary>
    /// Exception thrown when product data is invalid.
    /// </summary>
    public class InvalidProductDataException : DomainException
    {
        public InvalidProductDataException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a product is out of stock.
    /// </summary>
    public class ProductOutOfStockException : DomainException
    {
        public Guid ProductId { get; }
        public string ProductName { get; }

        public ProductOutOfStockException(Guid productId, string productName) 
            : base($"Product '{productName}' (ID: {productId}) is out of stock.")
        {
            ProductId = productId;
            ProductName = productName;
        }
    }

    /// <summary>
    /// Exception thrown when insufficient inventory is available.
    /// </summary>
    public class InsufficientInventoryException : DomainException
    {
        public Guid ProductId { get; }
        public int RequestedQuantity { get; }
        public int AvailableQuantity { get; }

        public InsufficientInventoryException(Guid productId, int requestedQuantity, int availableQuantity) 
            : base($"Insufficient inventory for product '{productId}'. Requested: {requestedQuantity}, Available: {availableQuantity}")
        {
            ProductId = productId;
            RequestedQuantity = requestedQuantity;
            AvailableQuantity = availableQuantity;
        }
    }

    /// <summary>
    /// Exception thrown when no active pricing is found for a product.
    /// </summary>
    public class ProductPricingNotFoundException : DomainException
    {
        public Guid ProductId { get; }

        public ProductPricingNotFoundException(Guid productId) 
            : base($"No active pricing found for product '{productId}'.")
        {
            ProductId = productId;
        }
    }

    /// <summary>
    /// Exception thrown when inventory is not found for a product.
    /// </summary>
    public class InventoryNotFoundException : DomainException
    {
        public Guid ProductId { get; }

        public InventoryNotFoundException(Guid productId) 
            : base($"No inventory record found for product '{productId}'.")
        {
            ProductId = productId;
        }
    }
}
