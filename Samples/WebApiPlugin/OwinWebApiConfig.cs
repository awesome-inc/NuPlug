using System.Web.Http;
using System.Web.Http.Dependencies;
using Newtonsoft.Json.Converters;
using Owin;
using Swashbuckle.Application;

namespace WebApiPlugin
{
    public class OwinWebApiConfig
    {
        public static IDependencyResolver DependencyResolver { get; set; }

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
                .EnableSwagger(c => c.SingleApiVersion("v1", "A title for your API"))
                .EnableSwaggerUi();

            appBuilder.UseWebApi(config);
        }
    }
}