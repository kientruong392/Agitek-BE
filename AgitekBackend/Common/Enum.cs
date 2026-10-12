namespace AgitekBackend.Common
{
    public enum Provider
    {
        Credentials,
        Google
    }

    public enum RoleName
    {
        SuperAdmin,
        Admin,
        Manager,
        Staff,
        Delivery,
        Customer,
        Guest
    }

    public enum GenderType
    {
        Male,
        Female,
        Other
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
        Completed,
        Returned,
        Cancelled,
        Refunded
    }

    public enum RefundStatus
    {
        Pending,
        Approved,
        Rejected,
    }

    public enum PaymentMethod
    {
        COD,
        Bank,
        Momo
    }

    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed,
        Refunded
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
        LowStock,
        OutOfStock
    }

    public enum WarrantyStatus
    {
        Pending,
        Processing,
        Completed,
        Rejected
    }

    public enum LocationStatus
    {
        Active,
        Inactive,
        Maintenance
    }
}
