namespace Application.Wrappers;

public interface IResponseWrapper
{
    List<string> Messages { get; set; }
    bool IsSuccessful { get; set; }

    public interface IResponseWrapper<T> : IResponseWrapper
    {
        T? Data { get; set; }
    }
}
