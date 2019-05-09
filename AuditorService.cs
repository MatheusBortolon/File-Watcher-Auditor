using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace FW
{
    public class AuditorService
    {
        private Dictionary<string, FileSystemWatcher> watchers;
        private Config config;
        private HttpClient httpClient;
        private string path;

        public AuditorService()
        {
            watchers = new Dictionary<string, FileSystemWatcher>();
            httpClient = new HttpClient();
            path = @".\delettedFiles.log";
        }

        public void Start()
        {
            CarregarConfig();
            
            foreach(var caminho in config.Caminhos)
                CriarWatcher(caminho);
        }

        public void Stop()
        {
            var items = watchers.ToList();
            foreach (var watcherItem in items)
            {
                watcherItem.Value.EnableRaisingEvents = false;

                if (!string.IsNullOrEmpty(config.ApiSalvar))
                    watcherItem.Value.Deleted -= AuditApi;
                else
                    watcherItem.Value.Deleted -= AuditFile;

                watcherItem.Value.Dispose();
                watchers.Remove(watcherItem.Key);
            }
        }

        private void CarregarConfig()
        {
            using (StreamReader r = new StreamReader("fw.config.json"))
            {
                string json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<Config>(json);
            }
        }

        private void CriarWatcher(string caminho)
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = caminho;
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.Security;

            if (!string.IsNullOrEmpty(config.ApiSalvar))
                watcher.Deleted += AuditApi;
            else
                watcher.Deleted += AuditFile;

            watcher.EnableRaisingEvents = true;

            watchers.Add(caminho, watcher);
        }

        private async void AuditFile(object source, FileSystemEventArgs e)
        {
            var message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + e.FullPath;

            using (StreamWriter sw = File.AppendText(path))
                await sw.WriteLineAsync(message);
        }

        private async void AuditApi(object source, FileSystemEventArgs e)
        {
            var message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + e.FullPath;

            var body = new StringContent(message, System.Text.Encoding.UTF8, "application/json");
            await httpClient.PostAsync(config.ApiSalvar, body);
        }
    }
}
