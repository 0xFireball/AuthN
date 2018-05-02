using System.Linq;
using System.Security.Claims;
using AuthN.Configuration;
using AuthN.Services.Auth.Crypto;
using Nancy;
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

            // Authorization token
            pipelines.BeforeRequest.AddItemToEndOfPipeline(ctx => {
                var authToken = ctx.Request.Headers.Authorization;
                if (string.IsNullOrEmpty(authToken)) return null;

                try {
                    var claims = new WebTokenBuilder()
                        .withAlgorithm(new RS384Algorithm(serverContext.configuration.crypto))
                        .mustVerify()
                        .decode(authToken);

                    ctx.CurrentUser =
                        new ClaimsPrincipal(
                            new ClaimsIdentity(claims.Select(x => new Claim(x.Key, x.Value.ToString())))
                        );

                    return null;
                } catch {
                    return null;
                }
            });

            // Enable CORS
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx => {
                foreach (var origin in serverContext.configuration.corsOrigins) {
                    ctx.Response.WithHeader("Access-Control-Allow-Origin", origin);
                }

                ctx.Response
                    .WithHeader("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE")
                    .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type, Authorization");
            });

            // Set configuration...
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container) {
            base.ConfigureApplicationContainer(container);

            // Register IoC components
            container.Register<ISContext>(serverContext);
        }
    }
}