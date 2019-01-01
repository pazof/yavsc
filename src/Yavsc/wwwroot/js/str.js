var constraints = { audio: true, video: false }

navigator.mediaDevices.getUserMedia(constraints)
    .then(function(stream) {
        /* use the stream */
        console.log("got stream!");
        console.log(stream)
    })
    .catch(function(err) {
        /* handle the error */
        console.log(err)
    });