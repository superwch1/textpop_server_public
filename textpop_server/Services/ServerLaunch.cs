using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace MessageWebServer.Services
{
    public class ServerLaunch : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var defaultApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("textpop-1c7b9-firebase-adminsdk-ax0av-020c938d7e.json"),
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
