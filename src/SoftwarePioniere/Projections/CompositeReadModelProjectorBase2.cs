using Microsoft.Extensions.Logging;


namespace SoftwarePioniere.Projections
{
    public abstract class CompositeReadModelProjectorBase2 : CompositeReadModelProjectorBase
    {
        protected readonly IProjectorServices Services;

        protected CompositeReadModelProjectorBase2(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory)
        {
            Services = services;
        }

    }
}