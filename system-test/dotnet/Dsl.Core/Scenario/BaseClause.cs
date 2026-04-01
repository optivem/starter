using Optivem.Testing;

namespace Dsl.Core.Scenario
{
    public class BaseClause
    {
        internal Channel? Channel { get; }

        public BaseClause(Channel? channel)
        {
            Channel = channel;
        }
    }
}

