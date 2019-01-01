var notifClick =
    function(nid) {
        if (nid > 0) {
            $.get('/api/dimiss/click/' + nid);
        }
    };

var setUiCult = function(lngspec) {
    document.cookie = 'ASPNET_CULTURE=c=' + lngspec + '|uic=' + lngspec;
    location.reload();
};