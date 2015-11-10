using System.Diagnostics;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Newtonsoft.Json.Converters;
using Owin;
using Swashbuckle.Application;

namespace RestPlugin
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class OwinWebApiConfig
    {
        internal static IDependencyResolver DependencyResolver;

        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();

            config.EnableCors();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            // we don't want no XML, just JSON, cf.:
            // http://codebetter.com/glennblock/2012/02/26/disabling-the-xml-formatter-in-asp-net-web-apithe-easy-way-2/
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // cf.: https://stackoverflow.com/questions/20242035/mediatypeformatter-serialize-enum-string-values-in-web-api/21193435#21193435
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

            if (DependencyResolver != null)
                config.DependencyResolver = DependencyResolver;

            // add swagger/swashbuckle, cf.: https://github.com/domaindrivendev/Swashbuckle
            config
                .EnableSwagger(c => c.SingleApiVersion("v1", "NuPlug REST Api sample"))
                .EnableSwaggerUi();

            // NOTE: not needed - cf.: http://docs.autofac.org/en/latest/integration/webapi.html#owin-integration
            //appBuilder.UseAutofacMiddleware(_container);
            //appBuilder.UseAutofacWebApi(config);

            appBuilder.UseWebApi(config);
        }
    }
}