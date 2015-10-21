var Tags =  (function(apiBaseUrl){ 
var self = {};
self.blogsApiUrl = (apiBaseUrl || '/api') + "/Blogs";

self.tag = function (postid,tagname,callback) {

	var data = {
           postid: postid,
           tag: tagname
          };
	$.ajax({
          url: '/api/Blogs/Tag/',
          type: 'POST',
          data: data,
          success: function() {
           if (callback) callback(postid,tagname);
          },
          statusCode: {
            	400: Yavsc.onAjaxBadInput
            	},
            error: Yavsc.onAjaxError
        });
	};
self.untag = function (postid,tagname,callback) {
	$.ajax({
            url: self.blogsApiUrl+"/Untag/"+postid,
            type: "POST",
            data: { postid: postid,  tag: tagname },
            success: function () { 
            	if (callback) callback(postid,tagname);
           },
            statusCode: {
            	400: Yavsc.onAjaxBadInput
            	},
            error: Yavsc.onAjaxError});
	};
	$(document).ready(function(){ 


	$('.tagname').click(function (){
	   var postid=$(this).parent().data('postid');
	   var $thistag = $(this);
	   // assert (postid); : all tagname class element 
	   // are related to a data-postid html attribute,
	   // found on their parent element
	   self.untag(postid, $thistag.text(), function () { $thistag.remove(); } );
	}
	);
	$('.taginput').autocomplete({
  	  minLength: 0,
      delay: 200,
      source:  function( request, response ) {
        $.ajax({
          url: "/api/Blogs/Tags",
          type: "POST",
          data: {
            pattern: request.term
          },
          success: function( data ) {
            response( data );
          }
        });
      }, 
      open: function() {
        $( this ).removeClass( "ui-corner-all" ).addClass( "ui-corner-top" );
      },
      close: function() {
        $( this ).removeClass( "ui-corner-top" ).addClass( "ui-corner-all" );
      }
	});	
	});

return self;
})();
