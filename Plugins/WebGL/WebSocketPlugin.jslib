var ProtonNetClientPlugin = {
    $ProtonLibrary: {   
        CreateWebSocket: function (id, url) {
            var ws = new WebSocket(url);
            ws.sp = {
                id: id,
            };

            ws.binaryType = "arraybuffer";

            ws.onopen = function (event) {
                console.log("[onopen] ws with id " + ws.sp.id + " is open.");
                try {
                    dynCall_vi(window.protonWebGL.onOpen, ws.sp.id);
                } catch (e) {
                    console.error("[onopen] Error in onOpen callback: ", e);
                }
            };

            ws.onclose = function (event) {
                console.log("[onclose] ws with id " + ws.sp.id + " is close with code: " + event.code + ", reason: " + event.reason);

                var bufferReasonData = ProtonLibrary.ToUTF8(event.reason || "");
                var wasClean = event.wasClean ? 1 : 0;
                try {
                    dynCall_viiii(window.protonWebGL.onClose, ws.sp.id, event.code, bufferReasonData, wasClean);
                } catch (e) {
                    console.error("[onclose] Error in onClose callback: ", e);
                } finally {
                    if (bufferReasonData) Module._free(bufferReasonData);
                }
            };

            ws.onerror = function (event) {
                var bufferData = ProtonLibrary.ToUTF8(JSON.stringify(event));
                try {
                    dynCall_vii(window.protonWebGL.onError, ws.sp.id, bufferData);
                } catch (e) {
                    console.error("[onerror] Error in onError callback: ", e);
                } finally {
                    if (bufferData) Module._free(bufferData);
                }
            };

            ws.onmessage = function (event) {
                var buffer = new Uint8Array(event.data);
                var bufferPtr = Module._malloc(buffer.length);

                if (!bufferPtr) {
                    console.error("[onmessage] Failed to allocate memory for bufferPtr.");
                    return;
                }

                Module.HEAPU8.set(buffer, bufferPtr);

                try {
                    dynCall_viii(window.protonWebGL.onMessage, ws.sp.id, bufferPtr, buffer.length);
                } catch (e) {
                    console.error("[onmessage] Error in onMessage callback: ", e);
                } finally {
                    Module._free(bufferPtr);
                }
            };

            return ws;
        },

        ToUTF8: function (text) {
            if (text === null || text === undefined) {
                return 0;
            }

            var length = lengthBytesUTF8(text) + 1;
            var buffer = Module._malloc(length);

            if (!buffer) {
                console.error("Failed to allocate memory for UTF-8 string.");
                return 0;
            }

            stringToUTF8(text, buffer, length);
            return buffer;
        }
    },

    Init: function (onOpen, onClose, onError, onMessage) {
		if (!window.protonWebGL) {
			window.protonWebGL = {
				onOpen: onOpen,
				onClose: onClose,
				onError: onError,
				onMessage: onMessage,
				wsInstances: {}
			};
		}
    },

    InitInstance: function (id, url) {
        if (!window.protonWebGL) {
            console.error("[InitInstance] are you missing Init()?");
            return false;
        }

        var wsInstance = {
            ws: null,
            url: UTF8ToString(url),
        };

        window.protonWebGL.wsInstances[id] = wsInstance;
		return true;
    },

    Connect: function (id) {
        var wsInstance = window.protonWebGL.wsInstances[id];
        if (!wsInstance) {
            console.error("[Connect] could not found wsInstance: " + id + ", are you missing InitInstance()?");
            return false;
        }

        wsInstance.ws = ProtonLibrary.CreateWebSocket(id, wsInstance.url);
        return true;
    },

    Send: function (id, bufferPtr, length) {
        var wsInstance = window.protonWebGL.wsInstances[id];
        if (!wsInstance) {
            console.error("[Send] could not found wsInstance: " + id + ", are you missing Connect()?");
            return false;
        }

        var ws = wsInstance.ws;
        if (!ws) {
            console.error("[Send] could not found ws in wsInstance: " + id + ", are you missing Connect()?");
            return false;
        }

        if (ws.readyState !== WebSocket.OPEN) {
            console.error("[Send] ws is not open in wsInstance: " + id + ", are you missing Connect()?");
            return false;
        }

        var buffer = new Uint8Array(Module.HEAPU8.buffer, bufferPtr, length);

        try {
            ws.send(buffer);
            return true;
        } catch (e) {
            console.error("[Send] Error sending data in wsInstance: " + id + ": error is " + e);
            return false;
        }
    },

    Disconnect: function (id) {
        var wsInstance = window.protonWebGL.wsInstances[id];
        if (!wsInstance) {
            console.error("[Disconnect] could not found wsInstance: " + id + ", are you missing Connect()?");
            return false;
        }

        var ws = wsInstance.ws;
        if (!ws) {
            console.error("[Disconnect] could not found ws in wsInstance: " + id + ", are you missing Connect()?");
            return false;
        }

        if (ws.readyState === WebSocket.CLOSED) {
            console.warn("[Disconnect] WebSocket is already closed: " + id);
            return false;
        }

        if (ws.readyState !== WebSocket.OPEN && ws.readyState !== WebSocket.CLOSING) {
            console.warn("[Disconnect] ws is not in open/closing state: " + id);
            return false;
        }

        ws.close();
        delete wsInstance.ws;
        wsInstance.ws = null;
        console.log("[Disconnect] WebSocket connection closed for id: " + id);

        return true;
    },

    IsConnected: function (id) {
        var wsInstance = window.protonWebGL.wsInstances[id];
        if (!wsInstance) {
            return false;
        }

        var ws = wsInstance.ws;
        if (!ws) {
            return false;
        }

        return ws.readyState === WebSocket.OPEN;
    }
};

autoAddDeps(ProtonNetClientPlugin, '$ProtonLibrary');
mergeInto(LibraryManager.library, ProtonNetClientPlugin);
