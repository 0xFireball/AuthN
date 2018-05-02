using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using AuthN.Nfx;
using AuthN.Nfx.WebTokens;
using FederationDemo.Modules;
using Microsoft.Extensions.Configuration;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Diagnostics;
using Nancy.TinyIoc;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace FederationDemo {
    public class AppBootstrapper : DefaultNancyBootstrapper {
        public AppBootstrapper(IConfiguration config) {
            this.config = config;
            this.RegisterModules(this.ApplicationContainer, new[] {
                new ModuleRegistration(typeof(FederationShowModule))
            });
        }

        public IConfiguration config { get; }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines) {
            base.ApplicationStartup(container, pipelines);

            var authSources = config.GetSection("authSources").Get<IDictionary<string, string>>();
            var publicKeys = new List<RSA>();
            foreach (var authSource in authSources) {
                var publicKey =
                    (RsaKeyParameters) PublicKeyFactory.CreateKey(Convert.FromBase64String(authSource.Key));
                publicKeys.Add(DotNetUtilities.ToRSA(publicKey));
            }

            AuthenticationHook.install(pipelines, new RS384Algorithm(publicKeys[0]));
        }

        public override void Configure(INancyEnvironment environment) {
            base.Configure(environment);
            
            environment.Diagnostics(true, "password");
        }
    }
}