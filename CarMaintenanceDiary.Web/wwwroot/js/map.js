let leafletMapInstance;

window.initFuelMap = (stations, lat, lng, radiusMeters) => {
    console.log("initFuelMap called with:", stations, lat, lng, radiusMeters);

    const map = L.map('map').setView([lat, lng], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    // 🧍 User location marker
    L.marker([lat, lng], {
        icon: L.icon({
            iconUrl: 'https://cdn-icons-png.flaticon.com/512/64/64113.png',
            iconSize: [32, 32],
            iconAnchor: [16, 32],
            popupAnchor: [0, -32]
        })
    }).addTo(map).bindPopup("You are here").openPopup();

    // 🔵 Circle showing search radius
    const circle = L.circle([lat, lng], {
        radius: radiusMeters,
        color: 'transparent',   // stroke color invisible
        fillOpacity: 0          // fill invisible
    }).addTo(map);

    // ⛽ Fuel stations
    stations.forEach(station => {
        L.marker([station.latitude, station.longitude])
            .addTo(map)
            .bindPopup(station.name || "Unnamed Station");
    });

    // 📦 Fit map to circle bounds
    map.fitBounds(circle.getBounds(), { padding: [20, 20] });
};