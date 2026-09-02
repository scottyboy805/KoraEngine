using System;
using System.Threading;
using System.Threading.Tasks;
using internal KoraPlayer;

namespace KoraPlayer;

public class Async<T>
{
    // Private
    private Task task;
    private Result<T> result;

    // Properties
    public bool IsComplete => task != null && task.IsCompleted;
    public bool IsCancelled => task != null && task.IsCanceled;

    // Constructor
    public ~this()
    {
        if (task != null)
        {
            delete task;
            task = null;
        }
    }

    // Methods
    public Result<T> Wait()
    {
        if (task == null)
            return .Err;

        task.GetAwaiter().GetResult();
        return result;
    }

    internal Result<void> Schedule(delegate Result<T>() func)
    {
		// Check for already scheduled
		if(task != null)
			return .Err;

		// Create the task
        task = new .(new (state) =>
        {
            Async<T> self = (Async<T>)state;

			// Invoke the desired function
            Result<T> r = func();

			// store result for later retrieval
            self.result = r;
        }, this, default, TaskCreationOptions.None);

        task.Start();
		return .Ok;
    }
}

public class Async
{
	// Private
	private Task task;
	private Result<void> result;

	// Properties
	public bool IsComplete => task != null && task.IsCompleted;
	public bool IsCancelled => task != null && task.IsCanceled;

	// Constructor
	public ~this()
	{
	    if (task != null)
	    {
	        delete task;
	        task = null;
	    }
	}

	// Methods
	public Result<void> Wait()
	{
	    if (task == null)
	        return .Err;

	    task.GetAwaiter().GetResult();
	    return result;
	}

	internal Result<void> Schedule(delegate Result<void>() func)
	{
		// Check for already scheduled
		if(task != null)
			return .Err;

		// The main handler
	    task = new .(new (state) =>
	    {
			// Invoke
	        Result<void> r = func();

	        // store result for later retrieval
	        ((Async)state).result = r;
	    }, this, default, TaskCreationOptions.None);

	    task.Start(); // <-- THIS is what actually schedules on thread pool
		return .Ok;
	}
}