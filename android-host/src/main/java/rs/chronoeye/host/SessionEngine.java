package rs.chronoeye.host;

import java.io.IOException;
import java.util.*;

/** Single authority; call on a serial worker. Evidence adapters are native-only. */
public final class SessionEngine {
    public enum Status { ACTIVE, PAUSED, FINISHED }
    public enum Kind { GPS, AR, ANSWER }
    public interface Store {
        State load() throws IOException;
        void save(State state) throws IOException;
    }
    public static final class Task {
        public final String id;
        public final Kind kind;
        public final int points;
        public Task(String id, Kind kind, int points) {
            this.id = nonempty(id); this.kind = Objects.requireNonNull(kind);
            if (points < 0) throw new IllegalArgumentException("negative award");
            this.points = points;
        }
    }
    public static final class Mission {
        public final String id;
        public final List<Task> tasks;
        public final int bonus;
        public Mission(String id, List<Task> tasks, int bonus) {
            this.id = nonempty(id);
            if (tasks.isEmpty() || bonus < 0) throw new IllegalArgumentException("mission");
            Set<String> ids = new HashSet<>();
            for (Task t : tasks) if (!ids.add(t.id)) throw new IllegalArgumentException("duplicate task");
            this.tasks = Collections.unmodifiableList(new ArrayList<>(tasks)); this.bonus = bonus;
        }
    }
    public static final class Request {
        public final String sessionId, requestId, missionId, taskId;
        public Request(String sessionId, String requestId, String missionId, String taskId) {
            this.sessionId=nonempty(sessionId); this.requestId=nonempty(requestId);
            this.missionId=nonempty(missionId); this.taskId=nonempty(taskId);
        }
    }
    public static final class State {
        public String sessionId, gameId, contentVersion, activeMissionId;
        public Status status;
        public long startedAt, savedAt, endedAt;
        public Request pending;
        public final Map<String,Integer> awards = new LinkedHashMap<>();
        public final Set<String> completedTasks = new LinkedHashSet<>();
        public final Set<String> completedMissions = new LinkedHashSet<>();
        public int score() { int n=0; for(int v:awards.values()) n=Math.addExact(n,v); return n; }
        public State copy() {
            State s=new State(); s.sessionId=sessionId; s.gameId=gameId; s.contentVersion=contentVersion;
            s.activeMissionId=activeMissionId; s.status=status; s.startedAt=startedAt;
            s.savedAt=savedAt; s.endedAt=endedAt; s.pending=pending;
            s.awards.putAll(awards); s.completedTasks.addAll(completedTasks);
            s.completedMissions.addAll(completedMissions); return s;
        }
    }
    private final Store store;
    private final String gameId, version;
    private final Map<String,Mission> missions = new LinkedHashMap<>();
    private State state;
    public SessionEngine(Store store, String gameId, String version, List<Mission> missions) throws IOException {
        this.store=Objects.requireNonNull(store); this.gameId=nonempty(gameId); this.version=nonempty(version);
        for(Mission m:missions) if(this.missions.put(m.id,m)!=null) throw new IllegalArgumentException("duplicate mission");
        if(missions.isEmpty()) throw new IllegalArgumentException("empty game");
        state=store.load();
        if(state!=null) {
            if(!gameId.equals(state.gameId)||!version.equals(state.contentVersion))
                throw new IOException("Content version mismatch: explicit migration required");
            if(state.activeMissionId!=null && !this.missions.containsKey(state.activeMissionId))
                throw new IOException("Unknown saved mission");
            State recovered=state.copy(); recovered.pending=null;
            if(recovered.status!=Status.FINISHED) recovered.status=Status.PAUSED;
            commit(recovered, System.currentTimeMillis());
        }
    }
    public synchronized State snapshot() { return state==null?null:state.copy(); }
    public synchronized void start(long now) throws IOException {
        if(state!=null) throw new IllegalStateException("Session exists; resume instead");
        State s=new State(); s.sessionId=UUID.randomUUID().toString(); s.gameId=gameId;
        s.contentVersion=version; s.status=Status.ACTIVE; s.startedAt=now; commit(s,now);
    }
    public synchronized void pause(long now) throws IOException {
        active(); State s=state.copy(); s.status=Status.PAUSED; s.pending=null; commit(s,now);
    }
    public synchronized void resume(long now) throws IOException {
        if(state==null||state.status!=Status.PAUSED) throw new IllegalStateException("Not paused");
        State s=state.copy(); s.status=Status.ACTIVE; commit(s,now);
    }
    public synchronized void select(String id,long now) throws IOException {
        active(); if(!missions.containsKey(id)) throw new IllegalArgumentException("Unknown mission");
        if(state.pending!=null) throw new IllegalStateException("AR request pending");
        State s=state.copy(); s.activeMissionId=id; commit(s,now);
    }
    public synchronized Request beginAr(long now) throws IOException {
        active(); Task t=current();
        if(t==null||t.kind!=Kind.AR||state.pending!=null) throw new IllegalStateException("AR not ready");
        State s=state.copy(); s.pending=new Request(s.sessionId,UUID.randomUUID().toString(),s.activeMissionId,t.id);
        commit(s,now); return s.pending;
    }
    /** validatedSuccess comes from a task-specific native validator, never from raw web JSON. */
    public synchronized boolean arResult(Request result, boolean validatedSuccess,long now) throws IOException {
        active(); Request p=state.pending;
        if(p==null||result==null||!p.sessionId.equals(result.sessionId)||!p.requestId.equals(result.requestId)
            ||!p.missionId.equals(result.missionId)||!p.taskId.equals(result.taskId)) return false;
        State s=state.copy(); s.pending=null;
        if(validatedSuccess) award(s,current());
        commit(s,now); return true;
    }
    /** Called only after native GPS dwell / answer validation, not by map bridge. */
    public synchronized boolean completeValidated(String missionId,String taskId,Kind kind,long now) throws IOException {
        active(); if(kind==Kind.AR || state.pending!=null || !Objects.equals(missionId,state.activeMissionId)) return false;
        Task t=current(); if(t==null||!t.id.equals(taskId)||t.kind!=kind) return false;
        State s=state.copy(); award(s,t); commit(s,now); return true;
    }
    public synchronized void finish(long now) throws IOException {
        active(); if(state.completedMissions.size()!=missions.size()) throw new IllegalStateException("Incomplete game");
        State s=state.copy(); s.status=Status.FINISHED; s.endedAt=now; commit(s,now);
    }
    private Task current() {
        Mission m=missions.get(state.activeMissionId);
        if(m==null) throw new IllegalStateException("Select mission first");
        for(Task t:m.tasks) if(!state.completedTasks.contains(key(m.id,t.id))) return t;
        return null;
    }
    private void award(State s,Task t) {
        if(t==null) throw new IllegalStateException("No task");
        Mission m=missions.get(s.activeMissionId); String key=key(m.id,t.id);
        s.completedTasks.add(key); s.awards.putIfAbsent("task:"+key,t.points);
        boolean done=true; for(Task task:m.tasks) done &= s.completedTasks.contains(key(m.id,task.id));
        if(done) { s.completedMissions.add(m.id); s.awards.putIfAbsent("mission:"+m.id,m.bonus); }
        s.score(); // detect overflow before persistence
    }
    private void active() { if(state==null||state.status!=Status.ACTIVE) throw new IllegalStateException("Session inactive"); }
    private void commit(State next,long now) throws IOException {
        next.savedAt=now; store.save(next.copy()); state=next;
    }
    private static String key(String m,String t) { return m.length()+":"+m+t; }
    private static String nonempty(String s) {
        if(s==null||s.isEmpty()) throw new IllegalArgumentException("Empty ID"); return s;
    }
}
