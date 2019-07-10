using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager {
    Queue<Event> events = new Queue<Event>();

    public void AddEvent(Event e) {
        this.events.Enqueue(e);
    }

    public Event GetNext() {
        return this.events.Dequeue();
    }

    public bool HasNext() {
        return this.events.Count > 0;
    }
}

public class Event {
    public string name;

    public Event(string name) {
        this.name = name;
    }
}

public class TileClickedEvent : Event {
    public Vector2Int pos;    
    public TileClickedEvent(string name, Vector2Int pos) : base(name) {
        this.pos = pos;
    }
}