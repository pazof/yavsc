// Write your Javascript code.

var allowCircleToBlog = function (e) {
    var allow = $(this).prop('checked');
    var circleid = $(this).data('circle-id');
    var targetid = $(this).data('target-id');
    var auth = { CircleId: circleid, BlogPostId: targetid };
    var url = '/api/blogacl';
    if (!allow) url+='/'+circleid;
    console.log(auth);
    $.ajax({ 
            url: url, 
            type: allow?'POST':'DELETE', 
            data: JSON.stringify(auth), 
            contentType: "application/json;charset=utf-8", 
            success: function (data) { 
                console.log('auth '+allow?'POSTed':'DELETEd'+' Successfully'); 
            }, 
            error: function () { 
                console.log('auth not '+allow?'POSTed':'DELETEd'); 
            } 
        });
    e.preventDefault();
};
$(document).ready(function(){
$('input.Blogcirle[type=checkbox]').on('change',allowCircleToBlog);
}
);

