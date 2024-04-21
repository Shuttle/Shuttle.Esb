# Documentation

Please visit out [official documentation](https://www.pendel.co.za/shuttle-esb/index.html) for more information.

# Getting Started

Start a new **Console Application** project.  We'll need to install one of the support queue implementations.  For this example we'll use `Shuttle.Esb.AzureStorageQueues` which can be hosted locally using [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage):

```
PM> Install-Package Shuttle.Esb.AzureStorageQueues
```

We'll also make use of the [.NET generic host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host):

```
PM> Install-Package Microsoft.Extensions.Hosting
```

Next we'll implement our endpoint in order to start listening on our queue:

``` c#
internal class Program
{
    static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddServiceBus(builder =>
                    {
                        builder.Options.Inbox.WorkQueueUri = "azuresq://azure/work";
                        builder.Options.Asynchronous = true; // NOTE: we'll be using async processing
                    })
                    .AddAzureStorageQueues(builder =>
                    {
                        builder.AddOptions("azure", new AzureStorageQueueOptions
                        {
                            ConnectionString = "UseDevelopmentStorage=true;"
                        });
                    });
            })
            .Build()
            .RunAsync();
    }
}
```

Even though the options may be set directly as above, typically one would make use of a configuration provider:

```c#
internal class Program
{
    private static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                var configuration =
                    new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();

                services
                    .AddSingleton<IConfiguration>(configuration)
                    .AddServiceBus(builder =>
                    {
                        configuration
                            .GetSection(ServiceBusOptions.SectionName)
                            .Bind(builder.Options);

                        builder.Options.Asynchronous = true; // NOTE: we'll be using async processing
                    })
                    .AddAzureStorageQueues(builder =>
                    {
                        builder.AddOptions("azure", new AzureStorageQueueOptions
                        {
                            ConnectionString = configuration
                                .GetConnectionString("azure")
                        });
                    });
            })
            .Build()
            .RunAsync();
    }
}
```

The `appsettings.json` file would be as follows (remember to set to `Copy always`):

```json
{
  "ConnectionStrings": {
    "azure": "UseDevelopmentStorage=true;"
  },
  "Shuttle": {
    "ServiceBus": {
      "Inbox": {
        "WorkQueueUri": "azuresq://azure/work",
      }
    }
  }
}
```

### Send a command message for processing

``` c#
await serviceBus.SendAsync(new RegisterMember
{
    UserName = "user-name",
    EMailAddress = "user@domain.com"
});
```

### Publish an event message when something interesting happens

Before publishing an event one would need to register an `ISubscrtiptionService` implementation such as [Shuttle.Esb.Sql.Subscription](/implementations/subscription/sql.md).

``` c#
await serviceBus.PublishAsync(new MemberRegistered
{
    UserName = "user-name"
});
```

### Subscribe to those interesting events

``` c#
services.AddServiceBus(builder =>
{
    builder.AddSubscription<MemberRegistered>();
});
```

### Handle any messages

``` c#
public class RegisterMemberHandler : IAsyncMessageHandler<RegisterMember>
{
    public RegisterMemberHandler(IDependency dependency)
    {
    }

	public async Task ProcessMessageAsync(IHandlerContext<RegisterMember> context)
	{
        // perform member registration

		await context.PublishAsync(new MemberRegistered
		{
			UserName = context.Message.UserName
		});
	}
}
```

``` c#
public class MemberRegisteredHandler : IAsyncMessageHandler<MemberRegistered>
{
	public async Task ProcessMessageAsync(IHandlerContext<MemberRegistered> context)
	{
        // processing
	}
}
```