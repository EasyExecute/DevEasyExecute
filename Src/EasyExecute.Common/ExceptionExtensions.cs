using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyExecute.Common
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<string> GetMessages(this Exception ex)
        {
            if (ex == null) { yield break; }
            yield return ex.Message;
            var innerExceptions = Enumerable.Empty<Exception>();
            var exception = ex as AggregateException;
            if (exception != null && exception.InnerExceptions.Any())
            {
                innerExceptions = exception.InnerExceptions;
            }
            else if (ex.InnerException != null)
            {
                innerExceptions = new[] { ex.InnerException };
            }

            foreach (var msg in innerExceptions.SelectMany(innerEx => innerEx.GetMessages()))
            {
                yield return msg;
            }
        }
    }
}