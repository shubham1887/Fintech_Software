using FintechApp.Data;
using FintechApp.Entities;

namespace FintechApp.Helpers
{
    public class Logger
    {
        public void Write(Exception ex)
        {
            try
            {
                // Log to file
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                string fileName = $"Error_{DateTime.Now:yyyyMMdd}.log";
                string filePath = Path.Combine(logPath, fileName);

                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.Message}\n{ex.StackTrace}\n";
                if (ex.InnerException != null)
                {
                    logMessage += $"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}\n";
                }
                logMessage += new string('-', 100) + "\n";

                File.AppendAllText(filePath, logMessage);
            }
            catch
            {
                // Fail silently - logging should not break the application
            }
        }

        public void Write(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                string fileName = $"Info_{DateTime.Now:yyyyMMdd}.log";
                string filePath = Path.Combine(logPath, fileName);

                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(filePath, logMessage);
            }
            catch
            {
                // Fail silently
            }
        }

        public async Task WriteToDatabase(ApplicationDbContext context, int userId, string action, string module, string oldValue = "", string newValue = "")
        {
            try
            {
                var log = new AuditLog
                {
                    UserID = userId,
                    Action = action,
                    Module = module,
                    OldValue = oldValue,
                    NewValue = newValue,
                    IPAddress = string.Empty,
                    DateTime = DateTime.Now,
                    UpdatedateTime = DateTime.Now,
                    IsDeleted = false
                };

                await context.AuditLogs.AddAsync(log);
                await context.SaveChangesAsync();
            }
            catch
            {
                // Fail silently
            }
        }
    }
}
