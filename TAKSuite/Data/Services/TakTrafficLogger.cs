namespace TAKSuite.Data.Services
{
    public class TakTrafficLogger
    {
        private readonly string _logFile;
        private readonly SemaphoreSlim _sem = new(1, 1);

        public string LogDirectory { get; }

        public TakTrafficLogger()
        {
            LogDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(LogDirectory);
            _logFile = Path.Combine(LogDirectory, $"tak-traffic-{DateTime.Now:yyyyMMdd}.log");
        }

        public void Write(string category, string message)
        {
            _ = WriteInternalAsync(category, message);
        }

        private async Task WriteInternalAsync(string category, string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss.fff} [{category,-10}] {message}{Environment.NewLine}";
            await _sem.WaitAsync();
            try { await File.AppendAllTextAsync(_logFile, line); }
            catch { }
            finally { _sem.Release(); }
        }
    }
}
