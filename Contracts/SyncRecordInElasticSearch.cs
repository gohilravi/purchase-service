using MassTransit;

namespace purchase_service.Contracts;

[EntityName("SyncRecordInElasticSearch")]
public class SyncRecordInElasticSearch
{
    public string ElasticSearchId { get; set; } = string.Empty;
    public string ObjectType { get; set; } = "Purchase";
    public string Operation { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}