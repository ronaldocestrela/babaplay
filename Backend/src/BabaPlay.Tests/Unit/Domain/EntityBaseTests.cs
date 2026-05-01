using BabaPlay.Domain.Entities;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class EntityBaseTests
{
    private class TestEntity : EntityBase { }

    [Fact]
    public void Create_NewEntity_ShouldHaveNonEmptyId()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_NewEntity_ShouldHaveCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var entity = new TestEntity();

        entity.CreatedAt.Should().BeAfter(before);
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void RaiseDomainEvent_ShouldAddToDomainEvents()
    {
        var entity = new TestEntityWithEvents();
        entity.TriggerEvent();

        entity.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var entity = new TestEntityWithEvents();
        entity.TriggerEvent();

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    private class TestEntityWithEvents : EntityBase
    {
        public void TriggerEvent() => RaiseDomainEvent(new TestDomainEvent());
    }

    private class TestDomainEvent : BabaPlay.Domain.Events.IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
