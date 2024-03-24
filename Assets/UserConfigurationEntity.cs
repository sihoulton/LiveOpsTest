using Azure;
using Azure.Data.Tables;
using System;
using System.Runtime.Serialization;

public class UserConfigurationEntity: ITableEntity
{
	public string PartitionKey { get; set; }
	public string RowKey { get; set; }
	public DateTimeOffset? Timestamp { get; set; }
	public ETag ETag { get; set; }

	[DataMember(Name = "RowKey")]
	public string UserName { get; set; }

	[DataMember(Name = "Version")]
	public decimal Version{ get; set; }

	[DataMember(Name = "Difficulty")]
	public string Difficulty { get; set; }
}