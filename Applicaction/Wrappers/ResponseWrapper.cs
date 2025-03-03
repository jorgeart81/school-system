using System;

namespace Applicaction.Wrappers;

public class ResponseWrapper : IResponseWrapper
{
    public List<string> Messages { get; set; } = [];
    public bool IsSuccessful { get; set; }

    public static IResponseWrapper Fail()
    {
        return new ResponseWrapper()
        {
            IsSuccessful = false
        };
    }

    public static IResponseWrapper Fail(string message)
    {
        return new ResponseWrapper()
        {
            IsSuccessful = false,
            Messages = [message],
        };
    }

    public static IResponseWrapper Fail(List<string> messages)
    {
        return new ResponseWrapper()
        {
            IsSuccessful = false,
            Messages = messages,
        };
    }

    public static IResponseWrapper Success()
    {
        return new ResponseWrapper()
        {
            IsSuccessful = true
        };
    }

    public static IResponseWrapper Success(string message)
    {
        return new ResponseWrapper()
        {
            IsSuccessful = true,
            Messages = [message],
        };
    }

    public static IResponseWrapper Success(List<string> messages)
    {
        return new ResponseWrapper()
        {
            IsSuccessful = true,
            Messages = messages,
        };
    }

    public static Task<IResponseWrapper> FailAsync() => Task.FromResult(Fail());
    public static Task<IResponseWrapper> FailAsync(string message) => Task.FromResult(Fail(message));
    public static Task<IResponseWrapper> FailAsync(List<string> messages) => Task.FromResult(Fail(messages));

    public static Task<IResponseWrapper> SuccesAsync() => Task.FromResult(Success());
    public static Task<IResponseWrapper> SuccesAsync(string message) => Task.FromResult(Success(message));
    public static Task<IResponseWrapper> SuccesAsync(List<string> messages) => Task.FromResult(Success(messages));
}

public class ResponseWrapper<T> : ResponseWrapper, IResponseWrapper<T>
{
    public T Data { get; set; }

    public new static IResponseWrapper<T> Fail()
    {
        return new ResponseWrapper<T>()
        {
            IsSuccessful = false
        };
    }

    public new static IResponseWrapper<T> Fail(string message)
    {
        return new ResponseWrapper<T>()
        {
            IsSuccessful = false,
            Messages = [message],
        };
    }

    public new static IResponseWrapper<T> Fail(List<string> messages)
    {
        return new ResponseWrapper<T>()
        {
            IsSuccessful = false,
            Messages = messages,
        };
    }

    public new static IResponseWrapper<T> Success()
    {
        return new ResponseWrapper<T>()
        {
            IsSuccessful = true
        };
    }

    public new static IResponseWrapper<T> Success(string message)
    {
        return new ResponseWrapper<T>()
        {
            IsSuccessful = true,
            Messages = [message],
        };
    }

    public static IResponseWrapper<T> Success(T data)
    {
        return new ResponseWrapper<T>()
        {
            IsSuccessful = true,
            Data = data,
        };
    }

    public static IResponseWrapper<T> Success(T data, string message)
    {
        return new ResponseWrapper<T>()
        {
            IsSuccessful = true,
            Data = data,
            Messages = [message],
        };
    }

    public static IResponseWrapper<T> Success(T data, List<string> messages)
    {
        return new ResponseWrapper<T>()
        {
            IsSuccessful = true,
            Data = data,
            Messages = messages,
        };
    }

    public new static Task<IResponseWrapper<T>> FailAsync() => Task.FromResult(Fail());
    public new static Task<IResponseWrapper<T>> FailAsync(string message) => Task.FromResult(Fail(message));
    public new static Task<IResponseWrapper<T>> FailAsync(List<string> messages) => Task.FromResult(Fail(messages));

    public new static Task<IResponseWrapper<T>> SuccesAsync() => Task.FromResult(Success());
    public new static Task<IResponseWrapper<T>> SuccesAsync(string message) => Task.FromResult(Success(message));
    public static Task<IResponseWrapper<T>> SuccesAsync(T data) => Task.FromResult(Success(data));
    public static Task<IResponseWrapper<T>> SuccesAsync(T data, string message) => Task.FromResult(Success(data, message));
    public static Task<IResponseWrapper<T>> SuccesAsync(T data, List<string> messages) => Task.FromResult(Success(data, messages));

}
