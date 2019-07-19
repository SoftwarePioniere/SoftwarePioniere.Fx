using System.Threading;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Projections;

namespace WebApp
{
    public class MyCompositeProjector1 : CompositeReadModelProjectorBase2
    {
        private readonly MyProjector1 _proj1;

        public override void Initialize(CancellationToken cancellationToken = new CancellationToken())
        {
            RegisterChildProjector(_proj1);
        }

        public MyCompositeProjector1(ILoggerFactory loggerFactory, IProjectorServices services
        , MyProjector1 proj1) : base(loggerFactory, services)
        {
            StreamName = "$ce-SoftwarePionierTests_Fake";
            _proj1 = proj1;
        }
    }
}