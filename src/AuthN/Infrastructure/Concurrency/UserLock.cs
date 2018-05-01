using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthN.Infrastructure.Concurrency {
    public class UserLock {
        private readonly AutoResetEvent _readFree = new AutoResetEvent(true);
        private readonly AutoResetEvent _writeFree = new AutoResetEvent(true);

        public void obtainExclusiveWrite() {
            // Wait for exclusive read and write
            _writeFree.WaitOne();
            _readFree.WaitOne();
        }

        public async Task obtainExclusiveWriteAsync() {
            await Task.Run(() => obtainExclusiveWrite());
        }

        public void releaseExclusiveWrite() {
            // Release exclusive read and write
            _writeFree.Set();
            _readFree.Set();
        }

        public void obtainExclusiveRead() {
            _readFree.WaitOne();
        }

        public async Task obtainExclusiveReadAsync() {
            await Task.Run(() => obtainExclusiveRead());
        }

        public void releaseExclusiveRead() {
            _readFree.Set();
        }

        public void obtainConcurrentRead() {
            // Lock writing, and allow multiple concurrent reads
            _writeFree.WaitOne();
        }

        public async Task obtainConcurrentReadAsync() {
            await Task.Run(() => obtainConcurrentRead());
        }

        public void releaseConcurrentRead() {
            // Allow writing again
            _writeFree.Set();
        }

        public async Task withExclusiveWriteAsync(Task action) {
            await obtainExclusiveWriteAsync();
            await action;
            releaseExclusiveWrite();
        }

        public async Task withExclusiveWriteAsync(Func<Task> action) {
            await obtainExclusiveWriteAsync();
            await action();
            releaseExclusiveWrite();
        }

        public async Task withExclusiveReadAsync(Task action) {
            await obtainExclusiveReadAsync();
            await action;
            releaseExclusiveRead();
        }

        public async Task withExclusiveReadAsync(Func<Task> action) {
            await obtainExclusiveReadAsync();
            await action();
            releaseExclusiveRead();
        }

        public async Task withConcurrentReadAsync(Task action) {
            await obtainConcurrentReadAsync();
            await action;
            releaseConcurrentRead();
        }

        public async Task withConcurrentReadAsync(Func<Task> action) {
            await obtainConcurrentReadAsync();
            await action();
            releaseConcurrentRead();
        }
    }
}