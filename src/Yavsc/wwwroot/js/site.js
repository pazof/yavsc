// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
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
