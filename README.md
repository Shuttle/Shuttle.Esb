# Documentation

Please visit out [official documentation](https://shuttle.github.io/shuttle-esb/) for more information.

# Getting Started

Start a new **Console Application** project and select a Shuttle.Esb queue implementation from the supported queues:

```
PM> Install-Package Shuttle.Esb.AzureMQ
```

Now we'll need select one of the [supported containers](https://shuttle.github.io/shuttle-core/container/shuttle-core-container.html#implementations):

```
PM> Install-Package Shuttle.Core.Autofac
```

We'll also need to host our endpoint using a [worker service](https://shuttle.github.io/shuttle-core/service-hosting/shuttle-core-workerservice.html):

```
PM> Install-Package Shuttle.Core.WorkerService
```

Next we'll implement our endpoint in order to start listening on our queue:

``` c#
internal class Program
{
	private static void Main()
	{
		ServiceHost.Run<Host>();
	}
}

public class Host : IServiceHost
{
	private IServiceBus _bus;

    public void Start()
    {
        var containerBuilder = new ContainerBuilder();
        var registry = new AutofacComponentRegistry(containerBuilder);

        registry.Register<IAzureStorageConfiguration, DefaultAzureStorageConfiguration>();
        registry.RegisterServiceBus();

        _bus = new AutofacComponentResolver(containerBuilder.Build())
            .Resolve<IServiceBus>().Start();
    }

	public void Stop()
	{
		_bus.Dispose();
	}
}
```

A bit of configuration is going to be needed to help things along:

``` xml
<configuration>
	<configSections>
		<section name="serviceBus" type="Shuttle.Esb.ServiceBusSection, Shuttle.Esb"/>
	</configSections>

	<appSettings>
		<add key="azure" value="UseDevelopmentStorage=true;" />
	</appSettings>

	<serviceBus>
		<inbox uri="azuremq://azure/shuttle-server-work" 
		       deferredUri="azuremq://azure/shuttle-server-deferred"
		       errorUri="azuremq://azure/shuttle-error" />
	</serviceBus>
</configuration>
```

### Send a command message for processing

``` c#
bus.Send(new RegisterMemberCommand
{
    UserName = "user-name",
    EMailAddress = "user@domain.com"
});
```

### Publish an event message when something interesting happens

``` c#
bus.Publish(new MemberRegisteredEvent
{
    UserName = "user-name"
});
```

### Subscribe to those interesting events

``` c#
resolver.Resolve<ISubscriptionManager>().Subscribe<MemberRegisteredEvent>();
```

### Handle any messages

``` c#
public class RegisterMemberHandler : IMessageHandler<RegisterMemberCommand>
{
    public RegisterMemberHandler(IDependency dependency)
    {
    }

	public void ProcessMessage(IHandlerContext<RegisterMemberCommand> context)
	{
        // perform member registration

		context.Publish(new MemberRegisteredEvent
		{
			UserName = context.Message.UserName
		});
	}
}
```

``` c#
public class MemberRegisteredHandler : IMessageHandler<MemberRegisteredEvent>
{
	public void ProcessMessage(IHandlerContext<MemberRegisteredEvent> context)
	{
        // processing
	}
}
```