package rs.chronoeye.host;
import java.io.IOException;
import java.util.*;

public final class SessionEngineTest {
    static class Memory implements SessionEngine.Store {
        SessionEngine.State state; boolean fail;
        public SessionEngine.State load() {return state==null?null:state.copy();}
        public void save(SessionEngine.State s) throws IOException {if(fail)throw new IOException("disk full");state=s.copy();}
    }
    static List<SessionEngine.Mission> content() {return Arrays.asList(new SessionEngine.Mission("m", Arrays.asList(
        new SessionEngine.Task("gps",SessionEngine.Kind.GPS,10),
        new SessionEngine.Task("ar",SessionEngine.Kind.AR,10),
        new SessionEngine.Task("quiz",SessionEngine.Kind.ANSWER,10)),30));}
    static void check(boolean b) {if(!b)throw new AssertionError();}
    public static void main(String[] args) throws Exception {
        Memory store=new Memory(); SessionEngine e=new SessionEngine(store,"sombor","test-1",content());
        e.start(100); e.select("m",101);
        check(!e.completeValidated("m","quiz",SessionEngine.Kind.ANSWER,102));
        check(e.completeValidated("m","gps",SessionEngine.Kind.GPS,103));
        check(!e.completeValidated("m","gps",SessionEngine.Kind.GPS,104));
        SessionEngine.Request r=e.beginAr(105);
        check(!e.arResult(new SessionEngine.Request("wrong",r.requestId,"m","ar"),true,106));
        check(e.arResult(r,false,107)); check(e.snapshot().score()==10);
        r=e.beginAr(108); check(e.arResult(r,true,109)); check(!e.arResult(r,true,110));
        store.fail=true;
        try {e.completeValidated("m","quiz",SessionEngine.Kind.ANSWER,111);throw new AssertionError();}
        catch(IOException expected) {check(e.snapshot().score()==20);}
        store.fail=false; check(e.completeValidated("m","quiz",SessionEngine.Kind.ANSWER,112));
        check(e.snapshot().score()==60);
        e.snapshot().awards.clear(); check(e.snapshot().score()==60);
        SessionEngine restored=new SessionEngine(store,"sombor","test-1",content());
        check(restored.snapshot().status==SessionEngine.Status.PAUSED);
        check(restored.snapshot().startedAt==100); restored.resume(113);
        check(!restored.completeValidated("m","quiz",SessionEngine.Kind.ANSWER,114));
        restored.finish(115);check(restored.snapshot().endedAt==115);
        try {new SessionEngine(store,"sombor","test-2",content());throw new AssertionError();}
        catch(IOException expected) {}
        Memory interrupted=new Memory(); e=new SessionEngine(interrupted,"sombor","test-1",content());
        e.start(200);e.select("m",201);e.completeValidated("m","gps",SessionEngine.Kind.GPS,202);
        r=e.beginAr(203);e=new SessionEngine(interrupted,"sombor","test-1",content());e.resume(204);
        check(!e.arResult(r,true,205));check(e.snapshot().score()==10);
        System.out.println("PASS: ordering, duplicate awards, AR correlation/cancel, failed save rollback, defensive snapshot, restore, version gate, process interruption");
    }
}
