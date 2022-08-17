using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using TestOutcome = Interface.Data.TestOutcome;

namespace Interface.Poc2;

public class EnumsProfile : Profile
{
    public EnumsProfile()
    {
        CreateMap<Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestMessageLevel, Interface.Data.TestMessageLevel>()
            .ConvertUsingEnumMapping(opt => opt
                                            // optional: .MapByValue() or MapByName(), without configuration MapByValue is used
                                            .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestMessageLevel.Error, Interface.Data.TestMessageLevel.Error)
                                            .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestMessageLevel.Informational, Interface.Data.TestMessageLevel.Informational)
                                            .MapValue(Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestMessageLevel.Warning, Interface.Data.TestMessageLevel.Warning)
            )
            // .ReverseMap() // to support Destination to Source mapping, including custom mappings of ConvertUsingEnumMapping
            ;
    }
}