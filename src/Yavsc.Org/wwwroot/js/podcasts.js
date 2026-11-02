document.querySelectorAll("[data-podcast-player]").forEach(player => {
    const warning = player.parentElement.querySelector("[data-playback-error]");
    const showError = () => warning.classList.remove("d-none");
    const source = player.querySelector("source");
    if (player.dataset.hls === "true" && !player.canPlayType("application/vnd.apple.mpegurl")) {
        if (typeof Hls === "undefined" || !Hls.isSupported()) {
            showError();
            return;
        }
        const url = source.src;
        source.remove();
        const start = player.parentElement.querySelector("[data-start-playback]");
        start.classList.remove("d-none");
        start.addEventListener("click", () => {
            start.classList.add("d-none");
            const hls = new Hls();
            hls.on(Hls.Events.ERROR, (_event, data) => {
                if (data.fatal) {
                    showError();
                    hls.destroy();
                }
            });
            hls.loadSource(url);
            hls.attachMedia(player);
            player.play().catch(showError);
            window.addEventListener("pagehide", () => hls.destroy(), { once: true });
        }, { once: true });
    } else {
        source.addEventListener("error", showError);
    }
    player.addEventListener("error", showError);
});
