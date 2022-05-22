/*
Parts of the code, especially the scope eval is copied
from https://www.meziantou.net/asp-net-core-json-logger.htm
*/

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace janzi.Logging;

public abstract class BackgroundWorkerLoggerProvider: ILoggerProvider, IDisposable
{
    private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
    private readonly BlockingCollection<JsonLogEntry> LogActions = new BlockingCollection<JsonLogEntry>();
    private Task workerTask;
    private bool disposedValue;
    protected bool Disposed => disposedValue;
    public BackgroundWorkerLoggerProvider()
    {
        TaskFactory tf = new TaskFactory(cancellation.Token, TaskCreationOptions.LongRunning, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
        workerTask = tf.StartNew(ProcessQueue);
    }
    protected virtual async Task ProcessQueue()
    {
        foreach (var content in LogActions.GetConsumingEnumerable(cancellation.Token))
        {
            await ProcessQueueItem(content);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        cancellation.Cancel();
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                this.workerTask = null;//kill the reference to the task
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~BackgroundWorkerLoggerProvider()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    public void Enqueue(JsonLogEntry entry)
        => this.LogActions.Add(entry);
    public abstract ILogger CreateLogger(string categoryName);
    protected abstract Task ProcessQueueItem(JsonLogEntry content);
}
