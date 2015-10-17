var Yavsc =  (function(apiBaseUrl){ 
var self = {};

function dumpprops(obj) { 
var str = "";
for(var k in obj)
    if (obj.hasOwnProperty(k)) 
        str += k + " = " + obj[k] + "\n";
      return (str);   }

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
	};

self.notice = function (msg, msgok) { 
   	if (!msgok) msgok='Ok';
   	if (msg) { 
   	var note = $('<div class="notification">'+msg+'<br></div>');
   	$('<a class="actionlink"><i class="fa fa-check">'+msgok+'</i></a>').click(self.dimiss).appendTo(note);
   	note.appendTo("#notifications");
  	}  };


 self.onAjaxBadInput = function (data)
    {
    	if (!data) { Yavsc.notice('no data'); return; }
    	if (!data.responseJSON) { Yavsc.notice('no json data:'+data); return; }
   		if (!Array.isArray(data.responseJSON))  { Yavsc.notice('Bad Input: '+data.responseJSON); return; }
		$.each(data.responseJSON, function (key, value) {
		var errspanid = "Err_" + value.key;
		var errspan = document.getElementById(errspanid);
		if (errspan==null)
			alert('enoent '+errspanid);
		else 
			errspan.innerHTML=value.errors.join("<br/>");
    	});

    };

self.onAjaxError = function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			Yavsc.notice(xhr.status+" : "+xhr.responseText);
     };

return self;
})();

$(document).ready(function(){

$body = $("body");
$(document).on({
    ajaxStart: function() { $body.addClass("loading");    },
    ajaxStop: function() { $body.removeClass("loading"); }    
});

	var $window = $(window);
	$(window).scroll(function() {
		var $ns = $('#notifications');
		if ($ns.has('*').length>0) {
		if ($window.scrollTop()>375) { 
			$ns.css('position','fixed');
			$ns.css('z-index',2);
			$ns.css('top',0);
		}
		else {  
			$ns.css('position','static');
			$ns.css('z-index',1); 
		}}
	});
});
