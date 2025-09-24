using System;
using System.IO;
using Machine.Specifications;
using PublicApiGenerator;
using TA.Utils.Core; // reference via a known type in the core assembly

namespace TA.Utils.Core.PublicApi.Specs.Specs
{
    [Subject("Public API")]
    public class when_generating_public_api
    {
        static string approvedPath;
        static string receivedPath;
        static string publicApi;

        Establish context = () =>
        {
            var assembly = typeof(AsyncExtensions).Assembly;
            publicApi = ApiGenerator.GeneratePublicApi(assembly);
            var baseDir = AppContext.BaseDirectory;
            approvedPath = Path.Combine(baseDir, "PublicApi.approved.txt");
            receivedPath = Path.Combine(baseDir, "PublicApi.received.txt");
        };

        Because of = () =>
        {
            if (!File.Exists(approvedPath))
            {
                File.WriteAllText(receivedPath, publicApi);
                throw new Exception("Public API approval file not found. A received file has been generated. Review and copy PublicApi.received.txt to PublicApi.approved.txt if acceptable.");
            }
        };

        It should_match_the_approved_public_api = () =>
        {
            var approved = File.ReadAllText(approvedPath);
            if (!string.Equals(approved, publicApi, StringComparison.Ordinal))
            {
                File.WriteAllText(receivedPath, publicApi);
                throw new Exception("Public API mismatch. Review diff between PublicApi.approved.txt and PublicApi.received.txt, then update approved if intentional.");
            }
        };
    }
}