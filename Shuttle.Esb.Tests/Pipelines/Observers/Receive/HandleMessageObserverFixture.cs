using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class HandleMessageObserverFixture
{
    private async Task Should_be_able_to_return_when_no_message_handling_is_required_async(bool sync)
    {
        await Task.CompletedTask;
    }
}