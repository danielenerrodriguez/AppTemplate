window.chatDevice = {
    getDeviceId: function () {
        let deviceId = localStorage.getItem('chat-device-id');
        if (!deviceId) {
            // crypto.randomUUID() requires secure context (HTTPS or localhost)
            // Fall back to getRandomValues() for plain HTTP on LAN
            try {
                deviceId = crypto.randomUUID();
            } catch (e) {
                deviceId = Array.from(crypto.getRandomValues(new Uint8Array(16)))
                    .map(b => b.toString(16).padStart(2, '0')).join('');
            }
            localStorage.setItem('chat-device-id', deviceId);
        }
        return deviceId;
    },
    getModel: function () {
        return localStorage.getItem('chat-model') || '';
    },
    setModel: function (modelId) {
        if (modelId) {
            localStorage.setItem('chat-model', modelId);
        } else {
            localStorage.removeItem('chat-model');
        }
    }
};
