using System;
using System.Collections.Generic;
using System.Text;

namespace SoftwarePioniere.Projections
{
    public interface IProjectionContext
    {
        bool IsLiveProcessing { get; }

        string StreamName { get; }
    }
}
