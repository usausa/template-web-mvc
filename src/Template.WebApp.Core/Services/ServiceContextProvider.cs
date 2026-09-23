namespace Template.WebApp.Services;

public abstract class ServiceContextProvider
{
    public abstract ServiceContext Current { get; }
}
