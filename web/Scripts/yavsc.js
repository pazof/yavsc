
var Yavsc =  (function(apiBaseUrl){ 
var self = {};

self.apiBaseUrl = (apiBaseUrl || '/api');

self.dumpprops = function (obj) { 
var str = "";
for(var k in obj)
   if (obj.hasOwnProperty(k)) 
        str += "."+k + " = " + obj[k] + "\n";
return str;}
self.logObj = function(obj) {
	console.log('obj:'+obj);
	console.log(self.dumpprops(obj));
};

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

self.ajax = function (method,data,callback,badInputCallback,errorCallBack) {
if (!badInputCallback) badInputCallback=Yavsc.onAjaxBadInput;
if (!errorCallBack) errorCallBack=Yavsc.onAjaxError;
	$.ajax({
            url: self.apiBaseUrl+'/'+method,
            type: "POST",
            data: data,
            success: function (response) { 
            	if (callback) callback(response);
           },
            statusCode: {
            	400: Yavsc.onAjaxBadInput
            	},
            error: Yavsc.onAjaxError});
	};

self.onScroll = function() {
	var $notifications = $('#notifications');
		if ($notifications.has('*').length>0) {
		if ($(window).scrollTop()>0) { 
			$notifications.addClass("dispmodal");
		}
		else {  
			$notifications.removeClass("dispmodal");
		}}
	};

self.notice = function (msg, callback, msgok) { 
   	if (!msgok) msgok='Ok';
   	var note = $('<div class="notification">'+msg+'<br></div>');
   	var btn = $('<a class="actionlink"><i class="fa fa-check">'+msgok+'</i></a>');
   	if (callback) btn.click(callback);  
   	btn.click(self.dimiss).appendTo(note);
   	note.appendTo("#notifications");
   	self.onScroll();
  };

self.onAjaxBadInput = function (data)
    {
    	if (!data) { Yavsc.notice('Bad input, no data'); return; }
    	if (!data.responseJSON) { Yavsc.notice('Bad input, no json data response'); return; }
   		if (!Array.isArray(data.responseJSON))  { Yavsc.notice('Bad Input: '+data.responseJSON); return; }
		$.each(data.responseJSON, function (key, value) {
		var errspanid = "Err_" + value.key;
		var errspan = document.getElementById(errspanid);
		if (errspan==null)
			Yavsc.notice('ENOTANODE: '+errspanid);
		else 
			errspan.innerHTML=value.errors.join("<br/>");
    	});
    };

self.onAjaxError = function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			Yavsc.notice(xhr.status+" : "+xhr.responseText);
     };

$(document).ready(function(){

$('.maskable').each( function() {
var $mobj = $(this);
var $btnshow = $('#'+$mobj.data('btn-show'));
var $btnhide = $('#'+$mobj.data('btn-hide'));
var onClickHide = function(){
$mobj.addClass('hidden');
$btnshow.removeClass('hidden');
$btnhide.addClass('hidden');
};
$btnhide.click(onClickHide);
onClickHide();
$btnshow.click(function(){
$mobj.removeClass('hidden');
$btnshow.addClass('hidden');
$btnhide.removeClass('hidden');
});
});
});

$(document).ready(function(){
	$body = $("body");
	$(document).on({
	    ajaxStart: function() { $body.addClass("loading");    },
	    ajaxStop: function() { $body.removeClass("loading"); }    
	});
	$(window).scroll(self.onScroll);
});
return self;
})();

