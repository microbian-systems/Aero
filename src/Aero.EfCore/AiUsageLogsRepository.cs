using Aero.Core.Data;

namespace Aero.EfCore;

/// <summary>
/// Defines an interface for IAiUsageLogRepository.
/// </summary>
public interface IAiUsageLogRepository : IGenericRepository<AiUsageLog>;

/// <summary>
/// Represents a class for AiUsageLogsRepository.
/// </summary>
public sealed class AiUsageLogsRepository(AeroDbContext context, ILogger<AiUsageLogsRepository> log)
    : GenericEntityFrameworkRepository<AiUsageLog>(context, log), IAiUsageLogRepository;