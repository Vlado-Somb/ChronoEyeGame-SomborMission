/* Public configuration only. Create a NEW restricted browser key for the CHRONO EYE host.
   Do not paste a server key, service-account credential or Heritage Map's existing key here.
   Empty key deliberately enables the offline coordinate-preview mode. */
window.CHRONO_MAPS_CONFIG = {
  googleMapsApiKey: "",
  googleMapsLanguage: "sr",
  googleMapsRegion: "RS",
  gameDataRoot: "../../game-data/sombor/v1",
  snapshotUrl: "./data/sombor-demo.json",
  defaultCenter: { lat: 45.7727, lng: 19.1143 },
  defaultZoom: 16,
  maxLocationAgeMs: 120000
};
