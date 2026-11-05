
namespace SmartPalm.DeviceService.Domain.Devices;

public readonly record struct MacAddress
{
    private MacAddress(string value) => Value = value;

    public string Value { get; }

    public static MacAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("MAC address is required.", nameof(value));
        return new MacAddress(value.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;
}
