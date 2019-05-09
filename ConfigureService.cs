using Topshelf;

namespace FW
{
    public class ConfigureService
    {
        internal static void Configure()
        {
            HostFactory.Run(configure =>
            {
                configure.Service<AuditorService>(service =>
                {
                    service.ConstructUsing(s => new AuditorService());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                configure.RunAsLocalSystem();
                configure.SetServiceName("[FW] File Watcher");
                configure.SetDisplayName("[FW] File Watcher");
                configure.SetDescription("Auditoria para exclusão de arquivos");
                configure.StartAutomatically();
            });
        }
    }
}
