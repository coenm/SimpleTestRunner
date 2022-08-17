namespace TestRunViewer.ViewModel;

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using TestRunViewer.Model;
using TestRunViewer.ViewModel.Common;

public class TestViewModel : ViewModelBase, IDisposable
{
    private readonly IdleClient _client;

    private State _state;
    private string _message;
    private int _itemCount;

    public TestViewModel(IdleClient client)
    {
        _client = client;

        ItemCount = _client.ItemCount;
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

    public int ItemCount
    {
        get => _itemCount;

        set
        {
            _itemCount = value;
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

    private void OnItemsChanged(object sender, EventArgs e)
    {
        ItemCount = _client.ItemCount;
    }

    public Task WaitForAddedItems(int expectedItemAddition, CancellationToken cancellation)
    {
        var expectedCount = ItemCount + expectedItemAddition;
        return WaitForItemCountToBeGreaterOrEqual(expectedCount, cancellation);
    }

    public async Task WaitForItemCountToBeGreaterOrEqual(int expectedCountTotal, CancellationToken cancellation)
    {
        var tcs = new TaskCompletionSource<object>();
        await using (cancellation.Register(() => tcs.TrySetCanceled()))
        {
            void OnClientPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != nameof(ItemCount))
                {
                    return;
                }

                if (ItemCount >= expectedCountTotal)
                {
                    tcs.TrySetResult(null);
                }
            }

            PropertyChanged += OnClientPropertyChanged;
            try
            {
                await tcs.Task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            finally
            {
                PropertyChanged -= OnClientPropertyChanged;
            }
        }
    }
}

public enum State
{
    NotRun,
    Skipped,
    Executing,
    Succeeded,
    Failed,

    Unknown,
    Error,
}