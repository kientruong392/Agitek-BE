namespace AgitekBackend.Common
{
    public enum RoleName
    {
        Admin,
        Manager,
        Staff,
        Delivery,
        Customer
    }

    public enum ProductStatus
    {
        Pending,
        InStock,
        LowStock,
        OutOfStock,
        Hidden,
        PreOrder,
        Deleted
    }

    public enum OrderStatus
    {
        Ordered,
        Confirmed,
        Shipped,
        InTransit,
        Delivered,
        Reviewed,
        Returned,
        Cancelled,
        Refunded
    }

    public enum PaymentMethod
    {
        COD,
        Bank,
        Momo
    }

    public enum PaymentStatus
    {
        Unpaid,
        Paid
    }

    public enum MediaType
    {
        Image,
        Video
    }

    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }

    public enum VoucherStatus
    {
        Active,
        Inactive,
        Expired,
        OutOfStock
    }

    public enum WarrantyStatus
    {
        Pending,
        Processing,
        Completed,
        Rejected
    }
}