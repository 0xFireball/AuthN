using AuthN.Configuration;
using AuthN.Services.Auth;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace AuthN {
    public class AppBootstrapper : DefaultNancyBootstrapper {
        public SContext serverContext { get; }

        public AppBootstrapper(SContext context) {
            serverContext = context;
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines) {
            base.ApplicationStartup(container, pipelines);

            // Enable stateless authentication
            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx => {
                // Take API key from query string
                var apikey = (string) ctx.Request.Query.apikey.Value;
                if (apikey == null) // key wasn't in query, check alternate sources
                {
                    var authHeader = ctx.Request.Headers.Authorization;
                    if (!string.IsNullOrWhiteSpace(authHeader)) {
                        apikey = authHeader;
                    }
                }

                // Call authenticator
                var auther = new ApiAuthenticator(serverContext);
                return auther.resolveIdentity(apikey);
            }));

            // Enable CORS
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx => {
                foreach (var origin in serverContext.configuration.corsOrigins) {
                    ctx.Response.WithHeader("Access-Control-Allow-Origin", origin);
                }

                ctx.Response
                    .WithHeader("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE")
                    .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type, Authorization");
            });

            // TODO: Set configuration
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container) {
            base.ConfigureApplicationContainer(container);

            // Register IoC components
            container.Register<ISContext>(serverContext);
        }
    }
}