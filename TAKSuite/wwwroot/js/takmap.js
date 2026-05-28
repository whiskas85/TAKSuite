window.takMap = (function () {
    'use strict';

    var _map = null;
    var _userMarkers = {};
    var _missionMarkers = [];
    var _milModifierLabels = [];
    var _shapeMarkers = [];
    var _shapeLabelMarkers = [];
    var _baseLayers = {};
    var _storageKey = null;
    var _currentLayerId = null;
    var MIL_LABEL_MIN_ZOOM = 15;
    var _lcPanel = null;
    var _lcBtn = null;
    var _lcOpen = false;
    var _lcCssInjected = false;
    var _lcControl = null;
    var _customLayerDefs = [];
    var _routeMarkers = [];
    var _routeWpMarkers = [];

    var LAYERS = [
        { id: 'osm',           cat: 'OSM',       name: 'OpenStreetMap',           url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', attr: '© OpenStreetMap', maxZoom: 19, dflt: true },
        { id: 'otopo',         cat: 'OSM',       name: 'OpenTopoMap',             url: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', attr: '© OpenTopoMap', maxZoom: 17 },
        { id: 'cycle',         cat: 'OSM',       name: 'CycleOSM',               url: 'https://a.tile-cyclosm.openstreetmap.fr/cyclosm/{z}/{x}/{y}.png', attr: '© OpenStreetMap', maxZoom: 21 },
        { id: 'esri-sat',      cat: 'Esri',      name: 'Satellite',              url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', attr: '© Esri', maxZoom: 19 },
        { id: 'esri-clarity',  cat: 'Esri',      name: 'Clarity',                url: 'https://clarity.maptiles.arcgis.com/arcgis/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', attr: '© Esri', maxZoom: 20 },
        { id: 'esri-topo',     cat: 'Esri',      name: 'Topo',                   url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/{z}/{y}/{x}', attr: '© Esri', maxZoom: 19 },
        { id: 'esri-natgeo',   cat: 'Esri',      name: 'NatGeo',                 url: 'https://server.arcgisonline.com/ArcGIS/rest/services/NatGeo_World_Map/MapServer/tile/{z}/{y}/{x}', attr: '© Esri', maxZoom: 16 },
        { id: 'esri-usatopo',  cat: 'Esri',      name: 'USA Topo',               url: 'https://server.arcgisonline.com/ArcGIS/rest/services/USA_Topo_Maps/MapServer/tile/{z}/{y}/{x}', attr: '© Esri', maxZoom: 15 },
        { id: 'google-road',   cat: 'Google',    name: 'Roadmap',                url: 'https://mt1.google.com/vt/lyrs=m&x={x}&y={y}&z={z}', attr: '© Google', maxZoom: 20 },
        { id: 'google-sat',    cat: 'Google',    name: 'Satellite',              url: 'https://mt1.google.com/vt/lyrs=s&x={x}&y={y}&z={z}', attr: '© Google', maxZoom: 20 },
        { id: 'google-hybrid', cat: 'Google',    name: 'Hybrid',                 url: 'https://mt1.google.com/vt/lyrs=y&x={x}&y={y}&z={z}', attr: '© Google', maxZoom: 20 },
        { id: 'google-terrain',cat: 'Google',    name: 'Terrain',                url: 'https://mt1.google.com/vt/lyrs=p&x={x}&y={y}&z={z}', attr: '© Google', maxZoom: 20 },
        { id: 'usgs-topo',     cat: 'USGS',      name: 'Topo',                   url: 'https://basemap.nationalmap.gov/ArcGIS/rest/services/USGSTopo/MapServer/tile/{z}/{y}/{x}', attr: '© USGS', maxZoom: 15 },
        { id: 'usgs-sat',      cat: 'USGS',      name: 'Imagery',                url: 'https://basemap.nationalmap.gov/ArcGIS/rest/services/USGSImageryOnly/MapServer/tile/{z}/{y}/{x}', attr: '© USGS', maxZoom: 15 },
        { id: 'usgs-imgtopo',  cat: 'USGS',      name: 'Imagery + Topo',         url: 'https://basemap.nationalmap.gov/ArcGIS/rest/services/USGSImageryTopo/MapServer/tile/{z}/{y}/{x}', attr: '© USGS', maxZoom: 15 },
        { id: 'usgs-relief',   cat: 'USGS',      name: 'Shaded Relief',          url: 'https://basemap.nationalmap.gov/arcgis/rest/services/USGSShadedReliefOnly/MapServer/tile/{z}/{y}/{x}', attr: '© USGS', maxZoom: 15 },
        { id: 'naip',          cat: 'USGS',      name: 'NAIP USA Imagery',       url: 'https://gis.apfo.usda.gov/arcgis/rest/services/NAIP/USDA_CONUS_PRIME/ImageServer/tile/{z}/{y}/{x}', attr: '© USDA', maxZoom: 17 },
        { id: 'carto-dark',    cat: 'Carto',     name: 'Dark',                   url: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', attr: '© CartoDB', maxZoom: 19 },
        { id: 'carto-light',   cat: 'Carto',     name: 'Light',                  url: 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', attr: '© CartoDB', maxZoom: 19 },
        { id: 'openseamap',    cat: 'Marittimo', name: 'OpenSeaMap',             url: 'https://tiles.openseamap.org/seamark/{z}/{x}/{y}.png', attr: '© OpenSeaMap', maxZoom: 18 },
        { id: 'pl-orto',       cat: 'Regionale', name: 'Polonia Ortofoto',       url: 'https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMTS/StandardResolution?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ORTOFOTOMAPA&STYLE=default&TILEMATRIXSET=EPSG:3857&TILEMATRIX={z}&TILEROW={y}&TILECOL={x}&FORMAT=image/jpeg', attr: '© Geoportal Poland', maxZoom: 20 },
        { id: 'canada-cbmt',   cat: 'Regionale', name: 'Canada Base Map',        wms: true, url: 'https://maps.geogratis.gc.ca/wms/CBMT', layers: 'CBMT', attr: '© Natural Resources Canada', maxZoom: 17 },
        { id: 'canada-topo',   cat: 'Regionale', name: 'Canada Toporama',        wms: true, url: 'https://maps.geogratis.gc.ca/wms/toporama_en', layers: 'WMS-Toporama', attr: '© Natural Resources Canada', maxZoom: 17 },
        { id: 'de-basemap',    cat: 'Regionale', name: 'basemap.de Colore',      wms: true, url: 'https://sgx.geodatenzentrum.de/wms_basemapde', layers: 'de_basemapde_web_raster_farbe', attr: '© GeoBasis-DE/BKG', maxZoom: 19 },
        { id: 'de-grey',       cat: 'Regionale', name: 'basemap.de Grigio',      wms: true, url: 'https://sgx.geodatenzentrum.de/wms_basemapde', layers: 'de_basemapde_web_raster_grau', attr: '© GeoBasis-DE/BKG', maxZoom: 19 },
        { id: 'fema-flood',    cat: 'Regionale', name: 'FEMA Zone Alluvionali',  wms: true, url: 'https://hazards.fema.gov/arcgis/services/public/NFHLWMS/MapServer/WMSServer', layers: '12', attr: '© FEMA', maxZoom: 19, transparent: true }
    ];

    function createUserIcon(color, isStale) {
        var teamColor = color || '#6c757d';
        var bg     = isStale ? '#888888' : teamColor;
        var border = isStale ? teamColor  : 'rgba(0,0,0,0.35)';
        var bw     = isStale ? '3px'      : '2.5px';
        var html = '<div style="width:24px;height:24px;border-radius:50%;background:' + bg +
            ';border:' + bw + ' solid ' + border + ';box-shadow:0 2px 5px rgba(0,0,0,0.4)"></div>';
        return L.divIcon({ html: html, className: '', iconSize: [24, 24], iconAnchor: [12, 12], popupAnchor: [0, -16] });
    }

    // cotType → SIDC base (usato se __milsym non è presente nel CoT)
    function cotTypeToSidc(cotType) {
        if (!cotType) return 'SUGP-----------';
        var parts = cotType.split('-');
        if (parts.length < 3 || parts[0] !== 'a') return 'SUGP-----------';
        var affMap = { 'f': 'F', 'h': 'H', 'n': 'N', 'u': 'U', 'a': 'A', 'j': 'J', 'k': 'K' };
        var dimMap = { 'G': 'G', 'A': 'A', 'S': 'S', 'U': 'U', 'F': 'F' };
        var aff = affMap[parts[1]] || 'U';
        var dim = dimMap[parts[2]] || 'G';
        return 'S' + aff + dim + 'P-----------';
    }

    function createMilSymIcon(sidc, cotType) {
        if (typeof ms === 'undefined') return null;
        try {
            var code = (sidc && sidc.length >= 10) ? sidc : cotTypeToSidc(cotType);
            var symbol = new ms.Symbol(code, { size: 16 });
            var dataUrl = symbol.toDataURL();
            if (!dataUrl || dataUrl.length < 20) return null;
            return L.icon({ iconUrl: dataUrl, iconSize: [26, 26], iconAnchor: [13, 13], popupAnchor: [0, -15] });
        } catch (e) { return null; }
    }

    function updateMilModifierVisibility() {
        if (!_map) return;
        var visible = _map.getZoom() >= MIL_LABEL_MIN_ZOOM;
        var opacity = visible ? '1' : '0';
        _milModifierLabels.forEach(function (tt) {
            var el = tt.getElement();
            if (el) { el.style.transition = 'opacity 0.35s ease'; el.style.opacity = opacity; }
        });
        _shapeLabelMarkers.forEach(function (m) {
            var el = m.getElement();
            if (el) { el.style.transition = 'opacity 0.35s ease'; el.style.opacity = opacity; }
        });
    }

    function addMilModifierLabels(latlng, modifiers, store) {
        if (!modifiers) return;
        var v = function (code) { return (modifiers[code] || '').trim(); };
        var sp = '<div style="height:6px"></div>';

        // SINISTRA: Y (location) · spazio · T (unique desig) · Z (speed)
        var left = '';
        if (v('Y')) left += '<div>' + v('Y') + '</div>';
        if (v('Y') && (v('T') || v('Z'))) left += sp;
        if (v('T')) left += '<div>' + v('T') + '</div>';
        if (v('Z')) left += '<div>' + v('Z') + '</div>';
        if (left) {
            var ttL = L.tooltip({ permanent: true, direction: 'left', offset: [-17, 10], className: 'takmap-label-mil' })
                .setLatLng(latlng).setContent('<div style="text-align:right">' + left + '</div>').addTo(_map);
            store.push(ttL);
            _milModifierLabels.push(ttL);
        }

        // DESTRA: G (staff comments) · H (additional info) · spazio · J[2]+P[5]
        var right = '';
        if (v('G')) right += '<div>' + v('G') + '</div>';
        if (v('H')) right += '<div>' + v('H') + '</div>';
        var jt = v('J').substring(0, 2);
        var pt = v('P').substring(0, 5);
        var jp = (jt + ' ' + pt).trim();
        if (jp) {
            if (right) right += sp;
            right += '<div>' + jp + '</div>';
        }
        if (right) {
            var ttR = L.tooltip({ permanent: true, direction: 'right', offset: [17, 10], className: 'takmap-label-mil' })
                .setLatLng(latlng).setContent('<div style="text-align:left">' + right + '</div>').addTo(_map);
            store.push(ttR);
            _milModifierLabels.push(ttR);
        }
        updateMilModifierVisibility();
    }

    function createWaypointIcon(color) {
        var bg = color || '#0d6efd';
        var html = '<div style="width:10px;height:10px;border-radius:50%;background:' + bg + ';border:1.5px solid rgba(255,255,255,0.8);box-shadow:0 1px 3px rgba(0,0,0,0.5)"></div>';
        return L.divIcon({ html: html, className: '', iconSize: [10, 10], iconAnchor: [5, 5], popupAnchor: [0, -8] });
    }

    function createParkingIcon(color) {
        var bg = color || '#fd7e14';
        var r = parseInt(bg.slice(1, 3), 16) || 0;
        var g = parseInt(bg.slice(3, 5), 16) || 0;
        var b = parseInt(bg.slice(5, 7), 16) || 0;
        var lum = (r * 299 + g * 587 + b * 114) / 1000;
        var textColor = lum > 140 ? '#000' : '#fff';
        var html = '<div style="width:22px;height:22px;border-radius:4px;background:' + bg + ';border:2px solid rgba(0,0,0,0.55);box-shadow:0 1px 4px rgba(0,0,0,0.5);display:flex;align-items:center;justify-content:center;font-weight:700;font-size:13px;color:' + textColor + ';line-height:1">P</div>';
        return L.divIcon({ html: html, className: '', iconSize: [22, 22], iconAnchor: [11, 11], popupAnchor: [0, -14] });
    }

    function formatLastSec(sec) {
        if (sec == null) return '?';
        if (sec < 60) return Math.round(sec) + 's fa';
        if (sec < 3600) return Math.round(sec / 60) + 'm fa';
        return Math.round(sec / 3600) + 'h fa';
    }

    function staleStyle(sec) {
        if (sec < 120) return 'color:#198754';
        if (sec < 600) return 'color:#ffc107';
        return 'color:#dc3545';
    }

    function saveView() {
        if (!_map || !_storageKey) return;
        var c = _map.getCenter();
        try { localStorage.setItem(_storageKey, JSON.stringify({ lat: c.lat, lng: c.lng, zoom: _map.getZoom(), layerId: _currentLayerId })); } catch (e) {}
    }

    function injectLayerControlCss() {
        if (_lcCssInjected) return;
        _lcCssInjected = true;
        var style = document.createElement('style');
        style.textContent = [
            '.tm-lc-wrap{position:relative}',
            '.tm-lc-btn{width:34px;height:34px;background:#fff;border:2px solid rgba(0,0,0,.2);border-radius:5px;box-shadow:0 1px 5px rgba(0,0,0,.3);cursor:pointer;display:flex;align-items:center;justify-content:center;padding:0;color:#555;transition:background .15s}',
            '.tm-lc-btn:hover{background:#f0f0f0;color:#222}',
            '.tm-lc-btn svg{display:block}',
            '.tm-lc-panel{position:absolute;top:38px;right:0;background:#fff;border:1px solid rgba(0,0,0,.18);border-radius:8px;box-shadow:0 4px 16px rgba(0,0,0,.18);min-width:200px;max-height:65vh;overflow-y:auto;padding:6px 0;z-index:10000}',
            '.tm-lc-sep{height:1px;background:rgba(0,0,0,.08);margin:4px 0}',
            '.tm-lc-cat{padding:5px 12px 2px;font-size:10px;font-weight:700;text-transform:uppercase;letter-spacing:.07em;color:#999;pointer-events:none}',
            '.tm-lc-item{display:flex;align-items:center;padding:5px 14px 5px 10px;cursor:pointer;font-size:13px;color:#333;gap:8px;border-radius:0;transition:background .1s;white-space:nowrap}',
            '.tm-lc-item:hover{background:#eef3ff}',
            '.tm-lc-item.active{color:#0d6efd;font-weight:600}',
            '.tm-lc-check{width:14px;text-align:center;font-size:12px;color:#0d6efd;flex-shrink:0}'
        ].join('');
        document.head.appendChild(style);
    }

    function buildLayerControl(map) {
        var LC = L.Control.extend({
            options: { position: 'topright' },
            onAdd: function() {
                var wrap = L.DomUtil.create('div', 'tm-lc-wrap');
                L.DomEvent.disableClickPropagation(wrap);
                L.DomEvent.disableScrollPropagation(wrap);

                _lcBtn = L.DomUtil.create('button', 'tm-lc-btn', wrap);
                _lcBtn.title = 'Cambia layer base';
                _lcBtn.innerHTML = '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="12 2 2 7 12 12 22 7 12 2"/><polyline points="2 17 12 22 22 17"/><polyline points="2 12 12 17 22 12"/></svg>';

                _lcPanel = L.DomUtil.create('div', 'tm-lc-panel', wrap);
                _lcPanel.style.display = 'none';

                // Merge built-in LAYERS + custom layers into one grouped list
                var allDefs = LAYERS.slice();
                _customLayerDefs.forEach(function(cl) { allDefs.push(cl); });

                var cats = []; var catMap = {};
                allDefs.forEach(function(ld) {
                    var c = ld.cat || 'Altro';
                    if (!catMap[c]) { catMap[c] = []; cats.push(c); }
                    catMap[c].push(ld);
                });

                function activateLayer(ld) {
                    Object.keys(_baseLayers).forEach(function(n) {
                        if (map.hasLayer(_baseLayers[n])) map.removeLayer(_baseLayers[n]);
                    });
                    _baseLayers[ld.name].addTo(map);
                    _currentLayerId = ld.id;
                    saveView();
                    _lcPanel.querySelectorAll('.tm-lc-item').forEach(function(el) {
                        el.classList.remove('active');
                        el.querySelector('.tm-lc-check').textContent = '';
                    });
                }

                cats.forEach(function(cat, ci) {
                    if (ci > 0) { L.DomUtil.create('div', 'tm-lc-sep', _lcPanel); }
                    var catEl = L.DomUtil.create('div', 'tm-lc-cat', _lcPanel);
                    catEl.textContent = cat;

                    catMap[cat].forEach(function(ld) {
                        var item = L.DomUtil.create('div', 'tm-lc-item', _lcPanel);
                        var check = L.DomUtil.create('span', 'tm-lc-check', item);
                        var label = L.DomUtil.create('span', '', item);
                        label.textContent = ld.name;

                        if (_currentLayerId === ld.id) {
                            item.classList.add('active');
                            check.textContent = '✓';
                        }

                        L.DomEvent.on(item, 'click', function() {
                            activateLayer(ld);
                            item.classList.add('active');
                            check.textContent = '✓';
                            _lcPanel.style.display = 'none';
                            _lcOpen = false;
                        });
                    });
                });

                L.DomEvent.on(_lcBtn, 'click', function(e) {
                    _lcOpen = !_lcOpen;
                    _lcPanel.style.display = _lcOpen ? 'block' : 'none';
                    L.DomEvent.stopPropagation(e);
                });

                // Close on outside click
                document.addEventListener('click', function() {
                    if (_lcOpen && _lcPanel) { _lcPanel.style.display = 'none'; _lcOpen = false; }
                });

                return wrap;
            }
        });
        return new LC();
    }

    function latLngToUTM(lat, lng) {
        var a  = 6378137.0;
        var f  = 1 / 298.257223563;
        var b  = a * (1 - f);
        var e2 = 1 - (b / a) * (b / a);
        var k0 = 0.9996;

        var zone = Math.floor((lng + 180) / 6) + 1;
        var lon0 = (zone - 1) * 6 - 180 + 3;

        var latR  = lat  * Math.PI / 180;
        var dLon  = (lng - lon0) * Math.PI / 180;
        var sinLat = Math.sin(latR);
        var cosLat = Math.cos(latR);
        var tanLat = Math.tan(latR);

        var N  = a / Math.sqrt(1 - e2 * sinLat * sinLat);
        var T  = tanLat * tanLat;
        var C  = (e2 / (1 - e2)) * cosLat * cosLat;
        var A  = cosLat * dLon;
        var A2 = A * A; var A3 = A2 * A; var A4 = A3 * A; var A5 = A4 * A; var A6 = A5 * A;

        var e4 = e2 * e2; var e6 = e4 * e2;
        var ep2 = e2 / (1 - e2);
        var M = a * (
            (1 - e2/4 - 3*e4/64 - 5*e6/256)   * latR
          - (3*e2/8 + 3*e4/32 + 45*e6/1024)   * Math.sin(2*latR)
          + (15*e4/256 + 45*e6/1024)           * Math.sin(4*latR)
          - (35*e6/3072)                        * Math.sin(6*latR)
        );

        var easting = k0 * N * (
            A + (1 - T + C) * A3/6 + (5 - 18*T + T*T + 72*C - 58*ep2) * A5/120
        ) + 500000;

        var northing = k0 * (
            M + N * tanLat * (
                A2/2
              + (5 - T + 9*C + 4*C*C) * A4/24
              + (61 - 58*T + T*T + 600*C - 330*ep2) * A6/720
            )
        );
        if (lat < 0) northing += 10000000;

        var bands = 'CDEFGHJKLMNPQRSTUVWX';
        var band  = bands[Math.min(19, Math.max(0, Math.floor((lat + 80) / 8)))];

        return { zone: zone, band: band, easting: Math.round(easting), northing: Math.round(northing) };
    }

    return {
        init: function (elementId, storageKey) {
            if (_map) { _map.remove(); _map = null; }
            _userMarkers = {};
            _missionMarkers = [];
            _milModifierLabels = [];
            _shapeMarkers = [];
            _shapeLabelMarkers = [];
            _baseLayers = {};
            _customLayerDefs = [];
            _lcControl = null;
            _routeMarkers = [];
            _routeWpMarkers = [];
            _storageKey = storageKey || null;
            _currentLayerId = null;

            var initView = [45.5, 9.2];
            var initZoom = 6;
            var savedLayerId = null;
            if (_storageKey) {
                try {
                    var saved = localStorage.getItem(_storageKey);
                    if (saved) { var sv = JSON.parse(saved); initView = [sv.lat, sv.lng]; initZoom = sv.zoom; savedLayerId = sv.layerId || null; }
                } catch (e) {}
            }

            _map = L.map(elementId, { zoomControl: true }).setView(initView, initZoom);
            _map.createPane('routePane');
            _map.getPane('routePane').style.zIndex = 390;

            // UTM coordinate display
            var UtmControl = L.Control.extend({
                onAdd: function () {
                    var div = L.DomUtil.create('div', 'takmap-utm-box');
                    div.innerHTML = '<span class="takmap-utm-label">UTM</span><span class="takmap-utm-val">—</span>';
                    L.DomEvent.disableClickPropagation(div);
                    return div;
                }
            });
            new UtmControl({ position: 'bottomleft' }).addTo(_map);

            _map.on('mousemove', function (e) {
                var utm = latLngToUTM(e.latlng.lat, e.latlng.lng);
                var el = _map.getContainer().querySelector('.takmap-utm-val');
                if (el) el.textContent = utm.zone + utm.band + '  ' + utm.easting + ' E  ' + utm.northing + ' N';
            });
            _map.on('mouseout', function () {
                var el = _map.getContainer().querySelector('.takmap-utm-val');
                if (el) el.textContent = '—';
            });

            if (_storageKey) {
                _map.on('moveend zoomend', saveView);
            }
            _map.on('zoomend', updateMilModifierVisibility);
            _map.on('baselayerchange', function (e) {
                var ld = LAYERS.filter(function (l) { return l.name === e.name; })[0];
                if (ld) _currentLayerId = ld.id;
                saveView();
            });

            LAYERS.forEach(function (ld) {
                var layer;
                if (ld.wms) {
                    layer = L.tileLayer.wms(ld.url, { layers: ld.layers, format: 'image/png', transparent: ld.transparent || false, attribution: ld.attr || '', maxZoom: ld.maxZoom || 18 });
                } else {
                    layer = L.tileLayer(ld.url, { maxZoom: ld.maxZoom || 19, attribution: ld.attr || '' });
                }
                _baseLayers[ld.name] = layer;
                var activate = savedLayerId ? (ld.id === savedLayerId) : ld.dflt;
                if (activate) { layer.addTo(_map); _currentLayerId = ld.id; }
            });
            // fallback al default se il layer salvato non esiste più
            if (!_currentLayerId) {
                var defLd = LAYERS.filter(function (l) { return l.dflt; })[0];
                if (defLd) { _baseLayers[defLd.name].addTo(_map); _currentLayerId = defLd.id; }
            }

            injectLayerControlCss();
            _lcControl = buildLayerControl(_map);
            _lcControl.addTo(_map);
            setTimeout(function () { if (_map) _map.invalidateSize(); }, 200);
        },

        setCustomLayers: function (layersJson) {
            if (!_map) return;
            var layers;
            try { layers = typeof layersJson === 'string' ? JSON.parse(layersJson) : layersJson; } catch (e) { return; }
            if (!layers || !layers.length) return;

            _customLayerDefs = [];
            layers.forEach(function (l) {
                var id = 'atak-' + l.id;
                var name = l.name;
                var tl;
                if (l.sourceType === 'WMS') {
                    tl = L.tileLayer.wms(l.url, {
                        layers: l.layers || '',
                        format: 'image/png',
                        transparent: true,
                        attribution: l.attribution || '',
                        minZoom: l.minZoom || 0,
                        maxZoom: l.maxZoom || 19
                    });
                } else {
                    tl = L.tileLayer(l.url, {
                        attribution: l.attribution || '',
                        minZoom: l.minZoom || 0,
                        maxZoom: l.maxZoom || 19
                    });
                }
                _baseLayers[name] = tl;
                _customLayerDefs.push({ id: id, name: name, cat: 'ATAK Maps' });
            });

            // Rebuild layer control to include new entries
            if (_lcControl) { _lcControl.remove(); }
            _lcControl = buildLayerControl(_map);
            _lcControl.addTo(_map);
        },

        updateMarkers: function (markersJson) {
            if (!_map) return;
            var markers = typeof markersJson === 'string' ? JSON.parse(markersJson) : markersJson;
            var seen = {};

            markers.forEach(function (m) {
                if (!m.uid || m.lat == null || m.lon == null) return;
                seen[m.uid] = true;

                var latlng = [m.lat, m.lon];
                var icon = createUserIcon(m.teamColor, m.isStale);
                var popup = '<b style="font-size:1rem">' + (m.callsign || m.uid) + '</b>' +
                    (m.team ? '<br><span style="color:' + (m.teamColor || '#888') + '">■ ' + m.team + '</span>' : '') +
                    (m.missionName ? '<br><span style="background:#0d6efd;color:#fff;font-size:10px;padding:1px 7px;border-radius:10px;font-weight:600">📋 ' + m.missionName + '</span>' : '') +
                    (m.battery != null ? '<br>🔋 ' + m.battery + '%' : '') +
                    '<br><span style="' + staleStyle(m.lastReportSec) + '">⏱ ' + formatLastSec(m.lastReportSec) + '</span>';

                if (_userMarkers[m.uid]) {
                    var ex = _userMarkers[m.uid];
                    ex.setLatLng(latlng);
                    ex.setIcon(icon);
                    ex.getPopup().setContent(popup);
                    ex.getTooltip().setContent(m.callsign || m.uid);
                } else {
                    var marker = L.marker(latlng, { icon: icon });
                    marker.bindPopup(popup);
                    marker.bindTooltip(m.callsign || m.uid, {
                        permanent: true,
                        direction: 'top',
                        offset: [0, -14],
                        className: 'takmap-label'
                    });
                    marker.addTo(_map);
                    _userMarkers[m.uid] = marker;
                }
            });

            Object.keys(_userMarkers).forEach(function (uid) {
                if (!seen[uid]) { _map.removeLayer(_userMarkers[uid]); delete _userMarkers[uid]; }
            });
        },

        updateMissionPoints: function (waypointsJson, parkingsJson) {
            if (!_map) return;

            _missionMarkers.forEach(function (m) { _map.removeLayer(m); });
            _missionMarkers = [];
            _milModifierLabels = [];

            var waypoints = typeof waypointsJson === 'string' ? JSON.parse(waypointsJson) : waypointsJson;
            var parkings  = typeof parkingsJson  === 'string' ? JSON.parse(parkingsJson)  : parkingsJson;

            (waypoints || []).forEach(function (w) {
                if (w.lat == null || w.lon == null) return;
                var isTactical = w.cotType && /^a-/.test(w.cotType);
                var icon = isTactical
                    ? (createMilSymIcon(w.sidc, w.cotType) || createWaypointIcon(w.color))
                    : createWaypointIcon(w.color);
                var popup;
                if (w.taskId) {
                    popup = '<div style="min-width:170px;font-family:inherit">' +
                        '<div style="font-weight:700;font-size:1rem;margin-bottom:2px">' + (w.name || 'WP') + '</div>' +
                        (w.taskName ? '<div style="font-size:0.8rem;color:#888;margin-bottom:8px">&#128203; ' + w.taskName + '</div>' : '') +
                        '<div style="display:flex;gap:5px;flex-wrap:wrap">' +
                        '<a href="/taskdetail/' + w.taskId + '" target="_blank" style="display:inline-block;padding:3px 10px;font-size:11px;font-weight:600;text-decoration:none;border-radius:4px;background:#0d6efd;color:#fff">&#127919; Operativo</a>' +
                        '<a href="/taskdetail/' + w.taskId + '/documents" target="_blank" style="display:inline-block;padding:3px 10px;font-size:11px;font-weight:600;text-decoration:none;border-radius:4px;background:#6c757d;color:#fff">&#128193; Documenti</a>' +
                        '</div></div>';
                } else {
                    popup = '<b>' + (w.name || 'WP') + '</b>';
                }
                var m = L.marker([w.lat, w.lon], { icon: icon });
                m.bindPopup(popup);
                var nameOffset = isTactical ? [0, -15] : [0, -8];
                m.bindTooltip(w.name || 'WP', { permanent: true, direction: 'top', offset: nameOffset, className: 'takmap-label' });
                m.addTo(_map);
                _missionMarkers.push(m);

                // Tooltip label cliccabile se il waypoint è collegato a un task
                if (w.taskId) {
                    (function (taskId) {
                        setTimeout(function () {
                            var tt = m.getTooltip();
                            if (!tt) return;
                            var el = tt.getElement();
                            if (!el) return;
                            el.style.pointerEvents = 'auto';
                            el.style.cursor = 'pointer';
                            el.title = 'Vai al task';
                            el.addEventListener('click', function (e) {
                                e.stopPropagation();
                                window.location.href = '/taskdetail/' + taskId;
                            });
                        }, 0);
                    })(w.taskId);
                }

                // Label MIL-2525 posizionate (solo per simboli tattici)
                if (isTactical && w.modifiers) {
                    addMilModifierLabels([w.lat, w.lon], w.modifiers, _missionMarkers);
                }
            });

            (parkings || []).forEach(function (p) {
                if (p.lat == null || p.lon == null) return;
                var m = L.marker([p.lat, p.lon], { icon: createParkingIcon(p.color) });
                m.bindPopup('<b>' + (p.name || 'Parcheggio') + '</b>');
                m.bindTooltip(p.name || 'Parking', { permanent: true, direction: 'top', offset: [0, -14], className: 'takmap-label' });
                m.addTo(_map);
                _missionMarkers.push(m);
            });
        },

        updateShapes: function (shapesJson) {
            if (!_map) return;
            _shapeMarkers.forEach(function (m) { _map.removeLayer(m); });
            _shapeMarkers = [];
            _shapeLabelMarkers = [];

            var shapes = typeof shapesJson === 'string' ? JSON.parse(shapesJson) : shapesJson;
            (shapes || []).forEach(function (s) {
                if (!s.points || s.points.length < 1) return;

                var dashArray = s.strokeStyle === 'dashed' ? '8,5'
                              : s.strokeStyle === 'dotted' ? '2,5' : null;
                var opts = {
                    color: s.strokeColor || '#ff0000',
                    weight: s.strokeWeight || 2,
                    dashArray: dashArray,
                    opacity: 0.9
                };

                var layer;
                if (s.shapeType === 'circle') {
                    var center = [s.points[0].lat, s.points[0].lon];
                    opts.radius = s.radius || 100;
                    opts.fillColor = s.fillColor || opts.color;
                    opts.fillOpacity = s.fillOpacity != null ? s.fillOpacity : 0;
                    opts.fill = opts.fillOpacity > 0;
                    layer = L.circle(center, opts);
                    layer.addTo(_map);
                    _shapeMarkers.push(layer);

                    var popupHtml = '<b>' + (s.name || '') + '</b><br><span style="font-size:10px;color:#888">⬤ ' + Math.round(s.radius || 0) + ' m</span>';
                    layer.bindPopup(popupHtml);

                    // Centro: dot colorato + label callsign permanente sopra (solo se cpvis=true)
                    if (s.showCenter) {
                        var centerDot = L.marker(center, { icon: createWaypointIcon(s.strokeColor || '#ff0000') });
                        centerDot.bindPopup(popupHtml);
                        if (s.name) {
                            centerDot.bindTooltip(s.name, { permanent: true, direction: 'top', offset: [0, -8], className: 'takmap-label' });
                        }
                        centerDot.addTo(_map);
                        _shapeMarkers.push(centerDot);
                    }

                    // Radius label a cavallo della circonferenza (punto sud) solo se labels_on=true
                    if (s.showLabel && s.radius) {
                        var dLat = s.radius / 111320;
                        var rHtml = '<div style="display:inline-block;transform:translate(-50%,-50%);background:rgba(0,0,0,0.65);color:#fff;font-size:11px;font-weight:600;padding:1px 5px;border-radius:3px;white-space:nowrap;line-height:1.4">' + Math.round(s.radius) + ' m</div>';
                        var rIcon = L.divIcon({ html: rHtml, className: '', iconSize: [0, 0], iconAnchor: [0, 0] });
                        var radiusMarker = L.marker([s.points[0].lat - dLat, s.points[0].lon], { icon: rIcon, interactive: false });
                        radiusMarker.addTo(_map);
                        _shapeMarkers.push(radiusMarker);
                        _shapeLabelMarkers.push(radiusMarker);
                    }
                } else {
                    if (s.points.length < 2) return;
                    var latlngs = s.points.map(function (p) { return [p.lat, p.lon]; });
                    var isFilled = s.shapeType === 'polygon' || s.shapeType === 'rectangle';
                    if (isFilled) {
                        opts.fillColor = s.fillColor || opts.color;
                        opts.fillOpacity = s.fillOpacity != null ? s.fillOpacity : (s.fillColor ? 0.15 : 0);
                        opts.fill = !!s.fillColor;
                        layer = L.polygon(latlngs, opts);
                    } else {
                        layer = L.polyline(latlngs, opts);
                    }
                    var popupLabel = isFilled ? (s.shapeType === 'rectangle' ? 'rettangolo' : 'poligono') : 'linea';
                    if (s.name) {
                        layer.bindPopup('<b>' + s.name + '</b><br><span style="font-size:10px;color:#888">' + popupLabel + '</span>');
                    }
                    layer.addTo(_map);
                    _shapeMarkers.push(layer);

                    // Label lunghezza su ogni segmento/lato, ruotata e centrata (tutte le forme se showLabel)
                    if (s.showLabel) {
                        var sides = latlngs.slice();
                        var closedShape = isFilled;
                        // rimuovi punto di chiusura se coincide col primo (forme chiuse)
                        if (closedShape && sides.length > 1 && sides[0][0] === sides[sides.length-1][0] && sides[0][1] === sides[sides.length-1][1])
                            sides = sides.slice(0, sides.length - 1);
                        var segCount = closedShape ? sides.length : sides.length - 1;
                        var R6 = 6378137;
                        for (var si = 0; si < segCount; si++) {
                            var sp1 = sides[si], sp2 = closedShape ? sides[(si + 1) % sides.length] : sides[si + 1];
                            var smLat = (sp1[0] + sp2[0]) / 2, smLon = (sp1[1] + sp2[1]) / 2;
                            var dφ = (sp2[0] - sp1[0]) * Math.PI / 180;
                            var dλ = (sp2[1] - sp1[1]) * Math.PI / 180;
                            var sa = Math.sin(dφ/2)*Math.sin(dφ/2) + Math.cos(sp1[0]*Math.PI/180)*Math.cos(sp2[0]*Math.PI/180)*Math.sin(dλ/2)*Math.sin(dλ/2);
                            var sLen = Math.round(R6 * 2 * Math.atan2(Math.sqrt(sa), Math.sqrt(1-sa)));
                            var sdx = (sp2[1] - sp1[1]) * Math.cos(smLat * Math.PI / 180);
                            var sdy = -(sp2[0] - sp1[0]);
                            var sAngle = Math.atan2(sdy, sdx) * 180 / Math.PI;
                            if (sAngle > 90) sAngle -= 180;
                            if (sAngle < -90) sAngle += 180;
                            var sHtml = '<div style="display:inline-block;transform:translate(-50%,-50%) rotate(' + sAngle.toFixed(1) + 'deg);background:rgba(0,0,0,0.65);color:#fff;font-size:11px;font-weight:600;padding:1px 5px;border-radius:3px;white-space:nowrap;line-height:1.4">' + sLen + ' m</div>';
                            var sIcon = L.divIcon({ html: sHtml, className: '', iconSize: [0, 0], iconAnchor: [0, 0] });
                            var sMarker = L.marker([smLat, smLon], { icon: sIcon, interactive: false });
                            sMarker.addTo(_map);
                            _shapeMarkers.push(sMarker);
                            _shapeLabelMarkers.push(sMarker);
                        }
                    }

                    // Centro: dot al centroide + label callsign (tutte le forme se cpvis=true)
                    if (s.showCenter && s.name) {
                        var sumLat = 0, sumLon = 0, n = latlngs.length;
                        for (var i = 0; i < n; i++) { sumLat += latlngs[i][0]; sumLon += latlngs[i][1]; }
                        var centroid = [sumLat / n, sumLon / n];
                        var dotColor = isFilled ? (s.fillColor || s.strokeColor || '#ff0000') : (s.strokeColor || '#ff0000');
                        var shapeDot = L.marker(centroid, { icon: createWaypointIcon(dotColor) });
                        shapeDot.bindTooltip(s.name, { permanent: true, direction: 'top', offset: [0, -8], className: 'takmap-label' });
                        shapeDot.bindPopup('<b>' + s.name + '</b><br><span style="font-size:10px;color:#888">' + popupLabel + '</span>');
                        shapeDot.addTo(_map);
                        _shapeMarkers.push(shapeDot);
                    }
                }
            });
            updateMilModifierVisibility();
        },

        updateRoutes: function (routesJson) {
            if (!_map) return;

            _routeMarkers.forEach(function (m) { _map.removeLayer(m); });
            _routeMarkers = [];
            _routeWpMarkers.forEach(function (m) { _map.removeLayer(m); });
            _routeWpMarkers = [];

            var routes = typeof routesJson === 'string' ? JSON.parse(routesJson) : routesJson;
            if (!routes || !routes.length) return;

            var pane = _map.getPane('routePane') ? 'routePane' : undefined;

            routes.forEach(function (route) {
                if (!route.waypoints || route.waypoints.length < 2) return;

                var clr    = route.color || '#ff0000';
                var opac   = (route.opacity !== undefined && route.opacity !== null) ? route.opacity : 1;
                var weight = Math.max(1.5, route.strokeWeight || 2);
                var style  = (route.strokeStyle || 'solid').toLowerCase();

                var dashArray = null;
                if (style === 'dashed')  dashArray = '10,6';
                if (style === 'dotted')  dashArray = '2,6';

                var polyOpts = {
                    color: clr, weight: weight, dashArray: dashArray,
                    opacity: opac, lineJoin: 'round', lineCap: 'round', pane: pane
                };

                if (style === 'outlined') {
                    // draw a wider dark outline first, then the colored line on top
                    var outline = L.polyline(route.waypoints.map(function (w) { return [w.lat, w.lon]; }), {
                        color: '#222', weight: weight + 4, opacity: Math.min(opac, 0.9),
                        lineJoin: 'round', lineCap: 'round', pane: pane
                    }).addTo(_map);
                    _routeMarkers.push(outline);
                }

                var line = L.polyline(route.waypoints.map(function (w) { return [w.lat, w.lon]; }), polyOpts).addTo(_map);
                _routeMarkers.push(line);

                var popHtml = '<div style="min-width:140px">'
                    + '<div style="font-weight:700;font-size:1rem;margin-bottom:4px">&#128739; ' + (route.name || 'Route') + '</div>';
                if (route.method)    popHtml += '<div style="font-size:12px">&#128663; ' + route.method + '</div>';
                if (route.direction) popHtml += '<div style="font-size:12px">&#8599; ' + route.direction + '</div>';
                if (route.routeType) popHtml += '<div style="font-size:12px;color:#888">' + route.routeType + '</div>';
                popHtml += '</div>';
                line.bindPopup(popHtml);

                // Named waypoints (b-m-p-w): all use route color, with tooltip
                route.waypoints.filter(function (w) { return w.isNamed; }).forEach(function (wp) {
                    var circle = L.circleMarker([wp.lat, wp.lon], {
                        radius: 6, color: '#fff', fillColor: clr, fillOpacity: opac, weight: 2
                    }).addTo(_map);
                    _routeWpMarkers.push(circle);

                    if (wp.name) {
                        circle.bindTooltip(wp.name, {
                            permanent: true, direction: 'top',
                            offset: [0, -8], className: 'takmap-label'
                        });
                    }
                    circle.bindPopup('<b>' + (wp.name || '') + '</b><br><small style="color:#888">&#128739; Waypoint</small>');
                });

                // Intermediate checkpoints (b-m-p-c): only render those with a label
                route.waypoints.filter(function (w) { return !w.isNamed && w.name; }).forEach(function (wp) {
                    var dot = L.circleMarker([wp.lat, wp.lon], {
                        radius: 3, color: clr, fillColor: clr, fillOpacity: opac, weight: 1
                    }).addTo(_map);
                    _routeWpMarkers.push(dot);
                    dot.bindTooltip(wp.name, {
                        permanent: true, direction: 'top',
                        offset: [0, -5], className: 'takmap-label'
                    });
                });
            });
        },

        fitToMarkers: function () {
            if (!_map) return;
            var pts = Object.values(_userMarkers).concat(_missionMarkers)
                .filter(function (m) { return !!m.getLatLng; })
                .map(function (m) { return m.getLatLng(); });
            _shapeMarkers.forEach(function (s) {
                try { var b = s.getBounds(); if (b.isValid()) { pts.push(b.getNorthWest()); pts.push(b.getSouthEast()); } } catch (e) {}
            });
            _routeMarkers.forEach(function (r) {
                try { var b = r.getBounds(); if (b.isValid()) { pts.push(b.getNorthWest()); pts.push(b.getSouthEast()); } } catch (e) {}
            });
            if (pts.length > 0) _map.fitBounds(L.latLngBounds(pts), { padding: [60, 60] });
        },

        dispose: function () {
            if (_map) { _map.remove(); _map = null; }
            _userMarkers = {};
            _missionMarkers = [];
            _shapeMarkers = [];
            _routeMarkers = [];
            _routeWpMarkers = [];
            _baseLayers = {};
        }
    };
})();
