// [log] - Goong Map JS helper cho Blazor interop
window.goongMap = (() => {
    let _map = null;
    let _marker = null;
    let _dotNetRef = null;

    function _isSdkReady() {
        return typeof goongjs !== 'undefined';
    }

    function _waitForSdk(timeoutMs = 5000) {
        return new Promise((resolve, reject) => {
            if (_isSdkReady()) { resolve(); return; }
            const start = Date.now();
            const interval = setInterval(() => {
                if (_isSdkReady()) {
                    clearInterval(interval);
                    resolve();
                } else if (Date.now() - start > timeoutMs) {
                    clearInterval(interval);
                    reject(new Error('Goong SDK chua load xong sau ' + timeoutMs + 'ms'));
                }
            }, 100);
        });
    }

    async function init(containerId, lat, lng, maptileKey, dotNetRef) {
        try {
            console.log('[log] - Bat dau khoi tao Goong Map, container:', containerId);

            if (_map) {
                _map.remove();
                _map = null;
                _marker = null;
                console.log('[log] - Da huy ban do cu');
            }

            _dotNetRef = dotNetRef;

            await _waitForSdk();
            console.log('[log] - Goong SDK san sang');

            const container = document.getElementById(containerId);
            if (!container) throw new Error('Khong tim thay container: #' + containerId);

            goongjs.accessToken = maptileKey;

            const centerLat = (lat && lat !== 0) ? lat : 10.7769;
            const centerLng = (lng && lng !== 0) ? lng : 106.7009;

            _map = new goongjs.Map({
                container: containerId,
                style: 'https://tiles.goong.io/assets/goong_map_web.json',
                center: [centerLng, centerLat],
                zoom: (lat !== 0 && lng !== 0) ? 14 : 11,
                attributionControl: false
            });

            _map.addControl(new goongjs.NavigationControl(), 'top-right');
            _map.addControl(new goongjs.GeolocateControl({
                positionOptions: { enableHighAccuracy: true },
                trackUserLocation: false
            }), 'top-right');

            _map.on('load', () => {
                console.log('[info] - Goong Map load xong');
                if (lat !== 0 && lng !== 0) _placeMarker(lat, lng);
            });

            _map.on('click', (e) => {
                const clickedLat = e.lngLat.lat;
                const clickedLng = e.lngLat.lng;
                console.log('[log] - Click toa do:', clickedLat, clickedLng);
                _placeMarker(clickedLat, clickedLng);
                _callbackBlazor(clickedLat, clickedLng);
            });

            _map.getCanvas().style.cursor = 'crosshair';
            console.log('[info] - Goong Map khoi tao thanh cong');
        } catch (err) {
            console.error('[error] - Khoi tao Goong Map that bai:', err.message);
            throw err;
        }
    }

    function _placeMarker(lat, lng) {
        if (_marker) _marker.remove();
        _marker = new goongjs.Marker({ color: '#E53935', draggable: true })
            .setLngLat([lng, lat])
            .addTo(_map);
        _marker.on('dragend', () => {
            const pos = _marker.getLngLat();
            console.log('[log] - Keo marker den:', pos.lat, pos.lng);
            _callbackBlazor(pos.lat, pos.lng);
        });
    }

    function _callbackBlazor(lat, lng) {
        if (_dotNetRef) {
            _dotNetRef.invokeMethodAsync('OnMapClick', lat, lng)
                .catch(err => console.error('[error] - Callback Blazor that bai:', err));
        }
    }

    function destroy() {
        try {
            if (_marker) { _marker.remove(); _marker = null; }
            if (_map) { _map.remove(); _map = null; }
            _dotNetRef = null;
            console.log('[log] - Goong Map da bi huy');
        } catch (e) {
            console.warn('[warn] - Loi khi huy map:', e.message);
        }
    }

    return { init, destroy };
})();
