using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using TestOutcome = Interface.Data.TestOutcome;

namespace Interface.Poc2.Collector;

public class EnumsProfile : Profile
{
    public EnumsProfile()
    {
        CreateMap<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome, Interface.Data.TestOutcome>()
            .ConvertUsingEnumMapping(opt => opt
                                            // optional: .MapByValue() or MapByName(), without configuration MapByValue is used
                                            .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.None, TestOutcome.None)
                                            .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Passed, TestOutcome.Passed)
                                            .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Failed, TestOutcome.Failed)
                                            .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Skipped, TestOutcome.Skipped)
                                            .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.NotFound,
                                                TestOutcome.NotFound)
            )
            // .ReverseMap() // to support Destination to Source mapping, including custom mappings of ConvertUsingEnumMapping
            ;
    }
}