using Newtonsoft.Json;
using System.IO;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface IDataService
    {
        List<TimeLogEntry> LoadTimeLogEntries(DateTime date);
        void SaveTimeLogEntries(DateTime date, IEnumerable<TimeLogEntry> entries);
    }

    public class DataService : IDataService
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
            var allEntries = LoadAllEntriesForMonth(date);
            allEntries[date.Day] = entries.ToList();
            var sortedEntries = allEntries.OrderBy(e => e.Key).ToDictionary(e => e.Key, e => e.Value);
            var json = JsonConvert.SerializeObject(sortedEntries, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public List<TimeLogEntry> LoadTimeLogEntries(DateTime date)
        {
            var allEntries = LoadAllEntriesForMonth(date);
            return allEntries.ContainsKey(date.Day) ? allEntries[date.Day] : new List<TimeLogEntry>();
        }

        private Dictionary<int, List<TimeLogEntry>> LoadAllEntriesForMonth(DateTime date)
        {
            var filePath = GetFilePath(date);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Dictionary<int, List<TimeLogEntry>>>(json)
                        ?? new Dictionary<int, List<TimeLogEntry>>();
            }
            return new Dictionary<int, List<TimeLogEntry>>();
        }

        protected virtual string GetFilePath(DateTime date)
        {
            return Path.Combine(dataDirectory, $"{date:yyyy-MM}.json");
        }
    }
}
