using Newtonsoft.Json;
using System.IO;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class DataService
    {
        private readonly string dataDirectory;

        public DataService()
        {
            dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeTracker");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }

        public void SaveTimeLogEntries(DateTime date, IEnumerable<TimeLogEntry> entries)
        {
            var filePath = GetFilePath(date);
            var json = JsonConvert.SerializeObject(entries);
            File.WriteAllText(filePath, json);
        }

        public List<TimeLogEntry> LoadTimeLogEntries(DateTime date)
        {
            var filePath = GetFilePath(date);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<TimeLogEntry>>(json);
            }
            return new List<TimeLogEntry>();
        }

        private string GetFilePath(DateTime date)
        {
            return Path.Combine(dataDirectory, $"{date:yyyy-MM-dd}.json");
        }
    }
}
