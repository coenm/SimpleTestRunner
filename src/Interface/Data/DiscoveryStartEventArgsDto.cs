using Interface.Data.Inner;
using Interface.Data.Logger;

namespace Interface.Data;

// automapper
public class DiscoveryStartEventArgsDto : EventArgsBaseDto
{
    public DiscoveryCriteriaDto DiscoveryCriteria { get; set; }
}