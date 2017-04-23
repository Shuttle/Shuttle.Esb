# Shuttle.Esb

A highly flexible and free .NET open-source enterprise service bus.

# Documentation 

There is [extensive documentation](http://shuttle.github.io/shuttle-esb/) on our site and you can make use of the [samples](https://github.com/Shuttle/Shuttle.Esb.Samples) to get you going.

# Overview

### Send a command message for processing

``` c#
var container = new WindsorComponentContainer(new WindsorContainer());

ServiceBus.Register(container);

using (var bus = ServiceBus.Create(container).Start())
{
	bus.Send(new RegisterMemberCommand
	{
		UserName = "Mr Resistor",
		EMailAddress = "ohm@resistor.domain"
	});
}
```

### Publish an event message when something interesting happens

``` c#
var smRegistry = new Registry();
var registry = new StructureMapComponentRegistry(smRegistry);

ServiceBus.Register(registry); // will using bootstrapping to register SubscriptionManager

using (var bus = ServiceBus
	.Create(
		new StructureMapComponentResolver(
		new Container(smRegistry)))
	.Start())
{
	bus.Publish(new MemberRegisteredEvent
	{
		UserName = "Mr Resistor"
	});
}
```

### Subscribe to those interesting events

``` c#
SubscriptionManager.Default().Subscribe<MemberRegisteredEvent>();
```

### Handle any messages

``` c#
public class RegisterMemberHandler : IMessageHandler<RegisterMemberCommand>
{
	public void ProcessMessage(IHandlerContext<RegisterMemberCommand> context)
	{
		Console.WriteLine();
		Console.WriteLine("[MEMBER REGISTERED] : user name = '{0}'", context.Message.UserName);
		Console.WriteLine();

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
		Console.WriteLine();
		Console.WriteLine("[EVENT RECEIVED] : user name = '{0}'", context.Message.UserName);
		Console.WriteLine();
	}
}
```