using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpDataLoader.Workers;

namespace KpDataLoader
{
    public class MainWorker: IWorker
    {
        private readonly ProbabilityFactory<IWorker> _workerFactory;

        public MainWorker(ProbabilityFactory<IWorker> workerFactory)
        {
            this._workerFactory = workerFactory;
        }

        public async Task<bool> RunAsync(CancellationToken ct = default)
        {
            var worker = this._workerFactory.Create();
            return await worker.RunAsync(ct);
        }
    }
}
