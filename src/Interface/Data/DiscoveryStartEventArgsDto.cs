using Interface.Data.Inner;
using Interface.Data.Logger;

namespace Interface.Data;

public class DiscoveryStartEventArgsDto : EventArgsBaseDto
{
    public DiscoveryCriteriaDto DiscoveryCriteria { get; set; }
}