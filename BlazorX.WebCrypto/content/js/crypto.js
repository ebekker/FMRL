
// Useful references to WebCrypto:
//  * https://github.com/diafygi/webcrypto-examples

window._blazorWebCrypto = {
    // Useful references when interop-ing with TypedArrays:
    //  * https://developer.mozilla.org/en-US/docs/Web/JavaScript/Typed_arrays
    //  * https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/TypedArray
    getRandomValues: function (length) {
        var arr = crypto.getRandomValues(new Uint8Array(length));
        return Array.from(arr);
    },
    subtle_digest: async function (algor, data) {
        var arr = new Uint8Array(data);
        var dig = await crypto.subtle.digest(algor, arr);
        return Array.from(new Uint8Array(dig));
    },
    // Details about API and algorMap:
    //  * https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/generateKey
    //  * https://developer.mozilla.org/en-US/docs/Web/API/Web_Crypto_API/Supported_algorithms
    subtle_generateKey: async function (keyAlgorMap, usages) {
        var key = await crypto.subtle.generateKey(keyAlgorMap, true, usages);
        var keyExp = Array.from(new Uint8Array(await crypto.subtle.exportKey("raw", key)));
        return keyExp;
    },
    subtle_derive_key_pbkdf2: async function (keyData, salt, iterations, hash, keyAlgorMap, usages) {
        // Based on:
        //  * https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/deriveKey#Example
        var deriveMap = { name: "PBKDF2", salt: new Uint8Array(salt), iterations: iterations, hash: hash };
        var baseKey = await crypto.subtle.importKey("raw", new Uint8Array(keyData),
            { name: "PBKDF2" }, false, ["deriveBits", "deriveKey"]);
        var key = await crypto.subtle.deriveKey(deriveMap, baseKey, keyAlgorMap, true, usages);
        var keyExp = Array.from(new Uint8Array(await crypto.subtle.exportKey("raw", key)));
        return keyExp;
    },
    subtle_encrypt_aes_cbc: async function (iv, keyExp, keyAlgorMap, usages, decoded) {
        var key = await crypto.subtle.importKey("raw", new Uint8Array(keyExp), keyAlgorMap, true, usages);
        var ivArr = new Uint8Array(iv);
        var arr = new Uint8Array(decoded);
        var encoded = await crypto.subtle.encrypt({ name: "AES-CBC", iv: ivArr }, key, arr);
        return Array.from(new Uint8Array(encoded));
    },
    subtle_decrypt_aes_cbc: async function (iv, keyExp, keyAlgorMap, usages, encoded) {
        var key = await crypto.subtle.importKey("raw", new Uint8Array(keyExp), keyAlgorMap, true, usages);
        var ivArr = new Uint8Array(iv);
        var arr = new Uint8Array(encoded);
        var decoded = await crypto.subtle.decrypt({ name: "AES-CBC", iv: ivArr }, key, arr);
        return Array.from(new Uint8Array(decoded));
    }
};

// Polyfill of Array.from as specified in:
//    https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/from#Polyfill
// Production steps of ECMA-262, Edition 6, 22.1.2.1
if (!Array.from) {
    Array.from = (function () {
        var toStr = Object.prototype.toString;
        var isCallable = function (fn) {
            return typeof fn === 'function' || toStr.call(fn) === '[object Function]';
        };
        var toInteger = function (value) {
            var number = Number(value);
            if (isNaN(number)) { return 0; }
            if (number === 0 || !isFinite(number)) { return number; }
            return (number > 0 ? 1 : -1) * Math.floor(Math.abs(number));
        };
        var maxSafeInteger = Math.pow(2, 53) - 1;
        var toLength = function (value) {
            var len = toInteger(value);
            return Math.min(Math.max(len, 0), maxSafeInteger);
        };

        // The length property of the from method is 1.
        return function from(arrayLike/*, mapFn, thisArg */) {
            // 1. Let C be the this value.
            var C = this;

            // 2. Let items be ToObject(arrayLike).
            var items = Object(arrayLike);

            // 3. ReturnIfAbrupt(items).
            if (arrayLike === null) {
                throw new TypeError('Array.from requires an array-like object - not null or undefined');
            }

            // 4. If mapfn is undefined, then let mapping be false.
            var mapFn = arguments.length > 1 ? arguments[1] : void undefined;
            var T;
            if (typeof mapFn !== 'undefined') {
                // 5. else
                // 5. a If IsCallable(mapfn) is false, throw a TypeError exception.
                if (!isCallable(mapFn)) {
                    throw new TypeError('Array.from: when provided, the second argument must be a function');
                }

                // 5. b. If thisArg was supplied, let T be thisArg; else let T be undefined.
                if (arguments.length > 2) {
                    T = arguments[2];
                }
            }

            // 10. Let lenValue be Get(items, "length").
            // 11. Let len be ToLength(lenValue).
            var len = toLength(items.length);

            // 13. If IsConstructor(C) is true, then
            // 13. a. Let A be the result of calling the [[Construct]] internal method 
            // of C with an argument list containing the single item len.
            // 14. a. Else, Let A be ArrayCreate(len).
            var A = isCallable(C) ? Object(new C(len)) : new Array(len);

            // 16. Let k be 0.
            var k = 0;
            // 17. Repeat, while k < len… (also steps a - h)
            var kValue;
            while (k < len) {
                kValue = items[k];
                if (mapFn) {
                    A[k] = typeof T === 'undefined' ? mapFn(kValue, k) : mapFn.call(T, kValue, k);
                } else {
                    A[k] = kValue;
                }
                k += 1;
            }
            // 18. Let putStatus be Put(A, "length", len, true).
            A.length = len;
            // 20. Return A.
            return A;
        };
    }());
}
