namespace ArchBench.PlugIns
{
    public interface IArchBenchPlugIn
    {
        string Name        { get; }
        string Description { get; }
        string Author      { get; }
        string Version     { get; }

        IArchBenchPlugInHost Host { get; set; }

        void Initialize();
        void Dispose();
    }
}
