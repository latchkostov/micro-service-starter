using System.Collections.Generic;

namespace MicroServiceStarter
{
    public class MicroServiceDefinitions
    {
        public List<MicroService> MicroServices { get; set; }

        public List<ConfigOverride> GlobalConfigOverrides { get; set; }
    }
}
