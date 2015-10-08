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
	self.dimiss = function () { 
		$(this).parent().remove();
	}
   	self.notice = function (msg, msgok) { 
   	if (!msgok) msgok='Ok';
   	if (msg) { 
   	var note = $('<div class="notification">'+msg+'<br></div>');
   	$('<a class="actionlink"><i class="fa fa-check">'+msgok+'</i></a>').click(self.dimiss).appendTo(note);
   	note.appendTo("#notifications");
  	}  };


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
        			Yavsc.notice(xhr.status+" : "+xhr.responseText);
			    else Yavsc.notice(false);
     }
return self;
})();

