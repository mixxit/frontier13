using projectrarahat.src;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace projectrarahat.src
{
    public sealed class AchievementsModState
    {
        private static readonly AchievementsModState instance = new AchievementsModState();

        private ConcurrentDictionary<string, RecordedEventCounters> playerRecordedEvents = new ConcurrentDictionary<string, RecordedEventCounters>();
        private ConcurrentDictionary<string, TrackedEventTypes> eventsToMonitorForPlayer = new ConcurrentDictionary<string, TrackedEventTypes>();
        private ConcurrentDictionary<string, List<EventToTrack>> eventsToMonitorForClasses = new ConcurrentDictionary<string, List<EventToTrack>>();

        internal void ResetPlayer(IServerPlayer player)
        {
            if (playerRecordedEvents.ContainsKey(player.PlayerUID))
                playerRecordedEvents[player.PlayerUID] = new RecordedEventCounters();
            if (eventsToMonitorForPlayer.ContainsKey(player.PlayerUID))
                eventsToMonitorForPlayer[player.PlayerUID] = new TrackedEventTypes(player.PlayerUID);
        }

        internal List<EventToTrack> GetClassEventsToTrack(string className)
        {
            if (!eventsToMonitorForClasses.ContainsKey(className))
                return new List<EventToTrack>();

            return eventsToMonitorForClasses[className];
        }

        private AchievementsModState() { }

        public static AchievementsModState Instance
        {
            get
            {
                return instance;
            }
        }

        public void RegisterPlayerEventToTrack(IServerPlayer player, EventToTrack eventToTrack)
        {
            if (!eventsToMonitorForPlayer.ContainsKey(player.PlayerUID))
                eventsToMonitorForPlayer[player.PlayerUID] = new TrackedEventTypes(player.PlayerUID);

            if (eventsToMonitorForPlayer[player.PlayerUID].IsTrackingEvent(eventToTrack))
                return;

            eventsToMonitorForPlayer[player.PlayerUID].StartTrackingEvent(eventToTrack);
        }

        internal void SetTrackedClassEvents(Dictionary<string, List<EventToTrack>> classEventsToTrackData)
        {
            if (classEventsToTrackData == null || classEventsToTrackData.Keys.Count() < 1)
                return;

            foreach(var className in classEventsToTrackData.Keys)
            {
                if (!classEventsToTrackData.ContainsKey(className))
                    continue;

                if (classEventsToTrackData[className] == null || classEventsToTrackData[className].Count() < 1)
                    continue;

                if (!eventsToMonitorForClasses.ContainsKey(className))
                    eventsToMonitorForClasses[className] = new List<EventToTrack>();


                foreach (var eventToTrack in classEventsToTrackData[className])
                    eventsToMonitorForClasses[className].Add(eventToTrack);
            }
        }

        public void RecordEvent(TrackedEventEnum eventType, IServerPlayer player, Block block)
        {
            if (!playerRecordedEvents.ContainsKey(player.PlayerUID))
                playerRecordedEvents[player.PlayerUID] = new RecordedEventCounters();

            playerRecordedEvents[player.PlayerUID].IncrementEvent(new EventToTrack(eventType, block.BlockMaterial.ToString()));
        }

        public bool IsPlayerTrackingEvent(IServerPlayer player, EventToTrack eventToTrack)
        {
            if (!eventsToMonitorForPlayer.ContainsKey(player.PlayerUID))
                eventsToMonitorForPlayer[player.PlayerUID] = new TrackedEventTypes(player.PlayerUID);

            return eventsToMonitorForPlayer[player.PlayerUID].IsTrackingEvent(eventToTrack);
        }


        public void UnregisterPlayerEventToTrack(IServerPlayer player, TrackedEventEnum eventType, string uniqueRef)
        {
            if (!eventsToMonitorForPlayer.ContainsKey(player.PlayerUID))
                eventsToMonitorForPlayer[player.PlayerUID] = new TrackedEventTypes(player.PlayerUID);

            if (!eventsToMonitorForPlayer[player.PlayerUID].IsTrackingEvent(new EventToTrack(eventType, uniqueRef)))
                return;

            eventsToMonitorForPlayer[player.PlayerUID].StopTrackingEvent(new EventToTrack(eventType, uniqueRef));
        }

        internal class TrackedEventTypes
        {


            ConcurrentDictionary<string, bool> trackedEventTypes = new ConcurrentDictionary<string, bool>();
            private string playerUID;

            public TrackedEventTypes(string playerUID)
            {
                this.playerUID = playerUID;
            }

            public void StartTrackingEvent(EventToTrack eventToTrack)
            {
                if (!trackedEventTypes.ContainsKey(eventToTrack.GetUniqueIndex()))
                    trackedEventTypes[eventToTrack.GetUniqueIndex()] = false;

                trackedEventTypes[eventToTrack.GetUniqueIndex()] = true;
                LogUtils<CharClassMod>.LogInfo($"Started tracking event for {eventToTrack.GetUniqueIndex()} for player {playerUID}");
            }

            public bool IsTrackingEvent(EventToTrack eventToTrack)
            {
                if (!trackedEventTypes.ContainsKey(eventToTrack.GetUniqueIndex()))
                    return false;
                return trackedEventTypes[eventToTrack.GetUniqueIndex()];
            }

            public void StopTrackingEvent(EventToTrack eventToTrack)
            {
                if (!trackedEventTypes.ContainsKey(eventToTrack.GetUniqueIndex()))
                    trackedEventTypes[eventToTrack.GetUniqueIndex()] = false;

                trackedEventTypes[eventToTrack.GetUniqueIndex()] = false;
                LogUtils<CharClassMod>.LogInfo($"Stopped tracking event for {eventToTrack.GetUniqueIndex()} for player {playerUID}");
            }
        }

        internal class RecordedEventCounters
        {
            ConcurrentDictionary<string, decimal> eventCounter = new ConcurrentDictionary<string, decimal>();
            public void IncrementEvent(EventToTrack eventToTrack)
            {
                if (!eventCounter.ContainsKey(eventToTrack.GetUniqueIndex()))
                    eventCounter[eventToTrack.GetUniqueIndex()] = 0;

                eventCounter[eventToTrack.GetUniqueIndex()] = GetEventCount(eventToTrack) + 1;
                LogUtils<RecordedEventCounters>.LogInfo($"Incremented counter for {eventToTrack.GetUniqueIndex()} to {GetEventCount(eventToTrack)}");
            }

            public decimal GetEventCount(EventToTrack eventToTrack)
            {
                if (!eventCounter.ContainsKey(eventToTrack.GetUniqueIndex()))
                    eventCounter[eventToTrack.GetUniqueIndex()] = 0;

                return eventCounter[eventToTrack.GetUniqueIndex()];
            }
        }

        public class EventToTrack
        {
            public EventToTrack()
            {

            }

            public EventToTrack(TrackedEventEnum eventType, string uniqueRef)
            {
                this.EventType = eventType;
                this.UniqueRef = uniqueRef;
            }

            public string GetUniqueIndex()
            {
                return EventType.ToString() + "_" + UniqueRef;
            }

            public TrackedEventEnum EventType { get; set; }
            public string UniqueRef { get; set; }
        }

        public enum TrackedEventEnum
        {
            BreakBlock
        }

    }
}
