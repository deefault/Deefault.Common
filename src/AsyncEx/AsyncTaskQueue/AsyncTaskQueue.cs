using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Deefault.AsyncEx.AsyncTaskQueue
{
    public class AsyncTaskQueue : IDisposable
    {
        private readonly int _poolSize;
        private readonly SemaphoreSlim _lock;

        public int PoolSize => _poolSize;

        public bool IsFull => _lock.CurrentCount == 0;
        public int AvailableWorkers => _lock.CurrentCount;

        public AsyncTaskQueue() : this(1)
        {
        }
        
        public AsyncTaskQueue(int poolSize = 1)
        {
            _poolSize = poolSize;
            _lock = new SemaphoreSlim(_poolSize);
        }
        
        #region Task

        [DebuggerStepThrough]
        public Task EnqueueAsync(Func<Task> taskFunc)
            => InternalEnqueueAsync(taskFunc, CancellationToken.None);
        
        [DebuggerStepThrough]
        public Task EnqueueAsync(Func<Task> taskFunc, CancellationToken ct)
            => InternalEnqueueAsync(taskFunc, ct);
        
        private async Task InternalEnqueueAsync(Func<Task> taskFunc, CancellationToken ct)
        {
            await _lock.WaitAsync(ct);
            
            try
            {
                var task = taskFunc.Invoke();
                await task;
            }
            finally
            {
                _lock.Release();
            }
        }

        #endregion
        
        #region Task<T>

        [DebuggerStepThrough]
        public Task<T> EnqueueAsync<T>(Func<Task<T>> taskFunc)
            => InternalEnqueueAsync(taskFunc, CancellationToken.None);
        
        [DebuggerStepThrough]
        public Task<T> EnqueueAsync<T>(Func<Task<T>> taskFunc, CancellationToken ct)
            => InternalEnqueueAsync(taskFunc, ct);
        
        private async Task<T> InternalEnqueueAsync<T>(Func<Task<T>> taskFunc, CancellationToken ct)
        {
            await _lock.WaitAsync(ct);
            
            try
            {
                var task = taskFunc.Invoke();
                return await task;
            }
            finally
            {
                _lock.Release();
            }
        }

        #endregion

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}