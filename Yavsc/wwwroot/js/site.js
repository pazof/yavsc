+(function($) {
    function notifClick(nid) {
        if (nid > 0) { 
            $.get('/api/dimiss/click/' + nid).done(function() {})
                .fail(function() {})
                .always(function() {});
        }
    }
})(jQuery);