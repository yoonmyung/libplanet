using System;
using System.IO;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Store;
using Libplanet.Tests.Blockchain;
using Libplanet.Tests.Common.Action;
using Libplanet.Tests.Store;
using Libplanet.Tests.Store.Trie;
using Xunit;
using Xunit.Abstractions;

namespace Libplanet.RocksDBStore.Tests
{
    public class RocksDBStoreTest : StoreTest, IDisposable
    {
        private readonly RocksDBStoreFixture _fx;

        public RocksDBStoreTest(ITestOutputHelper testOutputHelper)
        {
            try
            {
                TestOutputHelper = testOutputHelper;
                Fx = _fx = new RocksDBStoreFixture();
                FxConstructor = () => new RocksDBStoreFixture();
            }
            catch (TypeInitializationException)
            {
                throw new SkipException("RocksDB is not available.");
            }
        }

        protected override ITestOutputHelper TestOutputHelper { get; }

        protected override StoreFixture Fx { get; }

        protected override Func<StoreFixture> FxConstructor { get; }

        public void Dispose()
        {
            _fx?.Dispose();
        }

        [SkippableFact]
        public void ReopenStoreAfterDispose()
        {
            var path = Path.Combine(Path.GetTempPath(), $"rocksdb_test_{Guid.NewGuid()}");

            try
            {
                var store = new RocksDBStore(path);
                var stateStore =
                    new TrieStateStore(new MemoryKeyValueStore(), new MemoryKeyValueStore());
                var blocks = new BlockChain<DumbAction>(
                    new NullPolicy<DumbAction>(),
                    new VolatileStagePolicy<DumbAction>(),
                    store,
                    stateStore,
                    Fx.GenesisBlock
                );
                store.Dispose();

                store = new RocksDBStore(path);
                store.Dispose();
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }
    }
}
