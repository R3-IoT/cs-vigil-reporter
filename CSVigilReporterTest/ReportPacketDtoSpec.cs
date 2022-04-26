using CSVigilReporter.Dto;
using System.Text.Json;
using NUnit.Framework;

namespace CSVigilReporterTest;

[TestFixture]
public class ReportPacketDtoSpec
{
    [Test]
    public void ReportPacket_WhenConvertedToJson_HasCorrectFormat()
    {
        var replicaId = "test";
        var interval = 60;
        var cpu = 0.5f;
        var ram = 0.5f;
        var packet = new ReportPacketDto
        {
            Replica = replicaId,
            Interval = interval,
            Load = new ReportLoadDto
            {
                Cpu = cpu,
                Ram = ram
            }
        };

        var jsonString = JsonSerializer.Serialize(packet).ToLower();

        var expectedString = "{\"replica\":\"test\",\"interval\":60,\"load\":{\"cpu\":0.5,\"ram\":0.5}}";
        
        Assert.AreEqual(expectedString, jsonString);
    }
}