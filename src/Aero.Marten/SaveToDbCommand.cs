namespace Aero.Marten;

public class SaveToDbCommand<T>(IDocumentSession db, ILogger<SaveToDbCommand<T>> log) : ISaveToDbCommand<T>
    where T : Entity<string>, IEntity<string>
{
    public async Task<T> ExecuteAsync(T parameter)
    {
        log.LogInformation($"saving {parameter.GetType()} to database");
        db.Store(parameter);
        parameter.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var message = $"successfully saved {parameter.GetType()} to database with id {parameter.Id}";
        log.LogInformation(message);

        return parameter;
    }
}