using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthN.Infrastructure.Concurrency {
    public class ResourceThrottle {
        private readonly Semaphore _throttle;

        public ResourceThrottle(int maxConcurrent) {
            _throttle = new Semaphore(maxConcurrent, maxConcurrent);
        }

        public void acquire() {
            _throttle.WaitOne();
        }

        public async Task acquireAsync() {
            await Task.Run(() => acquire());
        }

        public void release() {
            _throttle.Release();
        }

        public async Task withResourceAsync(Func<Task> action) {
            await acquireAsync();
            await action();
            release();
        }
    }
}