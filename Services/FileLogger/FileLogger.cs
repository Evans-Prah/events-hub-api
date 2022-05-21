using Microsoft.Extensions.Configuration;
using System.Text;

namespace Services.FileLogger
{
    public class FileLogger : IFileLogger
    {
        private readonly string error_dir;
        private readonly string info_dir;
        private readonly string warning_dir;
        private ReaderWriterLockSlim _readWriteLock;

        public FileLogger(IConfiguration config)
        {
            _readWriteLock = new ReaderWriterLockSlim();
            error_dir = config["LOG_DIR:ERROR"];
            info_dir = config["LOG_DIR:INFO"];
            warning_dir = config["LOG_DIR:WARNING"];

            if (string.IsNullOrWhiteSpace(error_dir)) error_dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Errors");
            if (string.IsNullOrWhiteSpace(info_dir)) info_dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Info");
            if (string.IsNullOrWhiteSpace(warning_dir)) warning_dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Warning");

            if (!Directory.Exists(error_dir)) Directory.CreateDirectory(error_dir);
            if (!Directory.Exists(info_dir)) Directory.CreateDirectory(info_dir);
            if (!Directory.Exists(warning_dir)) Directory.CreateDirectory(warning_dir);
        }
        private void writeLog(string path, string content)
        {
            // Set Status to Locked
            _readWriteLock.EnterWriteLock();
            try
            {
                // Start Write Method
                using (var sw = new StreamWriter(Path.Combine(path, $"{DateTime.Now.ToString("yyyy-MMM-dd")}.log"), true))
                {
                    content += $"\nEnd Time: {DateTime.Now}\n\n";
                    sw.WriteLine(content);
                }
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }
        }

        public void LogError(string errorMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(errorMessage)) writeLog(error_dir, errorMessage);
            }
            catch (Exception) { }

        }

        public void LogError(Exception errorMessage)
        {
            try
            {
                writeLog(error_dir, errorMessage.ToString());
            }
            catch (Exception) { }

        }


        public void LogInfo(string infoMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(infoMessage)) writeLog(info_dir, infoMessage);
            }
            catch (Exception) { }
        }


        public void LogInfo(StringBuilder infoMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(infoMessage.ToString())) writeLog(info_dir, infoMessage.ToString());
            }
            catch (Exception) { }
        }

        public void LogWarning(string warningMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(warningMessage)) writeLog(warning_dir, warningMessage);
            }
            catch (Exception) { }
        }
    }
}
