// Setup some state to coordinate the ready
// status of the JS spinner and the.NET runtime
window._blazorXstartup = {
    jsReady: false,
    dnReady: false,
    dnSetReady: function () {
        window._blazorXstartup.dnReady = true;
        return false;
    },
    pollForReady: function () {
        console.log("Checking for ready!");
        if (window._blazorXstartup.jsReady && window._blazorXstartup.dnReady) {
            console.log("Both are ready!");
            document.getElementById("blazorXstartupSpinner").remove();
            document.getElementById("blazorXstartupApp").style.display = "";
            return;
        }

        // Keep running it till we're done
        window.setTimeout(window._blazorXstartup.pollForReady, 500);
    }
};
// Install a polling function to check the coordinated status
window.setTimeout(window._blazorXstartup.pollForReady, 500);
