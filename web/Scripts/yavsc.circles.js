
var CirclesApiUrl = apiBaseUrl + "/Circle";

function editNewCircle() {
  if ($('#fncirc').hasClass('hidden')) $('#fncirc').removeClass('hidden')
	$('#lgdnvcirc').html("Creation d'un cercle");
	$('#fncirc').removeClass("dirty");
	$("#title").val( '' ); 
	$('#btnnewcircle').show();
	$('#btneditcircle').hide();
 }

function selectCircle() {
    if ($('#fncirc').hasClass('hidden')) $('#fncirc').removeClass('hidden')
	var id = $(this).attr('cid');
	$('#lgdnvcirc').html("Edition du cercle");
	$('#btnnewcircle').hide();
	$('#btneditcircle').show();
    // get it from the server

    $.getJSON(CirclesApiUrl+"/Get/"+id, function(json) { 
		$("#title").val( json.Title); });
	$('#id').val(id);
	$('#fncirc').removeClass("dirty");
}

function onCircleChanged() 
    { $('#fncirc').addClass("dirty"); }

function removeCircle() {
    	 var id = $(this).attr('cid'); 
    	 $.ajax({
            url: CirclesApiUrl+"/Delete/"+id,
            type: "GET",
            success: function (data) { 
            // Drops the row
            $("#c_"+id).remove();
           },
            statusCode: {
            	400: Yavsc.onAjaxBadInput,
            error: Yavsc.onAjaxError }});
    }

 function modifyCircle() {
 var id = $('#id').val();
 var circle = { title: $("#title").val(), id: id} ;
 $.ajax({
            url: CirclesApiUrl+"/Update",
            type: "POST",
            data: circle,
            success: function () { 
            	$('#c_'+id+' td:first-child').text(circle.title);
            }
            ,
            statusCode: {
            	400: Yavsc.onAjaxBadInput,
            	error: Yavsc.onAjaxError
            	}
            	});
 }
    
function addCircle()
    {
  	  var circle = { title: $("#title").val() } ;
    	 $("#title").text('');
    	 $.ajax({
            url: CirclesApiUrl+"/Create",
            type: "POST",
            data: circle,
            success: function (id) { 
            // Adds a node rendering the new circle
            $('<tr id="c_'+id+'"/>').addClass('selected row')
            .appendTo('#tbcb');

            $('<td>'+circle.title+'</td>').attr('cid',id).click(selectCircle)
            .appendTo('#c_'+id);

            $('<input type="button" value="Remove">').addClass("actionlink").attr('cid',id).click(removeCircle).appendTo('<td></td>').appendTo('#c_'+id);
         
           },
            statusCode: {
            	400: Yavsc.onAjaxBadInput
            	},
            error: Yavsc.onAjaxError});
    }


