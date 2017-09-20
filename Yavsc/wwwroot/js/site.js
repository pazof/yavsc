var notifClick =
    function(nid) {
        if (nid > 0) {
            $.get('/api/dimiss/click/' + nid).done(function() {})
                .fail(function() {})
                .always(function() {});
        }
    };