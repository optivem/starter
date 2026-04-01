using Driver.Adapter;
using Optivem.Testing;

namespace Dsl.Core.Scenario
{
    public class ScenarioDslFactory
    {
        private readonly UseCaseDsl _app;

        public ScenarioDslFactory(UseCaseDsl app)
        {
            _app = app;
        }

        public ScenarioDsl Create(Channel channel) { return new ScenarioDsl(channel, _app); }
    }
}


