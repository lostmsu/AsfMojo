namespace AsfMojo.Media
{
    /// <summary>
    /// Fluent interface to create an image from an ASF stream
    /// </summary>
    public interface IAsfImageProperties
    {
        string FileName { get; set; }
        double Offset { get; set; }
    }

    /// <summary>
    /// Fluent interface to create an image from an ASF stream
    /// </summary>
    internal class AsfImageProperties : IAsfImageProperties
    {
        public string FileName { get; set; }
        public double Offset { get; set; }
    }
}
