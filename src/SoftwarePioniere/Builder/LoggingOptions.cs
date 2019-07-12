namespace SoftwarePioniere.Builder
{

    public class LoggingOptions
    {
        public bool UseSeq { get; set; }

        public string SeqServerUrl { get; set; } = "http://localhost:5341";

        public string MinimumLevel { get; set; } = "Information";

        public string TraceSources { get; set; }

        public string DebugSources { get; set; }

        public string InformationSources { get; set; }

        public string WarningSources { get; set; }

        public string LogDir { get; set; } = "logs";

        public string Template { get; set; } = "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{SourceContext}] {Message} {Exception}{NewLine}";
    }
}
