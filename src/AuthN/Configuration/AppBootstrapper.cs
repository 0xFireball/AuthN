using AuthN.Configuration;
using AuthN.Nfx;
using AuthN.Nfx.WebTokens;
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

            // Authorization token support
            AuthenticationHook.install(pipelines, new RS384Algorithm(serverContext.configuration.crypto));

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