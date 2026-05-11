using System.ComponentModel.DataAnnotations;
using CosmosDbManager.Application.DTOs.Request;

namespace CosmosDbManager.Web.ViewModels;

public sealed class ConfigurationViewModel
{
    [Required]
    [Display(Name = "Connection String")]
    [DataType(DataType.Password)]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Database Name")]
    public string DatabaseName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Container Name")]
    public string ContainerName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Partition Key")]
    public string PartitionKey { get; set; } = string.Empty;

    public static ConfigurationViewModel FromDto(CosmosConfigurationDto? dto)
    {
        if (dto is null)
        {
            return new ConfigurationViewModel();
        }

        return new ConfigurationViewModel
        {
            ConnectionString = dto.ConnectionString,
            DatabaseName = dto.DatabaseName,
            ContainerName = dto.ContainerName,
            PartitionKey = dto.PartitionKey
        };
    }

    public CosmosConfigurationDto ToDto()
    {
        return new CosmosConfigurationDto
        {
            ConnectionString = ConnectionString.Trim(),
            DatabaseName = DatabaseName.Trim(),
            ContainerName = ContainerName.Trim(),
            PartitionKey = PartitionKey.Trim()
        };
    }
}
