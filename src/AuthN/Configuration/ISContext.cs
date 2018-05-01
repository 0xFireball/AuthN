using AuthN.Infrastructure.Concurrency;
using AuthN.Services.Application;
using LiteDB;

namespace AuthN.Configuration {
    public interface ISContext {
        LiteDatabase database { get; }
        SConfiguration configuration { get; }
        SAppState appState { get; }
        UserServiceTable serviceTable { get; }
        SLogger log { get; }
    }
}