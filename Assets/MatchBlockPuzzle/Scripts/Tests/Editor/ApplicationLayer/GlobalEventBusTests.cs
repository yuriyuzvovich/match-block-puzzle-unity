using MatchPuzzle.ApplicationLayerLayer.EventBus;
using MatchPuzzle.Core.Interfaces;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.ApplicationLayer
{
    public class GlobalEventBusTests
    {
        private GlobalEventBus _bus;
        private int _received;

        [SetUp]
        public void SetUp()
        {
            _bus = new GlobalEventBus();
            _received = 0;
        }

        [Test]
        public void Publish_NotifiesSubscribedHandlers()
        {
            _bus.Subscribe<LevelStartedEvent>(_ => _received++);

            _bus.Publish(new LevelStartedEvent(1));

            Assert.AreEqual(1, _received);
        }

        [Test]
        public void Unsubscribe_StopsFurtherNotifications()
        {
            void Handler(LevelStartedEvent _) => _received++;

            _bus.Subscribe<LevelStartedEvent>(Handler);
            _bus.Unsubscribe<LevelStartedEvent>(Handler);

            _bus.Publish(new LevelStartedEvent(2));

            Assert.AreEqual(0, _received);
        }

        [Test]
        public void Clear_RemovesAllSubscriptions()
        {
            _bus.Subscribe<LevelStartedEvent>(_ => _received++);
            _bus.Clear();

            _bus.Publish(new LevelStartedEvent(3));

            Assert.AreEqual(0, _received);
        }

        [Test]
        public void MultipleEventTypes_DoNotInterfere()
        {
            int started = 0;
            int completed = 0;

            _bus.Subscribe<LevelStartedEvent>(_ => started++);
            _bus.Subscribe<LevelCompletedEvent>(_ => completed++);

            _bus.Publish(new LevelStartedEvent(1));
            _bus.Publish(new LevelCompletedEvent(1, 2));

            Assert.AreEqual(1, started);
            Assert.AreEqual(1, completed);
        }
    }
}
