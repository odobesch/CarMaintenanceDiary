window.getUserLocation = () =>
    new Promise((resolve, reject) => {
        navigator.geolocation.getCurrentPosition(
            pos => resolve({
                latitude: pos.coords.latitude,
                longitude: pos.coords.longitude
            }),
            err => reject(err),
            {
                enableHighAccuracy: true,   // 🔑 More precise
                timeout: 10000,
                maximumAge: 0
            }
        );
    });