namespace SmartPalm.DeviceService.API.Tests.Integration;

public class DockerRequiredFactAttribute : FactAttribute
{
    public DockerRequiredFactAttribute()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("SKIP_DOCKER_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
            Skip = "Docker integration tests were disabled with SKIP_DOCKER_TESTS=true.";
    }
}
