using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace Basic
{
	// Binder with MVC semantics. Treat the body as KeyValue pairs and model bind it.
	/// <summary>
	/// Mvc action value binder.
	/// </summary>
	public class MvcActionValueBinder : DefaultActionValueBinder
	{
		// Per-request storage, uses the Request.Properties bag. We need a unique key into the bag.
		private const string Key = "5DC187FB-BFA0-462A-AB93-9E8036871EC8";

		/// <summary>
		/// Gets the binding.
		/// </summary>
		/// <returns>The binding.</returns>
		/// <param name="actionDescriptor">Action descriptor.</param>
		public override HttpActionBinding GetBinding (HttpActionDescriptor actionDescriptor)
		{

			HttpActionBinding actionBinding = new HttpActionBinding ();

			HttpParameterDescriptor[] parameters = actionDescriptor.GetParameters ().ToArray ();
			HttpParameterBinding[] binders = Array.ConvertAll (parameters, p => DetermineBinding (actionBinding, p));

			actionBinding.ParameterBindings = binders;
			return actionBinding;            
		}

		private HttpParameterBinding DetermineBinding (HttpActionBinding actionBinding, HttpParameterDescriptor parameter)
		{
			HttpConfiguration config = parameter.Configuration;

			var attr = new ModelBinderAttribute(); // use default settings

			ModelBinderProvider provider = attr.GetModelBinderProvider(config);

			// Alternatively, we could put this ValueProviderFactory in the global config.
			List<ValueProviderFactory> vpfs = new List<ValueProviderFactory>(attr.GetValueProviderFactories(config));
			vpfs.Add(new BodyValueProviderFactory());
			//vpfs.Add (new RouteDataValueProviderFactory ());
			return new ModelBinderParameterBinding(parameter, provider, vpfs);
		}

		// Derive from ActionBinding so that we have a chance to read the body once and then share that with all the parameters.
		private class MvcActionBinding : HttpActionBinding
		{
			// Read the body upfront , add as a ValueProvider
			public override Task ExecuteBindingAsync (HttpActionContext actionContext, CancellationToken cancellationToken)
			{
				HttpRequestMessage request = actionContext.ControllerContext.Request;
				HttpContent content = request.Content;
				if (content != null) {
					// content.ReadAsStreamAsync ().Result;
					FormDataCollection fd = content.ReadAsAsync<FormDataCollection> ().Result;
					if (fd != null) {

						NameValueCollection nvc = fd.ReadAsNameValueCollection ();

						System.Web.Http.ValueProviders.IValueProvider vp = new System.Web.Http.ValueProviders.Providers.NameValueCollectionValueProvider (nvc, CultureInfo.InvariantCulture);

						request.Properties.Add (Key, vp);
					}
				}

				return base.ExecuteBindingAsync (actionContext, cancellationToken);
			}
		}
		// Get a value provider over the body. This can be shared by all parameters.
		// This gets the values computed in MvcActionBinding.
		private class BodyValueProviderFactory : System.Web.Http.ValueProviders.ValueProviderFactory
		{
			public override System.Web.Http.ValueProviders.IValueProvider GetValueProvider (HttpActionContext actionContext)
			{
				object vp;
				actionContext.Request.Properties.TryGetValue (Key, out vp);
				return (System.Web.Http.ValueProviders.IValueProvider)vp; // can be null                
			}
		}
	}
}


