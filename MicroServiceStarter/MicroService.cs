using System.Collections.Generic;
using Newtonsoft.Json;

namespace MicroServiceStarter
{
    public class MicroService
    {
        [JsonProperty("gitRepo")]
        public string GitRepo { get; set; }

        [JsonProperty("gitRepoBranch")]
        public string GitRepoBranch { get; set; }

        [JsonProperty("projectPath")]
        public string ProjectPath { get; set; }

        [JsonProperty("aspNetCoreEnvironment")]
        public string AspNetCoreEnvironment { get; set; }

        [JsonProperty("configOverrides")]
        public List<ConfigOverride> ConfigOverrides { get; set; }

        [JsonIgnore]
        public string RepoFolder { get; set; }

        [JsonIgnore]
        public string ClonePath { get; set; }
    }
}
