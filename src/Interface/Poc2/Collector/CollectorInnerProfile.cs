using AutoMapper;
using Interface.Data.Collector.Inner;

namespace Interface.Poc2.Collector;

public class CollectorInnerProfile : Profile
{
    public CollectorInnerProfile()
    {
        CreateMap<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase, TestCaseDto>()
            .ForMember(dest => dest.CodeFilePath, opts => opts.MapFrom(src => src.CodeFilePath))
            .ForMember(dest => dest.DisplayName, opts => opts.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.ExecutorUri, opts => opts.MapFrom(src => src.ExecutorUri))
            .ForMember(dest => dest.FullyQualifiedName, opts => opts.MapFrom(src => src.FullyQualifiedName))
            .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
            .ForMember(dest => dest.LineNumber, opts => opts.MapFrom(src => src.LineNumber))
            .ForMember(dest => dest.Source, opts => opts.MapFrom(src => src.Source))
            ;
    }
}