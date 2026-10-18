
var notifClick =
    function(nid) {
        if (nid > 0) {
            $.post({
                url: '/Home/DismissNotification/' + nid,
                success: $('div[data-nid='+nid+']').remove()
            });
        }
    };

var setUiCult = function(lngspec) {
    document.cookie = 'ASPNETCORE_CULTURE=c=' + lngspec + '|uic=' + lngspec;
    location.reload();
};

