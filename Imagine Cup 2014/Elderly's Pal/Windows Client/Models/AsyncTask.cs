/*
 * C# AsyncTask inspired from Java:
 * The idea is base on TPL (Task parallel library).
 * The purpose of async task is to run a task that is
 * independent from the UI
 * 
 * There are 3 base functions:
 *      onUpdate: Update the UI while process is running
 *      onPreExecute: Run before the task
 *      doInBackground: Run async task and return the result
 *      onPostExecute: Handle the result given by async task
 * 
 * Author: Nguyen Minh Tu
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceDreamers.Windows.Models
{
    public class AsyncTask<Tin, Tgo, Tout> where Tgo : struct
    {
        private readonly AsyncTaskCallBack callback;
        private readonly CancellationToken cancelToken;
        private readonly SynchronizationContext syncContext;
        private readonly TaskCreationOptions taskCreateOptions;

        /// Called upon an unhandled exception. If return true, exception is propagted up the stack.
        public Func<Exception, bool> Err;

        /// The code inside this function is executed on ananother thread.
        public Func<Tin, AsyncTaskCallBack, Tout> doInBackground;

        /// Post processing any result
        public Action<Tout> onPostExecute;

        /// This function is executed before anything.
        public Func<bool> onPreExecute;

        /// Do your GUI progress control manipulation here
        public Action<Tgo> onUpdate;

        private Task<Tout> task;

        public AsyncTask(CancellationToken ct)
            : this()
        {
            cancelToken = ct;
        }

        public AsyncTask(TaskCreationOptions tco = TaskCreationOptions.None)
        {
            //make it optional
            onPreExecute = delegate { return true; };
            Err = delegate { return true; };

            callback = new AsyncTaskCallBack(this);
            taskCreateOptions = tco;
            syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public async void Execute(Tin input)
        {
            if (onPreExecute())
            {
                Tout result = default(Tout);

                try
                {
                    result = await RunTaskAsync(input);

                    onPostExecute(result);
                }
                catch (OperationCanceledException oce)
                {
                    if (Err(oce))
                        throw;
                }
                catch (AggregateException aex)
                {
                    if (Err(aex.Flatten()))
                        throw;
                }
                catch (Exception ex)
                {
                    if (Err(ex))
                        throw;
                }
            }
        }

        private Task<Tout> RunTaskAsync(Tin input)
        {
            task = Task.Factory.StartNew(() => doInBackground(input, callback),
                cancelToken,
                taskCreateOptions,
                TaskScheduler.Default);

            return task;
        }

        /// Winforms you need to use Control.Invoke Method (Delegate)
        /// to make sure that control is updated in the UI thread.
        private static void PerformInvoke(Control ctrl, Action action)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke(action);
            else
                action();
        }

        public class AsyncTaskCallBack : IProgress<Tgo>
        {
            // Tin: Task input type
            // Tgo: Task process type
            // Tout: Task output type
            private readonly AsyncTask<Tin, Tgo, Tout> _asyncTask;

            public AsyncTaskCallBack(AsyncTask<Tin, Tgo, Tout> at)
            {
                _asyncTask = at;
            }

            public bool IsCancellationRequested
            {
                get { return _asyncTask.cancelToken.IsCancellationRequested; }
            }

            public void Report(Tgo value)
            {
                //make sure we are on the caller thread
                _asyncTask.syncContext.Post(o => _asyncTask.onUpdate(value), null);
            }

            public void ThrowIfCancel()
            {
                _asyncTask.cancelToken.ThrowIfCancellationRequested();
            }
        }
    }
}