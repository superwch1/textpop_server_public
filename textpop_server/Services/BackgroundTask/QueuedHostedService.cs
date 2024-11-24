namespace textpop_server.Services.BackgroundTask
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public IBackgroundTaskQueue TaskQueue { get; }

        public QueuedHostedService(IBackgroundTaskQueue taskQueue, IServiceScopeFactory serviceScopeFactory)
        {
            TaskQueue = taskQueue;
            _serviceScopeFactory = serviceScopeFactory;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken);

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope()) // Create a new scope
                    {
                        await workItem(scope.ServiceProvider, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
        }
    }
}
