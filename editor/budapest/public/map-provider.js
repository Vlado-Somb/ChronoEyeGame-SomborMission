/* Map-provider adapter: editorial coordinates stay WGS84; no Google credentials. */
(() => {
  'use strict';
  const TILE_URL = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
  const ATTRIBUTION = '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener noreferrer">OpenStreetMap contributors</a>';
  const osm = Object.freeze({
    id: 'osm',
    tileUrl: TILE_URL,
    attribution: ATTRIBUTION,
    create(element, center, zoom, leaflet = window.L) {
      if (!leaflet) throw Error('Leaflet is not available; check network access or CDN.');
      const map = leaflet.map(element).setView(center, zoom);
      leaflet.tileLayer(TILE_URL, { maxZoom: 19, attribution: ATTRIBUTION }).addTo(map);
      const pins = leaflet.layerGroup().addTo(map);
      return Object.freeze({
        onClick(handler) { map.on('click', e => handler(e.latlng)); },
        clearPins() { pins.clearLayers(); },
        addPin({ latitude, longitude, label, onClick, onDragStart, onDragEnd }) {
          const marker = leaflet.marker([latitude, longitude], { draggable: true }).addTo(pins);
          marker.bindTooltip(label);
          marker.on('click', () => onClick?.());
          marker.on('dragstart', () => onDragStart?.());
          marker.on('dragend', e => onDragEnd?.(e.target.getLatLng()));
        },
        resize() { map.invalidateSize(); },
        destroy() { map.remove(); }
      });
    }
  });
  const providers = Object.freeze({ osm });
  window.BudapestMapProviders = Object.freeze({
    available: Object.freeze(Object.keys(providers)),
    osm,
    create(id, element, center, zoom, leaflet) {
      if (!Object.hasOwn(providers, id)) throw Error('Map provider not configured: ' + id);
      return providers[id].create(element, center, zoom, leaflet);
    }
  });
})();
