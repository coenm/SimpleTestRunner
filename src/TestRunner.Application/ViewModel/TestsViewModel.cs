namespace TestRunner.Application.ViewModel;

using System;
using TestRunner.Application.ViewModel.Common;
using TestRunner.Core.Model;
using TestRunViewer.Model;

public class TestsViewModel : ViewModelBase
{
    private State _state;
    private string _message;
    private readonly SingleTestModel2 _model;

    public TestsViewModel(SingleTestModel2 model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        Message = string.Empty;
        model.Update += OnUpdate;
        ResetState();
    }
    
    public Guid Id => _model.TestCaseId;

    public string Name => _model.DisplayName;

    public State State
    {
        get => _state;

        set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
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
        State = _model.State switch
        {
            TestState.Empty => State.Unknown,
            TestState.Started => State.Executing,
            TestState.Succeeded => State.Succeeded,
            TestState.Failed => State.Failed,
            TestState.Skipped => State.Skipped,
            _ => State.Unknown,
        };
    }
}