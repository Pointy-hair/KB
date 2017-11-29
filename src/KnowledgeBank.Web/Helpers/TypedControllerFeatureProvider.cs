using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace KnowledgeBank.Web.Helpers
{
	public class TypedControllerFeatureProvider<TController> : ControllerFeatureProvider
	{
		protected override bool IsController(TypeInfo typeInfo) =>
			typeof(TController).GetTypeInfo().IsAssignableFrom(typeInfo) && base.IsController(typeInfo);
	}
}
