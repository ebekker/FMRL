/*
 * From Effect13 from Moving Letters by Tobias Ahlin:
 *   http://tobiasahlin.com/moving-letters/#13
 */

// Wrap every letter in a span
$('.ml13').each(function () {
    $(this).html($(this).text().replace(/([^\x00-\x80]|\w)/g, "<span class='letter'>$&</span>"));
});

anime.timeline({ loop: false })
    .add({
        targets: '.ml13 .letter',
        translateY: [100, 0],
        translateZ: 0,
        opacity: [0, 1],
        easing: "easeOutExpo",
        duration: 500,
        delay: function (el, i) {
            return 300 + 30 * i * 3;
        }
    }).add({
        targets: '.ml13 .letter',
        translateY: [0, -100],
        opacity: [1, 0],
        easing: "easeInExpo",
        duration: 500,
        delay: function (el, i) {
            return 100 + 30 * i * 2;
        },
        complete: function (anim) {
            window._blazorXstartup.jsReady = true;
            console.log("ready2run is go");
        }
    });
