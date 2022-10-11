using Anno;
using System;
using System.Collections.Generic;
using System.Text;


namespace System.Threading.Tasks
{
    /// <summary>
    /// Task扩展
    /// </summary>
    public static class TaskExtensions
    {
        public static Task StartNewAnno(this TaskFactory taskFactory, Action action, TaskCreationOptions creationOptions)
        {
            var titaContext = AnnoContext.Current;
            return taskFactory.StartNew(() =>
            {
                try
                {
                    AnnoContext.Current = titaContext;
                    action();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    AnnoContext.Current = null;
                }
            }, creationOptions);
        }
        public static Task StartNewAnno(this TaskFactory taskFactory, Action action)
        {
            return taskFactory.StartNewAnno(action, TaskCreationOptions.None);
        }


        public static Task<TResult> StartNewAnno<TResult>(this TaskFactory taskFactory, Func<TResult> function, TaskCreationOptions creationOptions)
        {
            var titaContext = AnnoContext.Current;
            return taskFactory.StartNew(() =>
            {
                TResult result;
                try
                {
                    AnnoContext.Current = titaContext;
                    result = function();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    AnnoContext.Current = null;
                }
                return result;
            }, creationOptions);
        }
        public static Task<TResult> StartNewAnno<TResult>(this TaskFactory taskFactory, Func<TResult> function)
        {
            return taskFactory.StartNewAnno(function, TaskCreationOptions.None);
        }
    }
}
