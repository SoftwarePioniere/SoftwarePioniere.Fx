using System.Threading;

namespace SoftwarePioniere.DomainModel
{
    public interface ISaga
    {
        void Initialize(CancellationToken cancellationToken = default(CancellationToken));
    }
}
