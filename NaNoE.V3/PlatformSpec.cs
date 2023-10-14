namespace NaNoE.V3;

public enum PlatformType
{
    Mobile,
    Desktop
}

public static class PlatformSpec
{
    public static PlatformType Type { get; set; }
}
