namespace TestRunner.Application.ViewModel;

using System;
using TestRunner.Application.ViewModel.Common;
using TestRunner.Core.Model;
using TestRunViewer.Model;

public class TestsViewModel : ViewModelBase
{
    private TestState _testState;
    private string _message;
    private readonly TestModel _model;

    public TestsViewModel(TestModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        Message = string.Empty;
        model.Update += OnUpdate;
        ResetState();
    }
    
    public Guid Id => _model.TestCaseId;

    public string Name => _model.DisplayName;

    public TestState TestState
    {
        get => _testState;

        set
        {
            if (_testState == value)
            {
                return;
            }

            _testState = value;
            OnPropertyChanged();
        }
    }

    public string Message
    {
        get => _message;

        set
        {
            var suffix = string.IsNullOrEmpty(value) ? string.Empty : $"- {value}";
            _message = $"{Id}{suffix}";
            OnPropertyChanged();
        }
    }

    public void Dispose()
    {
        _model.Update -= OnUpdate;
    }

    private void OnUpdate(object sender, EventArgs e)
    {
        Message = string.Empty;
        ResetState();
    }

    public void ResetState()
    {
        TestState = _model.State switch
        {
            TestRunViewer.Model.TestState.Empty => TestState.Unknown,
            TestRunViewer.Model.TestState.Started => TestState.Executing,
            TestRunViewer.Model.TestState.Succeeded => TestState.Succeeded,
            TestRunViewer.Model.TestState.Failed => TestState.Failed,
            TestRunViewer.Model.TestState.Skipped => TestState.Skipped,
            _ => TestState.Unknown,
        };
    }
}