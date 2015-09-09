var Yavsc =  (function(apiBaseUrl){ 
var self = {};

self.apiBaseUrl = (apiBaseUrl || '/api');

self.showHide = function () { 
    var id = $(this).attr('did');
    var target = $('#'+id);
    if (target.hasClass('hidden')) {
	target.removeClass('hidden');
	if (typeof this.oldhtml == "undefined")
		this.oldhtml = $(this).html();
    $(this).html('['+this.oldhtml+']');
    }
    else {
	target.addClass('hidden');
	$(this).html(this.oldhtml);
    }
	};

self.message = function (msg) { 
  if (msg) { 
  $("#msg").removeClass("hidden");
  $("#msg").text(msg);
  } else { $("#msg").addClass("hidden"); } };


return self;
})();
