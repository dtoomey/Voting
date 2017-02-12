using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace VotingService
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    internal sealed class ServiceRequestActionFilterAttribute : ActionFilterAttribute
    {
        private string activityId;
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart(actionContext.ActionDescriptor.ActionName, activityId);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            ServiceEventSource.Current.ServiceRequestStop(actionExecutedContext.ActionContext.ActionDescriptor.ActionName, activityId); //,
                //actionExecutedContext.Exception?.ToString() ?? string.Empty);
        }
    }
}
