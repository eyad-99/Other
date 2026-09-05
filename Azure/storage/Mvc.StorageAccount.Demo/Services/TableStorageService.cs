using Azure;
using Azure.Data.Tables;
using Mvc.StorageAccount.Demo.Data;
using Mvc.StorageAccount.Demo.Services;

public class TableStorageService : ITableStorageService
{
    private readonly TableClient _tableClient;

    public TableStorageService(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<AttendeeEntity> GetAttendee(string industry, string id)
    {
        await _tableClient.CreateIfNotExistsAsync();
        return await _tableClient.GetEntityAsync<AttendeeEntity>(industry, id);
    }

    public async Task<List<AttendeeEntity>> GetAttendees()
    {
        await _tableClient.CreateIfNotExistsAsync();
        Pageable<AttendeeEntity> attendeeEntities = _tableClient.Query<AttendeeEntity>();
        return attendeeEntities.ToList();
    }

    public async Task UpsertAttendee(AttendeeEntity attendeeEntity)
    {
        await _tableClient.CreateIfNotExistsAsync();
        await _tableClient.UpsertEntityAsync(attendeeEntity);
    }

    public async Task DeleteAttendee(string industry, string id)
    {
        await _tableClient.CreateIfNotExistsAsync();
        await _tableClient.DeleteEntityAsync(industry, id);
    }
}
