namespace JiraTogglSync.Services;

public class OperationResult
{
	public enum OperationStatus
	{
		Success,
		Error
	}

	public OperationStatus Status { get; }
	public string? Message { get; private init; }
	public WorkLogEntry OperationArgument { get; }

	private OperationResult(OperationStatus status, WorkLogEntry operationArgument)
	{
		Status = status;
		OperationArgument = operationArgument;
	}

	public static OperationResult Success(WorkLogEntry arg)
	{
		return new OperationResult(OperationStatus.Success, arg);
	}

	public static OperationResult Error(string message, WorkLogEntry arg)
	{
		return new OperationResult(OperationStatus.Error, arg)
		{
			Message = message
		};
	}
}