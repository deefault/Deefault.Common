using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deefault.AsyncEx.AsyncTaskQueue;
using Xunit;
// ReSharper disable ConvertToLocalFunction
// ReSharper disable UnusedVariable

namespace AsyncEx.Tests
{
    internal class FakeType
    {
        public int Data { get; set;  }
    }
    
    public class UnitTest1
    {
        [Fact]
        public void Default_Pool_Size_Should_Eq_1()
        {
            var queue = new AsyncTaskQueue();

            var actual = queue.PoolSize;
            
            Assert.Equal(1, actual);
        }
        
        [Fact]
        public void Single_Task_Should_Await()
        {
            var queue = new AsyncTaskQueue();
            Func<Task> taskFunc = () => Task.CompletedTask;

            var completedTask = queue.EnqueueAsync(taskFunc);
            
            Assert.True(completedTask.IsCompleted);
        }
        
        [Fact]
        public void Single_Task_Should_Await_In_Using_Block()
        {
            using (var queue = new AsyncTaskQueue())
            {
                Func<Task> taskFunc = () => Task.CompletedTask;

                var completedTask = queue.EnqueueAsync(taskFunc);

                Assert.True(completedTask.IsCompleted);
            }
        }
        
        [Fact]
        public async Task Single_TaskOfT_Should_Await()
        {
            var queue = new AsyncTaskQueue();
            Func<Task<FakeType>> taskFunc = () => Task.FromResult(new FakeType(){Data = 42});

            var completedTask = await queue.EnqueueAsync(taskFunc);
            
            Assert.Equal(42, completedTask.Data);
        }
        
        [Fact]
        public void Test_IsFull()
        {
            var queue = new AsyncTaskQueue(2);
            var tcsArray = new TaskCompletionSource[]
            {
                new TaskCompletionSource(),
                new TaskCompletionSource(),
                new TaskCompletionSource(),
            };
            List<Func<Task>> taskFuncs = tcsArray
                .Select<TaskCompletionSource, Func<Task>>(x => async () => await x.Task)
                .ToList();
            
            var task1 = queue.EnqueueAsync(taskFuncs[0]);
            Assert.False(queue.IsFull);
            
            var task2 = queue.EnqueueAsync(taskFuncs[1]);
            Assert.True(queue.IsFull);
        }
        
        [Fact]
        public void Test_AvailableWorkers()
        {
            var queue = new AsyncTaskQueue(3);
            var tcsArray = new TaskCompletionSource[]
            {
                new TaskCompletionSource(),
                new TaskCompletionSource(),
                new TaskCompletionSource(),
            };
            List<Func<Task>> taskFuncs = tcsArray
                .Select<TaskCompletionSource, Func<Task>>(x => async () => await x.Task)
                .ToList();

            var task1 = queue.EnqueueAsync(taskFuncs[0]);
            
            Assert.Equal(2, queue.AvailableWorkers);
            
            var task2 = queue.EnqueueAsync(taskFuncs[1]);
            
            Assert.Equal(1, queue.AvailableWorkers);
            
            var task3 = queue.EnqueueAsync(taskFuncs[2]);
            
            Assert.Equal(0, queue.AvailableWorkers);
        }
        
        [Fact]
        public async Task Should_Complete_Task_After_Pool_Availability()
        {
            // Arrange
            var queue = new AsyncTaskQueue(2);
            var tcsArray = new TaskCompletionSource[]
            {
                new TaskCompletionSource(),
                new TaskCompletionSource(),
                new TaskCompletionSource(),
            };
            tcsArray[2].SetResult();

            List<Func<Task>> taskFuncs = tcsArray
                .Select<TaskCompletionSource, Func<Task>>(x => () => x.Task)
                .ToList();
            
            // Act
            var task1 = queue.EnqueueAsync(taskFuncs[0]);
            var task2 = queue.EnqueueAsync(taskFuncs[1]);
            var task3 = queue.EnqueueAsync(taskFuncs[2]);

            // Assert
            Assert.False(task3.IsCompleted);
            Assert.Equal(TaskStatus.WaitingForActivation, task3.Status);
            
            tcsArray[0].SetResult();
            await task1;
            await task3;
            Assert.True(task3.IsCompleted);
        }
        
        [Fact]
        public async Task Should_Complete_TaskOfT_After_Pool_Availability()
        {
            // Arrange
            var queue = new AsyncTaskQueue(2);
            var tcsArray = new TaskCompletionSource<FakeType>[]
            {
                new TaskCompletionSource<FakeType>(),
                new TaskCompletionSource<FakeType>(),
                new TaskCompletionSource<FakeType>(),
            };
            tcsArray[2].SetResult(new FakeType(){Data = 3});

            List<Func<Task>> taskFuncs = tcsArray
                .Select<TaskCompletionSource<FakeType>, Func<Task>>(x => () => x.Task)
                .ToList();
            
            // Act
            var task1 = queue.EnqueueAsync(taskFuncs[0]);
            var task2 = queue.EnqueueAsync(taskFuncs[1]);
            var task3 = queue.EnqueueAsync(taskFuncs[2]);

            // Assert
            Assert.False(task3.IsCompleted);
            Assert.Equal(TaskStatus.WaitingForActivation, task3.Status);
            
            tcsArray[0].SetResult(new FakeType() {Data = 1});
            await task1;
            await task3;
            Assert.True(task3.IsCompleted);
        }


        [Fact]
        public async Task Should_Wait_For_Lock_Release()
        {
            // Arrange
            var queue = new AsyncTaskQueue(2);
            var tcsArray = new TaskCompletionSource[]
            {
                new TaskCompletionSource(),
                new TaskCompletionSource(),
                new TaskCompletionSource(),
            };
            tcsArray[2].SetResult();

            List<Func<Task>> taskFuncs = tcsArray
                .Select<TaskCompletionSource, Func<Task>>(x => () => x.Task)
                .ToList();
            
            // Act
            var task1 = queue.EnqueueAsync(taskFuncs[0]);
            var task2 = queue.EnqueueAsync(taskFuncs[1]);
            var delay = new CancellationTokenSource(1000);
            var task3 = queue.EnqueueAsync(taskFuncs[2], delay.Token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await task3;
            });
        }

        [Fact]
        public async Task Should_Wait_For_Lock_Release_TaskOfT()
        {
            // Arrange
            var queue = new AsyncTaskQueue(2);
            var tcsArray = new TaskCompletionSource<FakeType>[]
            {
                new TaskCompletionSource<FakeType>(),
                new TaskCompletionSource<FakeType>(),
                new TaskCompletionSource<FakeType>(),
            };
            tcsArray[2].SetResult(new FakeType(){Data = 3});

            List<Func<Task>> taskFuncs = tcsArray
                .Select<TaskCompletionSource<FakeType>, Func<Task>>(x => () => x.Task)
                .ToList();
            
            // Act
            var task1 = queue.EnqueueAsync(taskFuncs[0]);
            var task2 = queue.EnqueueAsync(taskFuncs[1]);
            var delay = new CancellationTokenSource(1000);
            var task3 = queue.EnqueueAsync(taskFuncs[2], delay.Token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await task3;
            });
        }
    }
}