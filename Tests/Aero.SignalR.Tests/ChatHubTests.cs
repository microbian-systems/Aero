using System.Buffers;
using Aero.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TUnit.Assertions;
using TUnit.Core;

namespace Aero.SignalR.Tests;

public sealed class ChatHubTests
{
    [Test]
    public async Task SendMessage_broadcasts_the_legacy_client_method_with_user_and_message()
    {
        var clients = Substitute.For<IHubCallerClients>();
        var allClients = Substitute.For<IClientProxy>();
        allClients.SendCoreAsync(Arg.Any<string>(), Arg.Any<object?[]>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        clients.All.Returns(allClients);
        var hub = new ChatHub { Clients = clients };

        await hub.SendMessage("Avery", "Proposal ready");

        await allClients.Received(1).SendCoreAsync(
            "ReceiveMessage",
            Arg.Is<object?[]>(arguments =>
                arguments.Length == 2 &&
                (string?)arguments[0] == "Avery" &&
                (string?)arguments[1] == "Proposal ready"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ChatHub_references_the_net10_shared_framework_signalr_core()
    {
        var signalRReference = typeof(ChatHub).Assembly
            .GetReferencedAssemblies()
            .Single(reference => reference.Name == "Microsoft.AspNetCore.SignalR.Core");

        await Assert.That(signalRReference.Version!.Major).IsGreaterThanOrEqualTo(10);
    }

    [Test]
    public async Task Default_json_hub_protocol_uses_system_text_json_without_newtonsoft()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSignalR();
        await using var provider = services.BuildServiceProvider();
        var protocol = provider.GetRequiredService<IHubProtocolResolver>().AllProtocols
            .Single(candidate => candidate.Name == "json");
        var references = protocol.GetType().Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        await Assert.That(references.Contains("System.Text.Json")).IsTrue();
        await Assert.That(references.Contains("Newtonsoft.Json")).IsFalse();
    }

    [Test]
    public async Task Default_json_hub_protocol_round_trips_a_receive_message_frame()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSignalR();
        await using var provider = services.BuildServiceProvider();
        var protocol = provider.GetRequiredService<IHubProtocolResolver>().AllProtocols
            .Single(candidate => candidate.Name == "json");
        var expectedUser = "Miyuki \ud83d\ude80 \"Ops\"";
        var expectedMessage = "Line one\nLine two — \u0417\u0430\043a\0440\u044b\u0442\u043e";
        var output = new ArrayBufferWriter<byte>();

        protocol.WriteMessage(
            new InvocationMessage(null, "ReceiveMessage", [expectedUser, expectedMessage]),
            output);
        var serialized = output.WrittenMemory.ToArray();
        var input = new ReadOnlySequence<byte>(serialized);

        var parsed = protocol.TryParseMessage(ref input, new ReceiveMessageBinder(), out var parsedMessage);

        await Assert.That(serialized[^1]).IsEqualTo((byte)0x1e);
        await Assert.That(parsed).IsTrue();
        await Assert.That(input.IsEmpty).IsTrue();
        var invocation = parsedMessage as InvocationMessage;
        await Assert.That(invocation).IsNotNull();
        await Assert.That(invocation!.Target).IsEqualTo("ReceiveMessage");
        await Assert.That((string?)invocation.Arguments[0]).IsEqualTo(expectedUser);
        await Assert.That((string?)invocation.Arguments[1]).IsEqualTo(expectedMessage);
    }

    private sealed class ReceiveMessageBinder : IInvocationBinder
    {
        public IReadOnlyList<Type> GetParameterTypes(string methodName) =>
            methodName == "ReceiveMessage"
                ? [typeof(string), typeof(string)]
                : throw new InvalidOperationException($"Unexpected hub method '{methodName}'.");

        public Type GetReturnType(string invocationId) => typeof(void);

        public Type GetStreamItemType(string streamId) => typeof(void);
    }
}
