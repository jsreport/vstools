using System;
using System.Threading;
using System.Threading.Tasks;

namespace JsReportVSTools.Impl
{
    //taken from http://stackoverflow.com/questions/15142507/deadlock-when-combining-app-domain-remoting-and-tasks

    public static class RemoteTask
    {
        public static async Task<T> ClientComplete<T>(RemoteTask<T> remoteTask,
            CancellationToken cancellationToken)
        {
            T result;

            using (cancellationToken.Register(remoteTask.Cancel))
            {
                var tcs = new RemoteTaskCompletionSource<T>();
                remoteTask.Complete(tcs);
                result = await tcs.Task;
            }

            //this is frozing things
            //await Task.Yield(); // HACK!!

            return result;
        }

        public static RemoteTask<T> ServerStart<T>(Func<CancellationToken, Task<T>> func)
        {
            return new RemoteTask<T>(func);
        }
    }

    public class RemoteTask<T> : MarshalByRefObject
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly Task<T> task;

        internal RemoteTask(Func<CancellationToken, Task<T>> starter)
        {
            task = starter(cts.Token);
        }

        internal void Complete(RemoteTaskCompletionSource<T> tcs)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    AggregateException agrException = t.Exception;
                    Exception inner = agrException.Flatten().InnerExceptions[0];

                    //exception does not need to be serializable like a JsReportException
                    try
                    {
                        tcs.TrySetException(inner);
                    }
                    catch (Exception e)
                    {
                        tcs.TrySetException(new Exception(inner.ToString()));
                    }
                }
                else if (t.IsCanceled)
                {
                    tcs.TrySetCancelled();
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        internal void Cancel()
        {
            cts.Cancel();
        }
    }

    public class RemoteTaskCompletionSource<T> : MarshalByRefObject
    {
        private readonly TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

        public Task<T> Task
        {
            get { return tcs.Task; }
        }

        public bool TrySetResult(T result)
        {
            return tcs.TrySetResult(result);
        }

        public bool TrySetCancelled()
        {
            return tcs.TrySetCanceled();
        }

        public bool TrySetException(Exception ex)
        {
            return tcs.TrySetException(ex);
        }
    }
}