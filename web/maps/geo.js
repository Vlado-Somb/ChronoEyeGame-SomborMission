/* CHRONO EYE Maps: pure, framework-independent WGS84 helpers. */
(function (root, factory) {
  const api = factory();
  if (typeof module === "object" && module.exports) module.exports = api;
  root.ChronoMapsGeo = api;
})(typeof window !== "undefined" ? window : globalThis, function () {
  "use strict";
  function validLatLng(lat, lng) {
    return Number.isFinite(lat) && Number.isFinite(lng) && lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180;
  }
  function distanceMeters(a, b) {
    if (!validLatLng(a.lat, a.lng) || !validLatLng(b.lat, b.lng)) throw new TypeError("Invalid WGS84 coordinate");
    const r = Math.PI / 180, dLat = (b.lat - a.lat) * r, dLng = (b.lng - a.lng) * r;
    const h = Math.sin(dLat / 2) ** 2 + Math.cos(a.lat * r) * Math.cos(b.lat * r) * Math.sin(dLng / 2) ** 2;
    return 6371000 * 2 * Math.asin(Math.sqrt(Math.min(1, h)));
  }
  function zoneForDistance(distance, zones) {
    if (!Number.isFinite(distance) || distance < 0 || !zones) return "outside";
    if (distance <= zones.closeMeters) return "close";
    if (distance <= zones.approachMeters) return "approach";
    if (distance <= zones.outerMeters) return "outer";
    return "outside";
  }
  function normalizeWaypoint(w) {
    const lat = Number(w.position?.latitude), lng = Number(w.position?.longitude);
    if (!w.id || !validLatLng(lat, lng)) throw new Error("Invalid waypoint: " + (w.id || "(missing id)"));
    const z = w.zones || {};
    if (![z.closeMeters,z.approachMeters,z.outerMeters].every(v=>Number.isFinite(v) && v > 0) ||
        !(z.closeMeters <= z.approachMeters && z.approachMeters <= z.outerMeters))
      throw new Error("Invalid zones for " + w.id);
    return { ...w, lat, lng };
  }
  function indexData(data) {
    if (!data || !Array.isArray(data.waypoints) || !Array.isArray(data.missions)) throw new Error("Invalid map data");
    const points = data.waypoints.map(normalizeWaypoint);
    const byId = new Map(points.map(p => [p.id, p]));
    if (byId.size !== points.length) throw new Error("Duplicate waypoint ID");
    const missionIds = new Set();
    for (const mission of data.missions) {
      if (!mission.id || missionIds.has(mission.id)) throw new Error("Duplicate or empty mission ID");
      if (!byId.has(mission.primaryWaypointId)) throw new Error("Missing waypoint for mission " + mission.id);
      missionIds.add(mission.id);
    }
    return { points, byId, missions: data.missions };
  }
  function formatDistance(meters) {
    return meters >= 1000 ? (meters / 1000).toFixed(2) + " km" : Math.round(meters) + " m";
  }
  return Object.freeze({ validLatLng, distanceMeters, zoneForDistance, normalizeWaypoint, indexData, formatDistance });
});
