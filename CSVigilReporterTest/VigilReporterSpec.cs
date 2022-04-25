using CSVigilReporter;
using Moq;
using NUnit.Framework;

namespace CSVigilReporterTest;

[TestFixture]
public class VigilReporterSpec
{

    [Test]
    public void PostReport_WhenCalled_ThenItUsesTheHttpClientToSendAPostToTheEndpoint()
    {
        var url = "";
        var secretToken = "";
        var probeId = "";
        var nodeId = "";
        var replicaId = "";
        var interval = 2;
        var reporter = new VigilReporter(
            url, 
            secretToken, 
            probeId, 
            nodeId, 
            replicaId, 
            interval);
    }
}