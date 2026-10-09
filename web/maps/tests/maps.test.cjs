const test=require("node:test");
const assert=require("node:assert/strict");
const fs=require("node:fs");
const path=require("node:path");
const geo=require("../geo.js");
const data=JSON.parse(fs.readFileSync(path.join(__dirname,"../data/sombor-demo.json"),"utf8"));
test("candidate snapshot has valid linked missions and WGS84 points",()=>{
  const result=geo.indexData(data);
  assert.equal(result.points.length,13);
  assert.equal(result.missions.length,9);
  assert.equal(result.points.filter(p=>p.id==="SO_GLOBAL").length,1);
  assert.ok(result.missions.every(m=>result.byId.has(m.primaryWaypointId)));
});
test("Haversine distance is symmetric and self-distance is zero",()=>{
  const a={lat:45.7724780193,lng:19.1138807951},b={lat:45.7721560619,lng:19.1137769085};
  assert.equal(geo.distanceMeters(a,a),0);
  assert.ok(geo.distanceMeters(a,b)>30);
  assert.ok(Math.abs(geo.distanceMeters(a,b)-geo.distanceMeters(b,a))<1e-8);
});
test("zone thresholds are inclusive, ordered and display-only",()=>{
  const z={closeMeters:7,approachMeters:21,outerMeters:75};
  assert.equal(geo.zoneForDistance(7,z),"close");
  assert.equal(geo.zoneForDistance(8,z),"approach");
  assert.equal(geo.zoneForDistance(21,z),"approach");
  assert.equal(geo.zoneForDistance(22,z),"outer");
  assert.equal(geo.zoneForDistance(76,z),"outside");
});
test("rejects invalid WGS84 points and broken mission references",()=>{
  assert.throws(()=>geo.normalizeWaypoint({id:"x",position:{latitude:200,longitude:20},zones:{outerMeters:50,approachMeters:20,closeMeters:5}}));
  assert.throws(()=>geo.indexData({waypoints:data.waypoints,missions:[{id:"bad",primaryWaypointId:"MISSING"}]}));
});
