using System.Text.Json.Serialization;

namespace AgitekBackend.DTOs.Payments
{
    public class SePayWebhookDTO
    {
        [JsonPropertyName("transferType")]
        public string? TransferType { get; set; }

        [JsonPropertyName("transfer_type")]
        public string? TransferTypeSnakeCase { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("transferAmount")]
        public decimal TransferAmount { get; set; }

        [JsonPropertyName("transfer_amount")]
        public decimal TransferAmountSnakeCase { get; set; }

        [JsonPropertyName("referenceNumber")]
        public string? ReferenceNumber { get; set; }

        [JsonPropertyName("reference_number")]
        public string? ReferenceNumberSnakeCase { get; set; }

        [JsonIgnore]
        public string? EffectiveTransferType => TransferType ?? TransferTypeSnakeCase;

        [JsonIgnore]
        public decimal EffectiveTransferAmount => TransferAmount != 0 ? TransferAmount : TransferAmountSnakeCase;

        [JsonIgnore]
        public string? EffectiveReferenceNumber => ReferenceNumber ?? ReferenceNumberSnakeCase;
    }
}
