package rs.chronoeye.host

import android.util.AtomicFile
import org.json.JSONArray
import org.json.JSONObject
import java.io.File
import java.io.IOException

/** Use File(context.noBackupFilesDir, "chrono-session-v1.json"). Serial worker only. */
class AndroidSessionStore(file: File) : SessionEngine.Store {
    private val atomic = AtomicFile(file)
    override fun load(): SessionEngine.State? {
        if (!atomic.baseFile.exists() && !File(atomic.baseFile.path + ".bak").exists()) return null
        try {
            val j = JSONObject(atomic.openRead().use { it.readBytes().toString(Charsets.UTF_8) })
            require(j.getInt("schemaVersion") == 1) { "Unsupported session schema" }
            return SessionEngine.State().apply {
                sessionId=j.getString("sessionId"); gameId=j.getString("gameId")
                contentVersion=j.getString("contentVersion"); status=SessionEngine.Status.valueOf(j.getString("status"))
                activeMissionId=if(j.isNull("activeMissionId")) null else j.getString("activeMissionId")
                startedAt=j.getLong("startedAt"); savedAt=j.getLong("savedAt"); endedAt=j.getLong("endedAt")
                val a=j.getJSONObject("awards"); a.keys().forEach { awards[it]=a.getInt(it) }
                val tasks=j.getJSONArray("completedTasks"); for(i in 0 until tasks.length()) completedTasks.add(tasks.getString(i))
                val missions=j.getJSONArray("completedMissions"); for(i in 0 until missions.length()) completedMissions.add(missions.getString(i))
                if(!j.isNull("pending")) { val p=j.getJSONObject("pending")
                    pending=SessionEngine.Request(p.getString("sessionId"),p.getString("requestId"),p.getString("missionId"),p.getString("taskId")) }
            }
        } catch(e: Exception) { throw IOException("Cannot restore session; preserve file for recovery", e) }
    }
    override fun save(s: SessionEngine.State) {
        val j=JSONObject().put("schemaVersion",1).put("sessionId",s.sessionId).put("gameId",s.gameId)
            .put("contentVersion",s.contentVersion).put("status",s.status.name)
            .put("activeMissionId",s.activeMissionId ?: JSONObject.NULL)
            .put("startedAt",s.startedAt).put("savedAt",s.savedAt).put("endedAt",s.endedAt)
            .put("awards",JSONObject(s.awards)).put("completedTasks",JSONArray(s.completedTasks))
            .put("completedMissions",JSONArray(s.completedMissions))
        j.put("pending",s.pending?.let { p -> JSONObject().put("sessionId",p.sessionId)
            .put("requestId",p.requestId).put("missionId",p.missionId).put("taskId",p.taskId) } ?: JSONObject.NULL)
        val out=atomic.startWrite()
        try { out.write(j.toString().toByteArray(Charsets.UTF_8)); atomic.finishWrite(out) }
        catch(e: Exception) { atomic.failWrite(out); throw IOException("Session not saved",e) }
    }
}
