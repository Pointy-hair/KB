using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace KnowledgeBank.Web.Helpers
{
	public class BranchAttribute : Attribute, IResourceFilter
	{
		private readonly string _branch;

		public BranchAttribute(string branch)
		{
			_branch = branch;
		}

		public void OnResourceExecuting(ResourceExecutingContext context)
		{
			var pathSplit = context.HttpContext.Request.PathBase.Value.Split('/');
			var requestBasePath = pathSplit.Last();

			if(requestBasePath.Equals(_branch, StringComparison.InvariantCultureIgnoreCase))
				context.Result = new NotFoundResult();
		}

		public void OnResourceExecuted(ResourceExecutedContext context)
		{
		}
	}
}
