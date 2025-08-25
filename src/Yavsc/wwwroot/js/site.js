
var notifClick =
    function(nid) {
        if (nid > 0) {
            $.get({
                url: '/api/dimiss/click/' + nid,
                success: $('div[data-nid='+nid+']').remove()
            });
        }
    };

var setUiCult = function(lngspec) {
    document.cookie = 'ASPNET_CULTURE=c=' + lngspec + '|uic=' + lngspec;
    location.reload();
};

