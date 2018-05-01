using AuthN.Configuration;
using AuthN.Models;
using AuthN.Utilities;

namespace AuthN.Modules.Meta {
    public class MetadataModule : SBaseModule {
        public MetadataModule(ISContext serverContext) : base("/meta", serverContext) {
            Get("/", _ => Response.asJsonNet(new {
                version = SContext.version
            }));
        }
    }
}