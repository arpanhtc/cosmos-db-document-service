using CosmosDbManager.Web.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace CosmosDbManager.Web.Tests.Extensions;

public sealed class SessionExtensionsTests
{
    [Fact]
    public void SetObject_And_GetObject_ShouldRoundTrip()
    {
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new TestSessionFeature(new TestSession()));

        var session = context.Session;
        var expected = new TestPayload("alpha", 42);

        session.SetObject("payload", expected);

        var actual = session.GetObject<TestPayload>("payload");

        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(expected);
    }

    private sealed record TestPayload(string Name, int Count);

    private sealed class TestSessionFeature : ISessionFeature
    {
        public TestSessionFeature(ISession session)
        {
            Session = session;
        }

        public ISession Session { get; set; }
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;

        public string Id { get; } = Guid.NewGuid().ToString("N");

        public bool IsAvailable => true;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
    }
}
