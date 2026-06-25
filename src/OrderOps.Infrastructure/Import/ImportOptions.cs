namespace OrderOps.Infrastructure.Import;

public class ImportOptions
{
    public const string SectionName = "Import";

    public string DataPath { get; set; } = "data";
    public bool RunOnStartup { get; set; }
}
