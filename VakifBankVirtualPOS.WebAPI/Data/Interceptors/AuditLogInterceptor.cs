using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using VakifBankVirtualPOS.WebAPI.Data.Entities;

namespace VakifBankVirtualPOS.WebAPI.Data.Interceptors
{
    public class AuditLogInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var clientCode = _httpContextAccessor.HttpContext?.Session.GetString("ClientCode") ?? null;

            // ToList() ile collection'ı önce listeye çevir
            var auditLogs = eventData.Context.ChangeTracker.Entries()
                .Where(x => x.Entity is not IDT_AUDIT_LOG &&
                       (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted))
                .ToList(); // BURADA ToList() çağırarak collection'ı sabitle

            var auditLogsEntities = new List<IDT_AUDIT_LOG>();

            foreach (var item in auditLogs)
            {
                var log = new IDT_AUDIT_LOG
                {
                    TableName = item.Metadata.GetTableName(),
                    Operation = item.State.ToString(),
                    CreatedAt = DateTime.Now,
                    ClientCode = clientCode
                };

                if (item.State == EntityState.Modified)
                {
                    log.OldValue = JsonSerializer.Serialize(item.OriginalValues.Properties.ToDictionary(p => p.Name, p => item.OriginalValues[p]));
                    log.NewValue = JsonSerializer.Serialize(item.CurrentValues.Properties.ToDictionary(p => p.Name, p => item.CurrentValues[p])); // Burada CurrentValues olmalı
                }
                else if (item.State == EntityState.Added)
                {
                    log.NewValue = JsonSerializer.Serialize(item.CurrentValues.Properties.ToDictionary(p => p.Name, p => item.CurrentValues[p])); // Burada CurrentValues olmalı
                }
                else if (item.State == EntityState.Deleted)
                {
                    log.OldValue = JsonSerializer.Serialize(item.OriginalValues.Properties.ToDictionary(p => p.Name, p => item.OriginalValues[p]));
                }

                auditLogsEntities.Add(log);
            }

            // Döngü BİTTİKTEN SONRA AddRange yap
            if (auditLogsEntities.Any())
            {
                eventData.Context.Set<IDT_AUDIT_LOG>().AddRange(auditLogsEntities);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}