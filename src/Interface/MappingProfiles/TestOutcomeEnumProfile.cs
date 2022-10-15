namespace Interface.MappingProfiles;

using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using TestOutcome = Data.TestOutcome;

public class TestOutcomeEnumProfile : Profile
{
    public TestOutcomeEnumProfile()
    {
        CreateMap<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome, TestOutcome>()
            .ConvertUsingEnumMapping(opt => opt
                // optional: .MapByValue() or MapByName(), without configuration MapByValue is used
                .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.None, TestOutcome.None)
                .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Passed, TestOutcome.Passed)
                .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Failed, TestOutcome.Failed)
                .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Skipped, TestOutcome.Skipped)
                .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.NotFound, TestOutcome.NotFound)
            )
            // .ReverseMap() // to support Destination to Source mapping, including custom mappings of ConvertUsingEnumMapping
            ;
    }
}