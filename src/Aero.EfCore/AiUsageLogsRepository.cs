using Aero.Core.Data;

namespace Aero.EfCore;

public interface IAiUsageLogRepository : IGenericRepository<AiUsageLog>;

public sealed class AiUsageLogsRepository(AeroDbContext context, ILogger<AiUsageLogsRepository> log)
    : GenericEntityFrameworkRepository<AiUsageLog>(context, log), IAiUsageLogRepository;