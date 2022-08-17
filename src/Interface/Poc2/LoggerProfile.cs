using AutoMapper;
using Interface.Data;
using Interface.Data.Inner;
using Interface.Data.Logger;
using Interface.Data.Logger.Inner;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;

namespace Interface.Poc2;

public class LoggerProfile : Profile
{   
    public LoggerProfile()
    {
        CreateMap<TestRunCriteria, TestRunCriteriaDto>()
            .ForMember(dest => dest.Tests, opts => opts.MapFrom(src => src.Tests))
            .ForMember(dest => dest.TestCaseFilter, opts => opts.MapFrom(src => src.TestCaseFilter))
            .ForMember(dest => dest.HasSpecificSources, opts => opts.MapFrom(src => src.HasSpecificSources))
            // inherrit
            .ForMember(dest => dest.KeepAlive, opts => opts.MapFrom(src => src.KeepAlive))
            .ForMember(dest => dest.TestRunSettings, opts => opts.MapFrom(src => src.TestRunSettings))
            .ForMember(dest => dest.FrequencyOfRunStatsChangeEvent, opts => opts.MapFrom(src => src.FrequencyOfRunStatsChangeEvent))
            .ForMember(dest => dest.RunStatsChangeEventTimeout, opts => opts.MapFrom(src => src.RunStatsChangeEventTimeout))
            ;

        CreateMap<TestRunStatistics, TestRunStatisticsDto>()
            .ForMember(dest => dest.ExecutedTests, opts => opts.MapFrom(src => src.ExecutedTests))
            .ForMember(dest => dest.Stats, opts => opts.MapFrom(src => src.Stats))
            ;    
        
        CreateMap<TestResultMessage, TestResultMessageDto>()
            .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.Category))
            .ForMember(dest => dest.Text, opts => opts.MapFrom(src => src.Text))
            ;
        CreateMap<TestSessionInfo, TestSessionInfoDto>()
            .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
            ;
        
        CreateMap<DiscoveryCriteria, DiscoveryCriteriaDto>()
            .ForMember(dest => dest.DiscoveredTestEventTimeout, opts => opts.MapFrom(src => src.DiscoveredTestEventTimeout))
            .ForMember(dest => dest.AdapterSourceMap, opts => opts.MapFrom(src => src.AdapterSourceMap))
            .ForMember(dest => dest.Package, opts => opts.MapFrom(src => src.Package))
            .ForMember(dest => dest.FrequencyOfDiscoveredTestsEvent, opts => opts.MapFrom(src => src.FrequencyOfDiscoveredTestsEvent))
            .ForMember(dest => dest.RunSettings, opts => opts.MapFrom(src => src.RunSettings))
            .ForMember(dest => dest.TestCaseFilter, opts => opts.MapFrom(src => src.TestCaseFilter))
            .ForMember(dest => dest.TestSessionInfo, opts => opts.MapFrom(src => src.TestSessionInfo))
            ;

        CreateMap<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult, TestResultDto>()
            .ForMember(dest => dest.TestCase, opts => opts.MapFrom(src => src.TestCase)) // used in collector
            .ForMember(dest => dest.Outcome, opts => opts.MapFrom(src => src.Outcome)) // used in collector
            .ForMember(dest => dest.ErrorMessage, opts => opts.MapFrom(src => src.ErrorMessage))
            .ForMember(dest => dest.ErrorStackTrace, opts => opts.MapFrom(src => src.ErrorStackTrace))
            .ForMember(dest => dest.DisplayName, opts => opts.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.Messages, opts => opts.MapFrom(src => src.Messages))
            .ForMember(dest => dest.ComputerName, opts => opts.MapFrom(src => src.ComputerName))
            .ForMember(dest => dest.Duration, opts => opts.MapFrom(src => src.Duration))
            .ForMember(dest => dest.StartTime, opts => opts.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime, opts => opts.MapFrom(src => src.EndTime))
            // inherrit,,emtpy
            ;

        CreateMap<EventArgs, EventArgsBaseDto>()
            .Include<TestRunMessageEventArgs, TestRunMessageEventArgsDto>()
            .Include<TestResultEventArgs, TestResultEventArgsDto>()
            .Include<TestRunStartEventArgs, TestRunStartEventArgsDto>()
            .Include<DiscoveryCompleteEventArgs, DiscoveryCompleteEventArgsDto>()
            .Include<DiscoveredTestsEventArgs, DiscoveredTestsEventArgsDto>()
            .Include<DiscoveryStartEventArgs, DiscoveryStartEventArgsDto>()
            .Include<TestRunCompleteEventArgs, TestRunCompleteEventArgsDto>()
            ;

        CreateMap<TestRunMessageEventArgs, TestRunMessageEventArgsDto>()
            .ForMember(dest => dest.Message, opts => opts.MapFrom(src => src.Message))
            .ForMember(dest => dest.Level, opts => opts.MapFrom(src => src.Level))
            ;

        CreateMap<TestResultEventArgs, TestResultEventArgsDto>()
            .ForMember(dest => dest.Result, opts => opts.MapFrom(src => src.Result))
            ;

        CreateMap<TestRunStartEventArgs, TestRunStartEventArgsDto>()
            .ForMember(dest => dest.TestRunCriteria, opts => opts.MapFrom(src => src.TestRunCriteria))
            ;

        CreateMap<DiscoveryCompleteEventArgs, DiscoveryCompleteEventArgsDto>()
            .ForMember(dest => dest.IsAborted, opts => opts.MapFrom(src => src.IsAborted))
            .ForMember(dest => dest.TotalCount, opts => opts.MapFrom(src => src.TotalCount))
            .ForMember(dest => dest.Metrics, opts => opts.MapFrom(src => src.Metrics))
            ;

        CreateMap<DiscoveredTestsEventArgs, DiscoveredTestsEventArgsDto>()
            .ForMember(dest => dest.DiscoveredTestCases, opts => opts.MapFrom(src => src.DiscoveredTestCases))
            ;

        CreateMap<DiscoveryStartEventArgs, DiscoveryStartEventArgsDto>()
            .ForMember(dest => dest.DiscoveryCriteria, opts => opts.MapFrom(src => src.DiscoveryCriteria))
            ;


        CreateMap<TestRunCompleteEventArgs, TestRunCompleteEventArgsDto>()
            .ForMember(dest => dest.TestRunStatistics, opts => opts.MapFrom(src => src.TestRunStatistics))
            .ForMember(dest => dest.IsCanceled, opts => opts.MapFrom(src => src.IsCanceled))
            .ForMember(dest => dest.IsAborted, opts => opts.MapFrom(src => src.IsAborted))
            .ForMember(dest => dest.Error, opts => opts.MapFrom(src => src.Error))
            .ForMember(dest => dest.ElapsedTimeInRunningTests, opts => opts.MapFrom(src => src.ElapsedTimeInRunningTests))
            .ForMember(dest => dest.Metrics, opts => opts.MapFrom(src => src.Metrics)) //check dictionary, objects
            ;





    }
}