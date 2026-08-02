namespace Aero.Marten;

/// <summary>
/// Represents a class for SaveToDbCommand.
/// </summary>
public class SaveToDbCommand<T>(IDocumentSession db, ILogger<SaveToDbCommand<T>> log) : ISaveToDbCommand<T>
    where T : Entity<string>, IEntity<string>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
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