window.coordPicker = (function () {
    var map = null;
    var marker = null;
    var dotnetRef = null;

    return {
        init: function (elementId, lat, lon, ref) {
            dotnetRef = ref;

            if (map) {
                map.remove();
                map = null;
                marker = null;
            }

            var hasPos = (lat != null && lon != null);
            var center = hasPos ? [lat, lon] : [45.5, 9.2];
            var zoom   = hasPos ? 13 : 6;

            map = L.map(elementId).setView(center, zoom);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19,
                attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            }).addTo(map);

            if (hasPos) {
                marker = L.marker([lat, lon]).addTo(map);
            }

            map.on('click', function (e) {
                var ll = e.latlng;
                if (marker) {
                    marker.setLatLng(ll);
                } else {
                    marker = L.marker(ll).addTo(map);
                }
                dotnetRef.invokeMethodAsync('OnMapClick', ll.lat, ll.lng);
            });

            // ensure tiles render in hidden containers
            setTimeout(function () { if (map) map.invalidateSize(); }, 200);
        },

        setMarker: function (lat, lon) {
            if (!map) return;
            if (marker) {
                marker.setLatLng([lat, lon]);
            } else {
                marker = L.marker([lat, lon]).addTo(map);
            }
            map.setView([lat, lon], Math.max(map.getZoom(), 13));
        },

        dispose: function () {
            if (map) {
                map.remove();
                map = null;
                marker = null;
            }
        }
    };
})();
