namespace TestRunViewer.ViewModel;

using System;
using TestRunViewer.Model;
using TestRunViewer.ViewModel.Common;

public class SingleTestViewModel : ViewModelBase, IDisposable
{
    private readonly SingleTestModel _client;

    private State _state;
    private string _message;

    public SingleTestViewModel(SingleTestModel client)
    {
        _client = client;

        Message = string.Empty;

        _client.Update += OnUpdate;

        ResetState();

        // AddItemCommand = new Command(OnAddItem, _ => _client.State == TestOutcome.Failed);
    }

    // public IAsyncCommand AddItemCommand { get; }

    public Guid Id => _client.Id;

    public string Name => _client.Name;

    public State State
    {
        get => _state;

        set
        {
            _state = value;
            OnPropertyChanged();
            // GetItemsCommand.UpdateCanExecute();
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

    public string SessionId => _client.SessionId;

    public void Initialize()
    {
        try
        {
            _client.Start();
            State = State.NotRun;
        }
        catch (Exception ex)
        {
            State = State.Error;
            Message = ex.Message;
        }
    }

    public void Dispose()
    {
        _client.Update -= OnUpdate;

        _client.Dispose();
    }

    private void OnUpdate(object sender, EventArgs e)
    {
        Message = string.Empty;
        ResetState();
    }

    public void ResetState()
    {
        State = _client.State switch
            {
                TestState.Empty => State.Unknown,
                TestState.Started => State.Executing,
                TestState.Succeeded => State.Succeeded,
                TestState.Failed => State.Failed,
                TestState.Skipped => State.Skipped,
                _ => State.Unknown,
            };
    }

    // public async Task WaitForItemCountToBeGreaterOrEqual(int expectedCountTotal, CancellationToken cancellation)
    // {
    //     var tcs = new TaskCompletionSource<object>();
    //     await using (cancellation.Register(() => tcs.TrySetCanceled()))
    //     {
    //         void OnClientPropertyChanged(object sender, PropertyChangedEventArgs e)
    //         {
    //             if (e.PropertyName != nameof(ItemCount))
    //             {
    //                 return;
    //             }
    //
    //             if (ItemCount >= expectedCountTotal)
    //             {
    //                 tcs.TrySetResult(null);
    //             }
    //         }
    //
    //         PropertyChanged += OnClientPropertyChanged;
    //         try
    //         {
    //             await tcs.Task.ConfigureAwait(false);
    //         }
    //         catch (OperationCanceledException)
    //         {
    //             // Ignore
    //         }
    //         finally
    //         {
    //             PropertyChanged -= OnClientPropertyChanged;
    //         }
    //     }
    // }
}