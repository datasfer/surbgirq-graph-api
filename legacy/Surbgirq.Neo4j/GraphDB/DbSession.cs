using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.GraphDB
{
    public  class DbSession : IDisposable
    {
        private readonly IAsyncSession _session;
        public IAsyncSession AsyncSession => _session;

        private DbSession(IDriver driver, string databaseName, AccessMode accessMode)
        {
            _session = driver.AsyncSession(options => 
            {
                options.WithDatabase(databaseName); 
                options.WithDefaultAccessMode(accessMode); 
            });
        }

        public static DbSession StartSession(IDriver driver, string databaseName, AccessMode accessMode = AccessMode.Read)
        {
            return new DbSession(driver, databaseName, accessMode);
        }

        public async Task CloseAsync()
        {
            await _session.CloseAsync();
        }

        public async void Dispose()
        {
            await CloseAsync();
        }
    }
}
