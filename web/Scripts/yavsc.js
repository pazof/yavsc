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
  $("#message").removeClass("hidden");
  $("#message").text(msg);
  } else { $("#message").addClass("hidden"); } };


 self.onAjaxBadInput = function (data)
    {
		$.each(data.responseJSON, function (key, value) {
		var errspanid = "Err_cr_" + value.key.replace("model.","");
		var errspan = document.getElementById(errspanid);
		if (errspan==null)
			alert('enoent '+errspanid);
		else 
			errspan.innerHTML=value.errors.join("<br/>");
    	});

    }
self.onAjaxError = function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			Yavsc.message(xhr.status+" : "+xhr.responseText);
			    else Yavsc.message(false);
     }


return self;
})();
