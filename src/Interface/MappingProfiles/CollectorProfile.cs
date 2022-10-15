using System;
using AutoMapper;
using Interface.Data.Collector;
using Interface.Data.Logger;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace Interface.MappingProfiles;

public class CollectorProfile : Profile
{
    public CollectorProfile()
    {
        CreateMap<EventArgs, EventArgsBaseDto>()
            .Include<TestCaseEndEventArgs, TestCaseEndEventArgsDto>()
            .Include<TestCaseStartEventArgs, TestCaseStartEventArgsDto>()
            .Include<SessionStartEventArgs, SessionStartEventArgsDto>()
            .Include<SessionEndEventArgs, SessionEndEventArgsDto>()
            .Include<TestHostLaunchedEventArgs, TestHostLaunchedEventArgsDto>()
            ;

        CreateMap<TestCaseEndEventArgs, TestCaseEndEventArgsDto>()
            .ForMember(dest => dest.TestOutcome, opts => opts.MapFrom(src => src.TestOutcome))
            // rest is all inheritance.. todo
            .ForMember(dest => dest.IsChildTestCase, opts => opts.MapFrom(src => src.IsChildTestCase))
            .ForMember(dest => dest.TestCaseId, opts => opts.MapFrom(src => src.TestCaseId))
            .ForMember(dest => dest.TestCaseName, opts => opts.MapFrom(src => src.TestCaseName))
            .ForMember(dest => dest.TestElement, opts => opts.MapFrom(src => src.TestElement))
            ;

        CreateMap<TestCaseStartEventArgs, TestCaseStartEventArgsDto>()
            // rest is all inheritance.. todo
            .ForMember(dest => dest.IsChildTestCase, opts => opts.MapFrom(src => src.IsChildTestCase))
            .ForMember(dest => dest.TestCaseId, opts => opts.MapFrom(src => src.TestCaseId))
            .ForMember(dest => dest.TestCaseName, opts => opts.MapFrom(src => src.TestCaseName))
            .ForMember(dest => dest.TestElement, opts => opts.MapFrom(src => src.TestElement))
            ;

        CreateMap<SessionStartEventArgs, SessionStartEventArgsDto>()
            // todo, empty, but not empty.
            ;

        CreateMap<SessionEndEventArgs, SessionEndEventArgsDto>()
            // todo, empty, but not empty.
            ;

        CreateMap<TestHostLaunchedEventArgs, TestHostLaunchedEventArgsDto>()
            .ForMember(dest => dest.TestHostProcessId, opts => opts.MapFrom(src => src.TestHostProcessId))
            // todo, empty, but not empty.
            ;
    }
}