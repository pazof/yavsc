var Tags =  (function(apiBaseUrl){ 
var self = {};
self.blogsApiUrl = (apiBaseUrl || '/api') + "/Blogs";

self.tag = function (postid,tagname,callback) {
	$.ajax({
            url: CirclesApiUrl+"/Tag",
            type: "POST",
            data: { postid: postid, tag: tagname },
            success: function () { 
            	if (callback) callback(postid,tagname);
           },
            statusCode: {
            	400: Yavsc.onAjaxBadInput
            	},
            error: Yavsc.onAjaxError});
	}
self.untag = function (postid,tagname,callback) {
	$.ajax({
            url: CirclesApiUrl+"/Untag",
            type: "POST",
            data: { postid: postid, tag: tagname },
            success: function () { 
            	if (callback) callback(postid,tagname);
           },
            statusCode: {
            	400: Yavsc.onAjaxBadInput
            	},
            error: Yavsc.onAjaxError});
	}
return self;
})();
