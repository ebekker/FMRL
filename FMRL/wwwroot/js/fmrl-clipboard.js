// Defines a routine enable a copy button
// with a target text field for selection
window._fmrl = {
    enableCopyButton: function (txt, btn) {
        btn.addEventListener("click", function () {
            txt.select();
            document.execCommand("copy");
        });
        return 0;
    }
};
