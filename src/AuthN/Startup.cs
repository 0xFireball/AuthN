using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AuthN.Configuration;
using AuthN.Services.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.Owin;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace AuthN {
    public class Startup {
        private const string config_file_name = "authn.json";
        private const string state_storage_database_file_name = "authn_state.lidb";
        private readonly IConfigurationRoot _fileConfig;

        public Startup(IHostingEnvironment env) {
            if (!File.Exists(config_file_name)) {
                try {
                    // Create config file
                    Console.WriteLine($"Configuration file {config_file_name} does not exist, creating default.");
                    var confFileContent = JsonConvert.SerializeObject(new SConfiguration(), Formatting.Indented);
                    File.WriteAllText(config_file_name, confFileContent);
                } catch (Exception ex) {
                    Console.WriteLine($"Could not write to {config_file_name}: {ex}");
                }
            }

            var builder = new ConfigurationBuilder()
                .AddJsonFile(config_file_name,
                    optional: true,
                    reloadOnChange: true)
                .SetBasePath(env.ContentRootPath);

            _fileConfig = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        // ReSharper disable once InconsistentNaming
        public void ConfigureServices(IServiceCollection services) {
            // Adds services required for using options.
            services.AddOptions();

            // Register IConfiguration
            services.Configure<SConfiguration>(_fileConfig);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ReSharper disable once InconsistentNaming
        public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime,
            IHostingEnvironment env, ILoggerFactory loggerFactory) {
            // create default configuration
            var serverConfig = new SConfiguration();
            // bind configuration
            _fileConfig.Bind(serverConfig);

            // build context
            var context = SConfigurator.createContext(serverConfig);

            // import crypto keys
            var privateKeySource = context.configuration.privateKey;
            if (string.IsNullOrEmpty(privateKeySource)) {
                // generate crypto keys
                var defaultKeySize = 2048;
                context.log.writeLine($"Private key not provided, generating new keypair of size {defaultKeySize}",
                    SLogger.LogLevel.Information);
                var gen = new RsaKeyPairGenerator();
                gen.Init(new KeyGenerationParameters(new SecureRandom(), defaultKeySize));
                var keyPair = gen.GenerateKeyPair();
                var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
                var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
                privateKeySource = Convert.ToBase64String(privateKeyInfo.ToAsn1Object().GetDerEncoded());
                var encodedPublicKey = Convert.ToBase64String(publicKeyInfo.ToAsn1Object().GetDerEncoded());
                context.log.writeLine($"private key:\n{privateKeySource}", SLogger.LogLevel.Information);
                context.log.writeLine($"public key:\n{encodedPublicKey}", SLogger.LogLevel.Information);
            }

            // import key pair
            var privateKey =
                (RsaPrivateCrtKeyParameters) PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKeySource));
            context.configuration.crypto = DotNetUtilities.ToRSA(privateKey);
            context.log.writeLine($"Imported private key from configuration", SLogger.LogLevel.Information);

            context.log.writeLine("Server context created", SLogger.LogLevel.Information);

            // load persistent state
            SConfigurator.loadState(context, state_storage_database_file_name);
            context.log.writeLine($"Persistent state loaded from {state_storage_database_file_name}",
                SLogger.LogLevel.Information);

            // load database
            context.connectDatabase();
            context.log.writeLine($"Database connected", SLogger.LogLevel.Information);

            // register application stop handler
            // AssemblyLoadContext.Default.Unloading += (c) => OnUnload(context);
            applicationLifetime.ApplicationStopping.Register(() => onUnload(context));
            context.log.writeLine($"Application interrupt handler registered", SLogger.LogLevel.Information);

            // add aspnet developer exception page
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // add aspnet logger
            if (env.IsDevelopment() && serverConfig.aspnetVerboseLogging) {
                loggerFactory.AddConsole(LogLevel.Information);
            } else {
                loggerFactory.AddConsole(LogLevel.Warning);
            }

            // add wwwroot/
            app.UseStaticFiles();

            // set up Nancy OWIN hosting
            app.UseOwin(x => x.UseNancy(options => {
                options.PassThroughWhenStatusCodesAre(
                    HttpStatusCode.NotFound,
                    HttpStatusCode.InternalServerError
                );
                options.Bootstrapper = new AppBootstrapper(context);
            }));

            context.log.writeLine($"Web services mapped successfully", SLogger.LogLevel.Information);
        }

        private void onUnload(ISContext sctx) {
            sctx.log.writeLine("Server unloading, force-persisting state data.", SLogger.LogLevel.Information);
            // persist on unload
            sctx.appState.persist(true);
        }
    }
}